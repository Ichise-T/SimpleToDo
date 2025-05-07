using System.Data;
using System.Diagnostics;

namespace SimpleToDo.services
{
    public class DatabaseCrudManager(Func<IDbConnection> connectionFactory)
    {
        private readonly Func<IDbConnection> _connectionFactory = connectionFactory;

        /// <summary>
        /// 指定した名前でデータベースを作成します。
        /// データベースの種類（MySQL, PostgreSQL, SQL Server, SQLite）に応じて適切なSQLを実行します。
        /// </summary>
        /// <param name="databaseName">作成するデータベース名</param>
        public void CreateDatabase(string databaseName)
        {
            using var connection = _connectionFactory();
            connection.Open();

            // データベースの種類を判別
            string databaseType = connection.GetType().Name.ToLower();

            // データベースごとのクエリを生成
            string query = databaseType switch
            {
                "mysqlconnection" => $"CREATE DATABASE IF NOT EXISTS {databaseName};",
                "npgsqlconnection" => $"CREATE DATABASE {databaseName};",
                "sqlconnection" => $"IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = {databaseName}) CREATE DATABASE [{databaseName}];",
                "sqliteconnection" => throw new NotSupportedException("SQLite does not support creating multiple databases."),
                _ => throw new NotSupportedException($"Unsupported database type: {databaseType}")
            };

            using var command = connection.CreateCommand();
            command.CommandText = query;
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// 指定したデータベースにテーブルを作成します。
        /// </summary>
        /// <param name="databaseName">データベース名</param>
        /// <param name="tableName">テーブル名</param>
        /// <param name="columnDefinitions">カラム定義の配列（例: "name VARCHAR(100)"）</param>
        public void CreateTable(string databaseName, string tableName, string[] columnDefinitions)
        {
            using var connection = _connectionFactory();
            connection.Open(); 
            connection.ChangeDatabase(databaseName);           

            // テーブル作成のSQLクエリを生成
            string query = $"CREATE TABLE IF NOT EXISTS {tableName} (id INT AUTO_INCREMENT PRIMARY KEY, {string.Join(", ", columnDefinitions)});";
            using var command = connection.CreateCommand();
            command.CommandText = query;

            // SQLクエリを実行
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// 指定したテーブルにレコードを挿入し、挿入されたレコードのIDを返します。
        /// </summary>
        /// <typeparam name="T">レコードの型</typeparam>
        /// <param name="databaseName">データベース名</param>
        /// <param name="tableName">テーブル名</param>
        /// <param name="record">挿入するレコードオブジェクト</param>
        /// <returns>挿入されたレコードのID</returns>
        public long CreateRecord<T>(string databaseName, string tableName, T record)
        {
            using var connection = _connectionFactory();
            connection.Open();
            connection.ChangeDatabase(databaseName);

            // ジェネリック型Tのプロパティを取得
            var recordType = record?.GetType();
            var properties = recordType?.GetProperties()
                .Where(p => p.Name != "Id") // Idプロパティを除外  
                .ToArray();
            properties ??= [];

            // プロパティからカラム名のカンマ区切りリストを作成
            var columnNames = string.Join(", ", properties.Select(parameter => parameter.Name));
            // SQLクエリのパラメータ名のカンマ区切りリストを作成
            var parameterNames = string.Join(", ", properties.Select(p => "@" + p.Name));

            using var command = connection.CreateCommand();
            string query = $"INSERT INTO {tableName} ({columnNames}) VALUES ({parameterNames})";
            command.CommandText = query;

            AddParametersToCommand(command, properties, record);

            command.ExecuteNonQuery();

            command.CommandText = "SELECT LAST_INSERT_ID()";

            // 最後に挿入されたレコードのIDを取得
            return Convert.ToInt64(command.ExecuteScalar());
        }

        /// <summary>
        /// 指定したデータベース・テーブルの全レコードをDataTableとして取得します。
        /// </summary>
        /// <param name="databaseName">データベース名</param>
        /// <param name="tableName">テーブル名</param>
        /// <returns>全レコードを格納したDataTable</returns>
        public DataTable ReadAllRecord(string databaseName, string tableName)
        {
            // データベース接続を開く
            using var connection = _connectionFactory();
            connection.Open();
            connection.ChangeDatabase(databaseName);

            // IDbCommandを作成
            using var command = connection.CreateCommand();
            string query = $"SELECT * FROM {tableName};";
            command.CommandText = query;

            // ExecuteReaderでデータを取得
            using var reader = command.ExecuteReader();

            // DataTableにデータを読み込む
            DataTable dataTable = new();
            dataTable.Load(reader);

            return dataTable;
        }

        /// <summary>
        /// 指定したIDのレコードをDataTableとして取得します。
        /// </summary>
        /// <param name="databaseName">データベース名</param>
        /// <param name="tableName">テーブル名</param>
        /// <param name="id">レコードID</param>
        /// <returns>該当レコードを格納したDataTable</returns>
        public DataTable ReadRecordById(string databaseName, string tableName, long id)
        {
            using var connection = _connectionFactory();            
            connection.Open();
            connection.ChangeDatabase(databaseName);

            using var command = connection.CreateCommand();
            string query = $"SELECT * FROM {tableName} WHERE Id = @Id";
            command.CommandText = query;

            var idParameter = command.CreateParameter();
            idParameter.ParameterName = "@Id";
            idParameter.Value = id;
            command.Parameters.Add(idParameter);

            using var reader = command.ExecuteReader();

            // DataTableにデータを読み込む
            DataTable dataTable = new();
            dataTable.Load(reader);
            return dataTable;
        }

        /// <summary>
        /// 指定したIDのレコードを新しい値で更新します。
        /// </summary>
        /// <typeparam name="T">レコードの型</typeparam>
        /// <param name="databaseName">データベース名</param>
        /// <param name="tableName">テーブル名</param>
        /// <param name="id">レコードID</param>
        /// <param name="record">更新する値を持つオブジェクト</param>
        public void UpdateRecord<T>(string databaseName, string tableName, long id, T record)
        {
            using var connection = _connectionFactory();            
            connection.Open();
            connection.ChangeDatabase(databaseName);

            Type? recordType = record?.GetType();
            var properties = recordType?.GetProperties()
                .Where(p => p.Name != "Id") // Idプロパティを除外  
                .ToArray();
            properties ??= [];

            // 全てのプロパティ名と対応するパラメータプレースホルダ（"@"）を結合  
            var setClause = string.Join(", ", properties.Select(p => p.Name + " = " + "@" + p.Name));

            using var command = connection.CreateCommand();
            string query = $"UPDATE {tableName} SET {setClause} WHERE Id = @Id";
            command.CommandText = query;

            // ID パラメータを追加  
            var idParameter = command.CreateParameter();
            idParameter.ParameterName = "@Id";
            idParameter.Value = id;
            command.Parameters.Add(idParameter);

            AddParametersToCommand(command, properties, record);

            command.ExecuteNonQuery();
        }

        /// <summary>
        /// 指定したIDのレコードを削除します。
        /// </summary>
        /// <param name="databaseName">データベース名</param>
        /// <param name="tableName">テーブル名</param>
        /// <param name="id">削除するレコードのID</param>
        public void DeleteRecord(string databaseName, string tableName, long id)
        {
            using var connection = _connectionFactory();            
            connection.Open();
            connection.ChangeDatabase(databaseName);

            // IDをパラメータ化→"＠"
            string query = $"DELETE FROM {tableName} WHERE Id = @Id"; 
            using var command = connection.CreateCommand();
            command.CommandText = query;

            // パラメータにIDを追加
            var idParameter = command.CreateParameter();
            idParameter.ParameterName = "@Id";
            idParameter.Value = id;
            command.Parameters.Add(idParameter);

            command.ExecuteNonQuery();
        }

        /// <summary>
        /// 指定したレコードのプロパティをコマンドのパラメータとして追加します。
        /// </summary>
        /// <typeparam name="T">レコードの型</typeparam>
        /// <param name="command">IDbCommandオブジェクト</param>
        /// <param name="properties">プロパティ情報配列</param>
        /// <param name="record">パラメータ値を持つオブジェクト</param>
        private static void AddParametersToCommand<T>(IDbCommand command, System.Reflection.PropertyInfo[] properties, T record)
        {
            // レコードの各プロパティに対してコマンドにパラメータを追加
            foreach (var property in properties)
            {
                try
                {
                    // IDbDataParameterを作成
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@" + property.Name;
                    // nullの場合はDBNull.Valueを設定
                    parameter.Value = property.GetValue(record) ?? DBNull.Value;
                    command.Parameters.Add(parameter);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error adding parameter {property.Name}: {ex.Message}");
                }
            }
        }
    }
}
