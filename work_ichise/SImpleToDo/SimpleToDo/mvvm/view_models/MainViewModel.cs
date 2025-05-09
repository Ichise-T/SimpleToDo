using System.Collections.ObjectModel;
using System.Data;
using SimpleToDo.mvvm.models;
using SimpleToDo.services;
using SimpleToDo.utils;
using MySql.Data.MySqlClient;
using System.ComponentModel;
using System.Data.Common;

namespace SimpleToDo.mvvm.view_models
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<WeatherInfoItemViewModel> WeatherInfoItems { get; set; } = [];
        public ObservableCollection<ToDoItemViewModel> ToDoItems { get; set; } = [];
        
        private readonly DatabaseCrudManager _dbManager;
        private readonly OpenWeatherApiClient _weatherApiClient;
        private readonly string _databaseName;
        private readonly string _tableName;
        
        private string _errorMessage = string.Empty;
        public string ErrorMessage 
        { 
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }
        
        public MainViewModel(string connectionString, string databaseName, string tableName, string apiKey)
        {
            _databaseName = databaseName;
            _tableName = tableName;
            
            // データベース接続の初期化
            Task<DbConnection> connectionFactory() 
            {
                return Task.FromResult<DbConnection>(new MySqlConnection(connectionString));
            }
            _dbManager = new DatabaseCrudManager(connectionFactory);
            
            try
            {
                // 非同期メソッドを同期的に初期化（実際はコンストラクタを非同期にできないため）
                // このように同期的に待機するのは、UIスレッドでのデッドロックの可能性があるため注意
                Task.Run(async () => 
                {
                    await _dbManager.CreateDatabaseAsync(_databaseName);
                    await _dbManager.CreateTableAsync(_databaseName, _tableName, 
                        ["task_name VARCHAR(250)", "is_checked BOOLEAN DEFAULT FALSE"]);
                }).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"データベース初期化エラー: {ex.Message}";
            }
            
            // 天気情報APIクライアントの初期化
            _weatherApiClient = new OpenWeatherApiClient(apiKey);
        }
        
        public async void LoadToDoDataAsync()
        {
            try
            {
                DataTable dataTable =  await _dbManager.ReadAllRecordAsync(_databaseName, _tableName);
                List<ToDo> toDoList = new DataConverter().ConvertDataTableToList(dataTable);
                
                ToDoItems.Clear();
                foreach (var toDoItem in toDoList)
                {
                    AddToDoItemToCollection(toDoItem);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"データ読み込みエラー: {ex.Message}";
            }
        }
        
        public async Task AddToDoItemAsync(string taskName)
        {
            if (string.IsNullOrWhiteSpace(taskName))
                return;

            try
            {
                ToDo toDoItem = new() { Task_Name = taskName };
                long taskId = await _dbManager.CreateRecordAsync(_databaseName, _tableName, toDoItem);
                toDoItem.Id = taskId;
                
                AddToDoItemToCollection(toDoItem);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"タスク追加エラー: {ex.Message}";
            }
        }
        
        private void AddToDoItemToCollection(ToDo toDoItem)
        {
            ToDoItems.Add(new ToDoItemViewModel(
                toDoItem,
                async () => await  UpdateToDoItemAsync(toDoItem),
                () => DeleteToDoItemAsync(toDoItem)
            ));
        }
        
        private async Task UpdateToDoItemAsync(ToDo toDoItem)
        {
            try
            {
                await _dbManager.UpdateRecordAsync(_databaseName, _tableName, toDoItem.Id, toDoItem);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"タスク更新エラー: {ex.Message}";
            }
        }
        
        private async void DeleteToDoItemAsync(ToDo toDoItem)
        {
            try
            {
                await _dbManager.DeleteRecordAsync(_databaseName, _tableName, toDoItem.Id);
                var itemToRemove = ToDoItems.FirstOrDefault(vm => vm._toDo.Id == toDoItem.Id);
                if (itemToRemove != null)
                {
                    ToDoItems.Remove(itemToRemove);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"タスク削除エラー: {ex.Message}";
            }
        }
        
        public void RemoveToDoItem(ToDoItemViewModel item)
        {
            ToDoItems.Remove(item);
        }
        
        public async Task LoadWeatherInfoAsync()
        {
            try
            {
                var weatherInfo = await _weatherApiClient.GetWeatherInfoAsync("Shiga");
                string weatherInfoString =
                    $"場所：{weatherInfo.Name}\n" +
                    $"天気: {weatherInfo.Description}\n" +
                    $"気温: {weatherInfo.Temperature}°C\n" +
                    $"湿度: {weatherInfo.Humidity}%";
                
                WeatherInfoItems.Clear();
                WeatherInfoItems.Add(new WeatherInfoItemViewModel(weatherInfoString));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"天気情報取得エラー: {ex.Message}";
            }
        }
        
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}