using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;

namespace ToDoList
{
    public class DatabaseCrudManager
    {
        private MySqlConnection? connection;

        public void DatabaseConnection()
        {
            string connectionStrings = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString; // App.configから読み込む
            connection = new MySqlConnection(connectionStrings);            
        }

        public void OpenConnection()
        {
            if(connection?.State != System.Data.ConnectionState.Open)
            {
                connection?.Open();
            }
        }

        public void CloseConnection()
        {
            if(connection?.State != System.Data.ConnectionState.Closed)
            {
                connection?.Close();
            }
        }

        public DataTable ReadAllData(string tableName)
        {
            DataTable dataTable = new();
            string query = $"SELECT * FROM {tableName};";
            using (MySqlCommand cmd = new(query, connection))
            {
                using MySqlDataAdapter adapter = new(cmd);
                adapter.Fill(dataTable);
            } 

            return dataTable;
        }

        public long CreateRecord<T>(string tableName, T record)
        {
            this.OpenConnection();

            // ジェネリック型Tのプロパティを取得
            var properties = typeof(T).GetProperties(); 
            // プロパティからカラム名のカンマ区切りリストを作成
            var columnNames = string.Join(", ", properties.Select(parameter => parameter.Name)); 
            // SQLクエリのパラメータ名のカンマ区切りリストを作成
            var parameterNames = string.Join(", ", properties.Select(p => "@" +  p.Name));

            var query = $"INSERT INTO {tableName} ({columnNames}) VALUES ({parameterNames})";

            using MySqlCommand cmd = new(query, connection);
            // レコードの各プロパティに対してコマンドにパラメータを追加
            foreach ( var property in properties)
            {
                cmd.Parameters.AddWithValue("@" + property.Name, property.GetValue(record));
            }

            cmd.ExecuteNonQuery();
            long id = cmd.LastInsertedId; // 最後に挿入されたレコードのIDを取得

            this.CloseConnection();

            return id;
        }

        public void DeleteRecord(string tableName, long id)
        {
            this.OpenConnection();

            string query = $"DELETE FROM {tableName} WHERE id = @id"; // idはパラメータ化
            using MySqlCommand cmd = new(query, connection);
            cmd.Parameters.AddWithValue("@id", id); 
            cmd.ExecuteNonQuery();

            this.CloseConnection();
        }

        public void UpdateRecord<T>(string tableName, long id, T record)
        {
            this.OpenConnection();

            var properties = typeof(T).GetProperties();
            // 全てのプロパティ名と対応するパラメータプレースホルダを結合
            var setClause = string.Join(",", properties.Select(p => $"{p.Name} =  @{p.Name}" ));

            string query = $"UPDATE {tableName} SET {setClause} WHERE id = @id";

            using MySqlCommand cmd = new(query, connection);
            foreach(var property in properties)
            {
                cmd.Parameters.AddWithValue("@" + property.Name, property.GetValue(record));
            }
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            this.CloseConnection();
        }
    }
}
