using SimpleToDo.services.database.interfaces;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace SimpleToDo.services.database
{
  /// <summary>
  /// 複数のデータベースに対応したCRUD操作を提供するマネージャクラス。
  /// データベースの作成、テーブル作成、レコードの追加・取得・更新・削除を汎用的に行う。
  /// </summary>
  /// <remarks>
  /// DatabaseCrudManagerのコンストラクタ
  /// </remarks>
  /// <param name="connectionFactory">データベース接続を生成するファクトリ関数</param>
  /// <exception cref="ArgumentNullException">connectionFactoryがnullの場合</exception>
  public class DatabaseCrudManager(Func<Task<IDbConnectionWrapper>> connectionFactory)
  {
        // データベース接続用のファクトリ関数
        private readonly Func<Task<IDbConnectionWrapper>> _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        
        // SQLクエリキャッシュ - 同じ型のクエリを再生成しないようにする
        private static readonly Dictionary<Type, Dictionary<string, string>> _queryCache = new();
        
        // スレッドセーフなロックオブジェクト
        private static readonly object _cacheLock = new();

    /// <summary>
    /// データベースを作成します（DB種別ごとにSQLを切り替え）。
    /// </summary>
    /// <param name="databaseName">作成するデータベース名</param>
    /// <exception cref="ArgumentNullException">databaseNameがnullまたは空の場合</exception>
    /// <exception cref="NotSupportedException">サポートされていないデータベース種別の場合</exception>
    public async Task CreateDatabaseAsync(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentNullException(nameof(databaseName), "データベース名が指定されていません");

            using var connection = await _connectionFactory();
            await connection.OpenAsync();

            string databaseType = GetDatabaseType(connection);

            // データベース種別ごとに作成クエリを切り替え
            string query = databaseType switch
            {
                "mysqlconnection" => $"CREATE DATABASE IF NOT EXISTS {databaseName};",
                "npgsqlconnection" => $"CREATE DATABASE IF NOT EXISTS {databaseName};",
                "sqlconnection" => $"IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = '{databaseName}') CREATE DATABASE [{databaseName}];",
                "sqliteconnection" => throw new NotSupportedException("SQLiteは複数データベースの作成をサポートしていません"),
                _ => throw new NotSupportedException($"サポートされていないデータベース種別です: {databaseType}")
            };

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = query;
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex) when (ex is not NotSupportedException)
            {
                throw new InvalidOperationException($"データベース '{databaseName}' の作成に失敗しました", ex);
            }
        }

        /// <summary>
        /// 指定したデータベースにテーブルを作成します。
        /// </summary>
        /// <param name="databaseName">データベース名</param>
        /// <param name="tableName">作成するテーブル名</param>
        /// <param name="columnDefinitions">カラム定義の配列</param>
        /// <exception cref="ArgumentNullException">引数がnullの場合</exception>
        public async Task CreateTableAsync(string databaseName, string tableName, string[] columnDefinitions)
        {
            ValidateStringArgument(databaseName, nameof(databaseName));
            ValidateStringArgument(tableName, nameof(tableName));
            
            if (columnDefinitions == null || columnDefinitions.Length == 0)
                throw new ArgumentNullException(nameof(columnDefinitions), "カラム定義が指定されていません");

            using var connection = await _connectionFactory();
            await connection.OpenAsync();
            connection.ChangeDatabase(databaseName);

            string query = $"CREATE TABLE IF NOT EXISTS {tableName} (id INT AUTO_INCREMENT PRIMARY KEY, {string.Join(", ", columnDefinitions)});";
            
            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = query;
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"テーブル '{tableName}' の作成に失敗しました", ex);
            }
        }

        /// <summary>
        /// レコードを挿入し、挿入されたIDを返します。
        /// </summary>
        /// <typeparam name="T">挿入するレコードの型</typeparam>
        /// <param name="databaseName">データベース名</param>
        /// <param name="tableName">テーブル名</param>
        /// <param name="record">挿入するレコード</param>
        /// <returns>挿入されたレコードのID</returns>
        /// <exception cref="ArgumentNullException">引数がnullの場合</exception>
        public async Task<long> CreateRecordAsync<T>(string databaseName, string tableName, T record)
        {
            ValidateStringArgument(databaseName, nameof(databaseName));
            ValidateStringArgument(tableName, nameof(tableName));
            
            if (record == null)
                throw new ArgumentNullException(nameof(record), "レコードがnullです");

            using var connection = await _connectionFactory();
            await connection.OpenAsync();
            connection.ChangeDatabase(databaseName);

            Type recordType = record.GetType();
            
            // キャッシュからクエリを取得、またはビルド
            (string query, PropertyInfo[] properties) = GetInsertQuery(recordType, tableName);

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = query;

                // プロパティ値をパラメータとして追加
                AddParametersToCommand(command, properties, record);

                await command.ExecuteNonQueryAsync();

                // 最後に挿入されたIDを取得
                command.CommandText = "SELECT LAST_INSERT_ID()";
                var result = await command.ExecuteScalarAsync();
                return result != null ? Convert.ToInt64(result) : 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"テーブル '{tableName}' へのレコード挿入に失敗しました", ex);
            }
        }

        /// <summary>
        /// テーブル内の全レコードをDataTableとして取得します。
        /// </summary>
        /// <param name="databaseName">データベース名</param>
        /// <param name="tableName">テーブル名</param>
        /// <returns>取得したレコードを含むDataTable</returns>
        public async Task<DataTable> ReadAllRecordAsync(string databaseName, string tableName)
        {
            ValidateStringArgument(databaseName, nameof(databaseName));
            ValidateStringArgument(tableName, nameof(tableName));

            using var connection = await _connectionFactory();
            await connection.OpenAsync();
            connection.ChangeDatabase(databaseName);

            var dataTable = new DataTable(tableName);
            
            try
            {
                using var command = connection.CreateCommand();
                string query = $"SELECT * FROM {tableName};";
                command.CommandText = query;

                using var reader = await command.ExecuteReaderAsync();
                dataTable.Load(reader);
                
                return dataTable;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"テーブル '{tableName}' からのデータ取得に失敗しました", ex);
            }
        }

        /// <summary>
        /// 指定したIDのレコードをDataTableとして取得します。
        /// </summary>
        /// <param name="databaseName">データベース名</param>
        /// <param name="tableName">テーブル名</param>
        /// <param name="id">取得するレコードのID</param>
        /// <returns>指定IDのレコードを含むDataTable</returns>
        public async Task<DataTable> ReadRecordByIdAsync(string databaseName, string tableName, long id)
        {
            ValidateStringArgument(databaseName, nameof(databaseName));
            ValidateStringArgument(tableName, nameof(tableName));

            using var connection = await _connectionFactory();
            await connection.OpenAsync();
            connection.ChangeDatabase(databaseName);

            var dataTable = new DataTable(tableName);
            
            try
            {
                using var command = connection.CreateCommand();
                string query = $"SELECT * FROM {tableName} WHERE id = @Id";
                command.CommandText = query;

                var idParameter = command.CreateParameter();
                idParameter.ParameterName = "@Id";
                idParameter.Value = id;
                command.Parameters.Add(idParameter);

                using var reader = await command.ExecuteReaderAsync();
                dataTable.Load(reader);
                
                return dataTable;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"テーブル '{tableName}' からのID={id}のレコード取得に失敗しました", ex);
            }
        }

        /// <summary>
        /// 指定IDのレコードを更新します。
        /// </summary>
        /// <typeparam name="T">更新するレコードの型</typeparam>
        /// <param name="databaseName">データベース名</param>
        /// <param name="tableName">テーブル名</param>
        /// <param name="id">更新するレコードのID</param>
        /// <param name="record">更新内容を含むレコード</param>
        /// <exception cref="ArgumentNullException">引数がnullの場合</exception>
        public async Task UpdateRecordAsync<T>(string databaseName, string tableName, long id, T record)
        {
            ValidateStringArgument(databaseName, nameof(databaseName));
            ValidateStringArgument(tableName, nameof(tableName));
            
            if (record == null)
                throw new ArgumentNullException(nameof(record), "レコードがnullです");

            using var connection = await _connectionFactory();
            await connection.OpenAsync();
            connection.ChangeDatabase(databaseName);

            Type recordType = record.GetType();
            
            // キャッシュからクエリを取得、またはビルド
            (string query, PropertyInfo[] properties) = GetUpdateQuery(recordType, tableName);
            
            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = query;

                var idParameter = command.CreateParameter();
                idParameter.ParameterName = "@Id";
                idParameter.Value = id;
                command.Parameters.Add(idParameter);

                // プロパティ値をパラメータとして追加
                AddParametersToCommand(command, properties, record);

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"テーブル '{tableName}' のID={id}のレコード更新に失敗しました", ex);
            }
        }

        /// <summary>
        /// 指定IDのレコードを削除します。
        /// </summary>
        /// <param name="databaseName">データベース名</param>
        /// <param name="tableName">テーブル名</param>
        /// <param name="id">削除するレコードのID</param>
        public async Task DeleteRecordAsync(string databaseName, string tableName, long id)
        {
            ValidateStringArgument(databaseName, nameof(databaseName));
            ValidateStringArgument(tableName, nameof(tableName));

            using var connection = await _connectionFactory();
            await connection.OpenAsync();
            connection.ChangeDatabase(databaseName);

            try
            {
                using var command = connection.CreateCommand();
                string query = $"DELETE FROM {tableName} WHERE id = @id";
                command.CommandText = query;

                var idParameter = command.CreateParameter();
                idParameter.ParameterName = "@id";
                idParameter.Value = id;
                command.Parameters.Add(idParameter);

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"テーブル '{tableName}' のID={id}のレコード削除に失敗しました", ex);
            }
        }

        #region ヘルパーメソッド

        /// <summary>
        /// データベースの種類を取得します。
        /// </summary>
        private static string GetDatabaseType(IDbConnectionWrapper connection) 
        {
            try
            {
                return (connection as dynamic).InnerConnectionTypeName?.ToLower() ?? 
                       connection.GetType().Name.ToLower();
            }
            catch
            {
                return connection.GetType().Name.ToLower();
            }
        }

        /// <summary>
        /// 文字列引数が有効（nullでも空でもない）であることを検証します。
        /// </summary>
        private static void ValidateStringArgument(string argument, string paramName)
        {
            if (string.IsNullOrWhiteSpace(argument))
                throw new ArgumentNullException(paramName, $"{paramName}がnullまたは空です");
        }

        /// <summary>
        /// INSERT クエリとそのプロパティ情報を取得します（キャッシュ対応）。
        /// </summary>
        private static (string Query, PropertyInfo[] Properties) GetInsertQuery(Type recordType, string tableName)
        {
            string cacheKey = $"INSERT_{tableName}";
            
            return GetOrCreateCachedQuery(recordType, cacheKey, properties =>
            {
                var columnNames = string.Join(", ", properties.Select(param => param.Name.ToLower()));
                var parameterNames = string.Join(", ", properties.Select(p => "@" + p.Name));
                return $"INSERT INTO {tableName} ({columnNames}) VALUES ({parameterNames})";
            });
        }

        /// <summary>
        /// UPDATE クエリとそのプロパティ情報を取得します（キャッシュ対応）。
        /// </summary>
        private static (string Query, PropertyInfo[] Properties) GetUpdateQuery(Type recordType, string tableName)
        {
            string cacheKey = $"UPDATE_{tableName}";
            
            return GetOrCreateCachedQuery(recordType, cacheKey, properties =>
            {
                var setClause = string.Join(", ", properties.Select(p => p.Name.ToLower() + " = " + "@" + p.Name));
                return $"UPDATE {tableName} SET {setClause} WHERE id = @Id";
            });
        }

        /// <summary>
        /// キャッシュからクエリを取得するか、新たに生成してキャッシュに追加します。
        /// </summary>
        private static (string Query, PropertyInfo[] Properties) GetOrCreateCachedQuery(
            Type recordType, string cacheKey, Func<PropertyInfo[], string> queryBuilder)
        {
            // ID以外のプロパティを取得（ロック外で実行可能）
            var properties = recordType.GetProperties()
                .Where(p => !p.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            
            // 1つのロックブロックで処理
            lock (_cacheLock)
            {
                if (!_queryCache.TryGetValue(recordType, out var typeCache))
                {
                    typeCache = [];
                    _queryCache[recordType] = typeCache;
                }
                
                if (!typeCache.TryGetValue(cacheKey, out string? query))
                {
                    query = queryBuilder(properties) ?? string.Empty;
                    typeCache[cacheKey] = query;
                }

                return (query, properties);
            }
        }

        /// <summary>
        /// コマンドにパラメータを追加します。
        /// </summary>
        private static void AddParametersToCommand<T>(IDbCommandWrapper command, PropertyInfo[] properties, T record)
        {
            foreach (var property in properties)
            {
                try
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@" + property.Name;
                    
                    // null値はDBNullに変換
                    var value = property.GetValue(record);
                    parameter.Value = value ?? DBNull.Value;
                    
                    command.Parameters.Add(parameter);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"パラメータ {property.Name} の追加でエラー: {ex.Message}");
                    throw new InvalidOperationException($"パラメータの設定中にエラーが発生しました: {property.Name}", ex);
                }
            }
        }

        #endregion
    }
}