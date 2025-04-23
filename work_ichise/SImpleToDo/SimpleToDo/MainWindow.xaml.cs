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

            string? openWeatherApiKey = ConfigurationManager.AppSettings["OpenWeatherApiKey"] ?? "";
            openWeatherApiClient = new(openWeatherApiKey);

            LoadToDoData();
            var _ = LoadWeatherInfo();
        }

        private void LoadToDoData()
        {
            DataTable dataTable = dbManager.ReadAllRecord("todo");

            List<ToDo> ToDoList = new DataConverter().ConvertDataTableToList(dataTable);

            ToDoList.ForEach(toDo =>
            {
                TaskItem taskItem = new
                (
                    toDo,
                    ToDoListBox,
                    (table, id, toDo) => dbManager.UpdateRecord(table, id, toDo),
                    (table, id) => dbManager.DeleteRecord(table, id)
                );
            });
        }

        private void AppendTaskButton_Click(object sender, RoutedEventArgs e)
        {
            string tableName = "todo";
            ToDo toDo = new() { Task = TextBoxInputTask.Text };
            long taskId = dbManager.CreateRecord(tableName, toDo);
            toDo.Id = taskId;

            // タスクアイテムの生成と追加
            TaskItem taskItem = new
                (
                    toDo,
                    ToDoListBox,
                    (table, id, toDo) => dbManager.UpdateRecord(table, id, toDo),
                    (table, id) => dbManager.DeleteRecord(table, id)
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