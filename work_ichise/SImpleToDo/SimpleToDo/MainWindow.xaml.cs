using System.Windows;
using System.Windows.Input;
using System.Data;
using SimpleToDo.view_models;
using SimpleToDo.models;
using SimpleToDo.utils;
using SimpleToDo.services;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace SimpleToDo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private readonly MainViewModel mainViewModel = new();
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

            string databaseName = ConfigurationManager.AppSettings["DatabaseName"] ?? "simple_todo";
            string tableName = ConfigurationManager.AppSettings["TableName"] ?? "todo";

            // データベースが無い場合のみ作成
            _databaseName = databaseName;
            dbManager.CreateDatabase(_databaseName);
            
            // テーブルが無い場合のみ作成               
            _tableName = tableName;
            dbManager.CreateTable(_databaseName, _tableName, ["task_name VARCHAR(250)", "is_checked BOOLEAN DEFAULT FALSE"]);

            // APIキーを取得
            string? openWeatherApiKey = ConfigurationManager.AppSettings["OpenWeatherApiKey"] ?? "";
            openWeatherApiClient = new(openWeatherApiKey);

            this.DataContext = mainViewModel;

            LoadToDoData();
            _ = LoadWeatherInfo();

        }

        private void LoadToDoData()
        {
            DataTable dataTable = dbManager.ReadAllRecord(_databaseName, _tableName);

            List<ToDo> ToDoList = new DataConverter().ConvertDataTableToList(dataTable);

            // MainViewModelのインスタンスを作成し、TaskItemsにToDoListを追加
            mainViewModel.ToDoItems.Clear();
            foreach (var toDoItem in ToDoList)
            {
                mainViewModel.ToDoItems.Add(new ToDoItemViewModel(
                    toDoItem,
                    () => dbManager.UpdateRecord(_databaseName, _tableName, toDoItem.Id, toDoItem),
                    () =>
                    {
                        dbManager.DeleteRecord(_databaseName, _tableName, toDoItem.Id);
                        mainViewModel.RemoveToDoItem(
                            mainViewModel.ToDoItems.First(vm => vm._toDo.Id == toDoItem.Id)
                        );
                    }));
            }
        }

        private void AppendTaskButton_Click(object sender, RoutedEventArgs e)
        {
            ToDo toDoItem = new() { Task_Name = TextBoxInputTask.Text };
            long taskId = dbManager.CreateRecord(_databaseName, _tableName, toDoItem);
            toDoItem.Id = taskId;

            // TaskItemsに追加
            mainViewModel.ToDoItems.Add(new ToDoItemViewModel(toDoItem,
                () => dbManager.UpdateRecord(_databaseName, _tableName, toDoItem.Id, toDoItem),
                () =>
                {
                    dbManager.DeleteRecord(_databaseName, _tableName, toDoItem.Id);
                    mainViewModel.RemoveToDoItem(
                        mainViewModel.ToDoItems.First(vm => vm._toDo.Id == toDoItem.Id)
                    );
                }));

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
            string weatherInfoString =
                $"場所：{weatherInfo.Name}\n" +
                $"天気: {weatherInfo.Description}\n" +
                $"気温: {weatherInfo.Temperature}°C\n" +
                $"湿度: {weatherInfo.Humidity}%";
            mainViewModel.WeatherInfoItems.Clear();
            mainViewModel.WeatherInfoItems.Add(new WeatherInfoItemViewModel(weatherInfoString));
        }
    }
}