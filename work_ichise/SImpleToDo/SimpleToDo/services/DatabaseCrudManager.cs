using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace SimpleToDo.services
{
    public class DatabaseCrudManager(Func<Task<DbConnection>> connectionFactory)
    {
        private readonly Func<Task<DbConnection>> _connectionFactory = connectionFactory;
        
        public async Task CreateDatabaseAsync(string databaseName)
        {
            using var connection = await _connectionFactory();
            await connection.OpenAsync();

            string databaseType = connection.GetType().Name.ToLower();

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

        public async Task<long> CreateRecordAsync<T>(string databaseName, string tableName, T record)
        {
            using var connection = await _connectionFactory();
            await connection.OpenAsync();
            connection.ChangeDatabase(databaseName);

            var recordType = record?.GetType();
            var properties = recordType?.GetProperties()
                .Where(p => p.Name != "Id")
                .ToArray();
            properties ??= [];

            var columnNames = string.Join(", ", properties.Select(parameter => parameter.Name));
            var parameterNames = string.Join(", ", properties.Select(p => "@" + p.Name));

            using var command = connection.CreateCommand();
            string query = $"INSERT INTO {tableName} ({columnNames}) VALUES ({parameterNames})";
            command.CommandText = query;

            AddParametersToCommand(command, properties, record);

            await command.ExecuteNonQueryAsync();

            command.CommandText = "SELECT LAST_INSERT_ID()";
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt64(result);
        }

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

        public async Task<DataTable> ReadRecordByIdAsync(string databaseName, string tableName, long id)
        {
            using var connection = await _connectionFactory();
            await connection.OpenAsync();
            connection.ChangeDatabase(databaseName);

            using var command = connection.CreateCommand();
            string query = $"SELECT * FROM {tableName} WHERE Id = @Id";
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

        public async Task UpdateRecordAsync<T>(string databaseName, string tableName, long id, T record)
        {
            using var connection = await _connectionFactory();
            await connection.OpenAsync();
            connection.ChangeDatabase(databaseName);

            Type? recordType = record?.GetType();
            var properties = recordType?.GetProperties()
                .Where(p => p.Name != "Id")
                .ToArray();
            properties ??= [];

            var setClause = string.Join(", ", properties.Select(p => p.Name + " = " + "@" + p.Name));

            using var command = connection.CreateCommand();
            string query = $"UPDATE {tableName} SET {setClause} WHERE Id = @Id";
            command.CommandText = query;

            var idParameter = command.CreateParameter();
            idParameter.ParameterName = "@Id";
            idParameter.Value = id;
            command.Parameters.Add(idParameter);

            AddParametersToCommand(command, properties, record);

            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteRecordAsync(string databaseName, string tableName, long id)
        {
            using var connection = await _connectionFactory();
            await connection.OpenAsync();
            connection.ChangeDatabase(databaseName);

            string query = $"DELETE FROM {tableName} WHERE Id = @Id";
            using var command = connection.CreateCommand();
            command.CommandText = query;

            var idParameter = command.CreateParameter();
            idParameter.ParameterName = "@Id";
            idParameter.Value = id;
            command.Parameters.Add(idParameter);

            await command.ExecuteNonQueryAsync();
        }

        private static void AddParametersToCommand<T>(IDbCommand command, System.Reflection.PropertyInfo[] properties, T record)
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