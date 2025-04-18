using System.Data;
using System.Diagnostics;

namespace SimpleToDo.services
{
    public class DatabaseCrudManager(Func<IDbConnection> connectionFactory)
    {
        private readonly Func<IDbConnection> _connectionFactory = connectionFactory;

        public DataTable ReadAllData(string tableName)
        {
            // DataTableを作成
            DataTable dataTable = new();

            // データベース接続を開く
            using var connection = _connectionFactory();
            connection.Open();

            // IDbCommandを作成
            using var command = connection.CreateCommand();
            string query = $"SELECT * FROM {tableName};";
            command.CommandText = query;

            // ExecuteReaderでデータを取得
            using var reader = command.ExecuteReader();

            // DataTableにデータを読み込む
            dataTable.Load(reader);

            return dataTable;
        }

        public long CreateRecord<T>(string tableName, T record)
        {
            using var connection = _connectionFactory();
            connection.Open();

            // ジェネリック型Tのプロパティを取得
            var recordType = record?.GetType();
            var properties = recordType?.GetProperties()
                .Where(p => p.Name != "Id") // Idプロパティを除外  
                .ToArray() ?? [];
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

        public void DeleteRecord(string tableName, long id)
        {
            using var connection = _connectionFactory();
            connection.Open();

            // IDをパラメータ化→"＠"
            string query = $"DELETE FROM {tableName} WHERE Id = @Id"; 
            using var command = connection.CreateCommand();
            command.CommandText = query;

            // IDパラメータを追加
            var idParameter = command.CreateParameter();
            idParameter.ParameterName = "@Id";
            idParameter.Value = id;
            command.Parameters.Add(idParameter);

            command.ExecuteNonQuery();
        }

        public void UpdateRecord<T>(string tableName, long id, T record)
        {
            using var connection = _connectionFactory();
            connection.Open();

            Type? recordType = record?.GetType();
            var properties = recordType?.GetProperties()
                .Where(p => p.Name != "Id") // Idプロパティを除外  
                .ToArray() ?? [];

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
