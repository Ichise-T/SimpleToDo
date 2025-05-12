using SimpleToDo.services.database.interfaces;
using System.Data;
using System.Diagnostics;

namespace SimpleToDo.services.database
{
    /// <summary>
    /// 複数のデータベースに対応したCRUD操作を提供するマネージャクラス。
    /// データベースの作成、テーブル作成、レコードの追加・取得・更新・削除を汎用的に行う。
    /// </summary>
    public class DatabaseCrudManager(Func<Task<IDbConnectionWrapper>> connectionFactory)
    {
        // データベース接続用のファクトリ関数
        private readonly Func<Task<IDbConnectionWrapper>> _connectionFactory = connectionFactory;

        /// <summary>
        /// データベースを作成します（DB種別ごとにSQLを切り替え）。
        /// </summary>
        public async Task CreateDatabaseAsync(string databaseName)
        {
            using var connection = await _connectionFactory();
            await connection.OpenAsync();

            string databaseType = (connection as dynamic).InnerConnectionTypeName ?? connection.GetType().Name.ToLower();

            // データベース種別ごとに作成クエリを切り替え
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
            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// 指定したデータベースにテーブルを作成します。
        /// </summary>
        public async Task CreateTableAsync(string databaseName, string tableName, string[] columnDefinitions)
        {
            using var connection = await _connectionFactory();
            await connection.OpenAsync();
            connection.ChangeDatabase(databaseName);

            string query = $"CREATE TABLE IF NOT EXISTS {tableName} (id INT AUTO_INCREMENT PRIMARY KEY, {string.Join(", ", columnDefinitions)});";
            using var command = connection.CreateCommand();
            command.CommandText = query;
            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// レコードを挿入し、挿入されたIDを返します。
        /// </summary>
        public async Task<long> CreateRecordAsync<T>(string databaseName, string tableName, T record)
        {
            using var connection = await _connectionFactory();
            await connection.OpenAsync();
            connection.ChangeDatabase(databaseName);

            var recordType = record?.GetType();
            var properties = recordType?.GetProperties()
                .Where(p => !p.Name.Equals("id", StringComparison.CurrentCultureIgnoreCase))
                .ToArray();
            properties ??= [];

            var columnNames = string.Join(", ", properties.Select(parameter => parameter.Name.ToLower()));
            var parameterNames = string.Join(", ", properties.Select(p => "@" + p.Name));

            using var command = connection.CreateCommand();
            string query = $"INSERT INTO {tableName} ({columnNames}) VALUES ({parameterNames})";
            command.CommandText = query;

            // プロパティ値をパラメータとして追加
            AddParametersToCommand(command, properties, record);

            await command.ExecuteNonQueryAsync();

            // 最後に挿入されたIDを取得
            command.CommandText = "SELECT LAST_INSERT_ID()";
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt64(result);
        }

        /// <summary>
        /// テーブル内の全レコードをDataTableとして取得します。
        /// </summary>
        public async Task<DataTable> ReadAllRecordAsync(string databaseName, string tableName)
        {
            using var connection = await _connectionFactory();
            await connection.OpenAsync();
            connection.ChangeDatabase(databaseName);

            using var command = connection.CreateCommand();
            string query = $"SELECT * FROM {tableName};";
            command.CommandText = query;

            using var reader = await command.ExecuteReaderAsync();

            DataTable dataTable = new();
            dataTable.Load(reader);

            return dataTable;
        }

        /// <summary>
        /// 指定したIDのレコードをDataTableとして取得します。
        /// </summary>
        public async Task<DataTable> ReadRecordByIdAsync(string databaseName, string tableName, long id)
        {
            using var connection = await _connectionFactory();
            await connection.OpenAsync();
            connection.ChangeDatabase(databaseName);

            using var command = connection.CreateCommand();
            string query = $"SELECT * FROM {tableName} WHERE id = @Id";
            command.CommandText = query;

            var idParameter = command.CreateParameter();
            idParameter.ParameterName = "@Id";
            idParameter.Value = id;
            command.Parameters.Add(idParameter);

            using var reader = await command.ExecuteReaderAsync();

            DataTable dataTable = new();
            dataTable.Load(reader);
            return dataTable;
        }

        /// <summary>
        /// 指定IDのレコードを更新します。
        /// </summary>
        public async Task UpdateRecordAsync<T>(string databaseName, string tableName, long id, T record)
        {
            using var connection = await _connectionFactory();
            await connection.OpenAsync();
            connection.ChangeDatabase(databaseName);

            Type? recordType = record?.GetType();
            var properties = recordType?.GetProperties()
                .Where(p => !p.Name.Equals("id", StringComparison.CurrentCultureIgnoreCase))
                .ToArray();
            properties ??= [];

            var setClause = string.Join(", ", properties.Select(p => p.Name.ToLower() + " = " + "@" + p.Name));

            using var command = connection.CreateCommand();
            string query = $"UPDATE {tableName} SET {setClause} WHERE id = @Id";
            command.CommandText = query;

            var idParameter = command.CreateParameter();
            idParameter.ParameterName = "@Id";
            idParameter.Value = id;
            command.Parameters.Add(idParameter);

            // プロパティ値をパラメータとして追加
            AddParametersToCommand(command, properties, record);

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// 指定IDのレコードを削除します。
        /// </summary>
        public async Task DeleteRecordAsync(string databaseName, string tableName, long id)
        {
            using var connection = await _connectionFactory();
            await connection.OpenAsync();
            connection.ChangeDatabase(databaseName);

            string query = $"DELETE FROM {tableName} WHERE id = @id";
            using var command = connection.CreateCommand();
            command.CommandText = query;

            var idParameter = command.CreateParameter();
            idParameter.ParameterName = "@id";
            idParameter.Value = id;
            command.Parameters.Add(idParameter);

            await command.ExecuteNonQueryAsync();
        }

        private static void AddParametersToCommand<T>(IDbCommandWrapper command, System.Reflection.PropertyInfo[] properties, T record)
        {
            foreach (var property in properties)
            {
                try
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@" + property.Name;
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