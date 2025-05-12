using System.Collections.ObjectModel;
using System.Data;
using SimpleToDo.mvvm.models;
using SimpleToDo.services.database;
using SimpleToDo.services.open_weather_map;
using SimpleToDo.utils;
using MySql.Data.MySqlClient;
using System.ComponentModel;
using SimpleToDo.services.database.interfaces;
using SimpleToDo.services.database.wrappers;

namespace SimpleToDo.mvvm.view_models
{
    /// <summary>
    /// アプリ全体の状態やToDoリスト・天気情報の管理を行うメインViewModelクラス。
    /// データベースやAPIとの連携、エラー管理、コレクションの操作などを担当します。
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// プロパティ変更通知イベント（INotifyPropertyChangedの実装）
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 天気情報のViewModelコレクション
        /// </summary>
        public ObservableCollection<WeatherInfoItemViewModel> WeatherInfoItems { get; set; } = [];

        /// <summary>
        /// ToDoアイテムのViewModelコレクション
        /// </summary>
        public ObservableCollection<ToDoItemViewModel> ToDoItems { get; set; } = [];
        
        // データベース操作用マネージャ
        private readonly DatabaseCrudManager _dbManager;
        // 天気情報APIクライアント
        private readonly OpenWeatherMapApiClient _weatherApiClient;
        // データベース名
        private readonly string _databaseName;
        // テーブル名
        private readonly string _tableName;
        
        // エラーメッセージ
        private string _errorMessage = string.Empty;

        /// <summary>
        /// エラーメッセージ（UIバインディング用）
        /// </summary>
        public string ErrorMessage 
        { 
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }
        
        /// <summary>
        /// MainViewModelのコンストラクタ。DBやAPIクライアントの初期化、テーブル作成などを行います。
        /// </summary>
        /// <param name="connectionString">DB接続文字列</param>
        /// <param name="databaseName">データベース名</param>
        /// <param name="tableName">テーブル名</param>
        /// <param name="apiKey">OpenWeatherMap APIキー</param>
        public MainViewModel(string connectionString, string databaseName, string tableName, string apiKey)
        {
            _databaseName = databaseName;
            _tableName = tableName;

            // データベース接続の初期化
            Task<IDbConnectionWrapper> connectionFactory() =>
                Task.FromResult<IDbConnectionWrapper>(
                    new DbConnectionWrapper(new MySqlConnection(connectionString))
                );
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
            _weatherApiClient = new OpenWeatherMapApiClient(apiKey);
        }
        
        /// <summary>
        /// ToDoデータをデータベースから読み込み、コレクションに反映します。
        /// </summary>
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
        
        /// <summary>
        /// 新しいToDoアイテムを追加し、データベースとコレクションを更新します。
        /// </summary>
        /// <param name="taskName">追加するタスク名</param>
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
        
        /// <summary>
        /// ToDoモデルをViewModelに変換し、コレクションに追加します。
        /// </summary>
        /// <param name="toDoItem">追加するToDoモデル</param>
        private void AddToDoItemToCollection(ToDo toDoItem)
        {
            ToDoItems.Add(new ToDoItemViewModel(
                toDoItem,
                async () => await  UpdateToDoItemAsync(toDoItem),
                () => DeleteToDoItemAsync(toDoItem)
            ));
        }
        
        /// <summary>
        /// ToDoアイテムの内容を更新し、データベースに反映します。
        /// </summary>
        /// <param name="toDoItem">更新対象のToDoモデル</param>
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
        
        /// <summary>
        /// ToDoアイテムを削除し、データベースとコレクションから除去します。
        /// </summary>
        /// <param name="toDoItem">削除対象のToDoモデル</param>
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
        
        /// <summary>
        /// ToDoアイテムのViewModelをコレクションから削除します（UI操作用）。
        /// </summary>
        /// <param name="item">削除するViewModel</param>
        public void RemoveToDoItem(ToDoItemViewModel item)
        {
            ToDoItems.Remove(item);
        }
        
        /// <summary>
        /// 天気情報をAPIから取得し、コレクションに反映します。
        /// </summary>
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
        
        /// <summary>
        /// プロパティ変更通知を発行します。
        /// </summary>
        /// <param name="propertyName">変更されたプロパティ名</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}