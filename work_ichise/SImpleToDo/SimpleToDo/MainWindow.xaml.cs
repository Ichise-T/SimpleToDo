using System.Windows;
using System.Windows.Input;
using System.Data;
using SimpleToDo.components;
using SimpleToDo.models;
using SimpleToDo.utils;
using SimpleToDo.services;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Diagnostics;

namespace SimpleToDo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private readonly string _databaseName;
        private readonly string _tableName;
        private readonly DatabaseCrudManager dbManager;
        private readonly OpenWeatherApiClient openWeatherApiClient;
        
        public MainWindow()
        {
            InitializeComponent();

            // データベース接続文字列を取得
            string connectionStrings = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            // MySqlConnectionのファクトリメソッドを作成
            IDbConnection connectionFactory() => new MySqlConnection(connectionStrings);

            // DatabaseCrudManagerのインスタンスを作成
            dbManager = new DatabaseCrudManager(connectionFactory);

            // データベースが無い場合のみ作成
            string databaseName = ConfigurationManager.AppSettings["DatabaseName"] ?? "simple_todo";
            _databaseName = databaseName;
            dbManager.CreateDatabase(_databaseName);
            
            // テーブルが無い場合のみ作成               
            string tableName = ConfigurationManager.AppSettings["TableName"] ?? "todo";
            _tableName = tableName;
            dbManager.CreateTable(_databaseName, _tableName, ["task VARCHAR(100)", "checked BOOLEAN DEFAULT FALSE"]);

            string? openWeatherApiKey = ConfigurationManager.AppSettings["OpenWeatherApiKey"] ?? "";
            openWeatherApiClient = new(openWeatherApiKey);

            LoadToDoData();
            _ = LoadWeatherInfo();
        }

        private void LoadToDoData()
        {
            DataTable dataTable = dbManager.ReadAllRecord(_databaseName, _tableName);

            List<ToDo> ToDoList = new DataConverter().ConvertDataTableToList(dataTable);

            ToDoList.ForEach(toDo =>
            {
                TaskItem taskItem = new
                (
                    toDo,
                    _tableName,
                    ToDoListBox,
                    (tableName, id, toDo) => dbManager.UpdateRecord(_databaseName, tableName, id, toDo),
                    (tableName, id) => dbManager.DeleteRecord(_databaseName, tableName, id)
                );
            });
        }

        private void AppendTaskButton_Click(object sender, RoutedEventArgs e)
        {
            ToDo toDo = new() { Task = TextBoxInputTask.Text };
            long taskId = dbManager.CreateRecord(_databaseName, _tableName, toDo);
            toDo.Id = taskId;

            // タスクアイテムの生成と追加
            TaskItem taskItem = new
                (
                    toDo,
                    _tableName,
                    ToDoListBox,
                    (tableName, id, toDo) => dbManager.UpdateRecord(_databaseName, tableName, id, toDo),
                    (tableName, id) => dbManager.DeleteRecord(_databaseName, tableName, id)
                );

            TextBoxInputTask.Text = "";
        }

        private void TextBoxInputTask_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AppendTaskButton_Click(sender, e);
            }
        }

        private async Task LoadWeatherInfo()
        {
            var weatherInfo = await openWeatherApiClient.GetWeatherInfoAsync("Shiga");
            Debug.WriteLine($"場所：{weatherInfo.Name}, 天気: {weatherInfo.Description}, 気温: {weatherInfo.Temperature}°C, 湿度: {weatherInfo.Humidity}%");
        }
    }
}