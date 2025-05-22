using System.Collections.ObjectModel;
using System.Data;
using SimpleToDo.mvvm.models;
using SimpleToDo.services.database;
using SimpleToDo.services.weather;
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
        public ObservableCollection<WeatherInfoItemViewModel> WeatherInfoItems { get; } = [];

        /// <summary>
        /// ToDoアイテムのViewModelコレクション
        /// </summary>
        public ObservableCollection<ToDoItemViewModel> ToDoItems { get; } = [];
        
        // データベース操作用マネージャ
        private readonly DatabaseCrudManager _dbManager;
        // 天気情報APIクライアント
        private readonly OpenWeatherMapApiClient _weatherClient;
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
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged(nameof(ErrorMessage));
                }
            }
        }
        
        /// <summary>
        /// MainViewModelのコンストラクタ。DBやAPIクライアントの初期化を行います。
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
            _dbManager = new DatabaseCrudManager(() => 
                Task.FromResult<IDbConnectionWrapper>(
                    new DbConnectionWrapper(new MySqlConnection(connectionString))
                ));
            
            // 天気情報APIクライアントの初期化
            _weatherClient = new OpenWeatherMapApiClient(apiKey);
            
            // データベースの初期化を開始（非同期で実行）
            _ = InitializeDatabaseAsync();
        }

        /// <summary>
        /// データベースの初期化を非同期で実行します
        /// </summary>
        private async Task InitializeDatabaseAsync()
        {
            try
            {
                await _dbManager.CreateDatabaseAsync(_databaseName);
                await _dbManager.CreateTableAsync(_databaseName, _tableName, 
                    ["task_name VARCHAR(250)", "is_checked BOOLEAN DEFAULT FALSE"]);
            }
            catch (Exception ex)
            {
                HandleException(ex, "データベース初期化");
            }
        }
        
        /// <summary>
        /// ToDoデータをデータベースから読み込み、コレクションに反映します。
        /// </summary>
        public async Task LoadToDoDataAsync()
        {
            try
            {
                DataTable dataTable = await _dbManager.ReadAllRecordAsync(_databaseName, _tableName);
                List<ToDoItem> toDoList = new DataConverter().ConvertDataTableToList(dataTable);
                
                ToDoItems.Clear();
                foreach (var toDoItem in toDoList)
                {
                    AddToDoItemToCollection(toDoItem);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "データ読み込み");
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
                ToDoItem toDoItem = new() { Task_Name = taskName };
                long taskId = await _dbManager.CreateRecordAsync(_databaseName, _tableName, toDoItem);
                toDoItem.Id = taskId;
                
                AddToDoItemToCollection(toDoItem);
            }
            catch (Exception ex)
            {
                HandleException(ex, "タスク追加");
            }
        }
        
        /// <summary>
        /// ToDoモデルをViewModelに変換し、コレクションに追加します。
        /// </summary>
        /// <param name="toDoItem">追加するToDoモデル</param>
        private void AddToDoItemToCollection(ToDoItem toDoItem)
        {
            ToDoItems.Add(new ToDoItemViewModel(
                toDoItem,
                async () => await UpdateToDoItemAsync(toDoItem),
                () => DeleteToDoItemAsync(toDoItem)
            ));
        }
        
        /// <summary>
        /// ToDoアイテムの内容を更新し、データベースに反映します。
        /// </summary>
        /// <param name="toDoItem">更新対象のToDoモデル</param>
        private async Task UpdateToDoItemAsync(ToDoItem toDoItem)
        {
            try
            {
                await _dbManager.UpdateRecordAsync(_databaseName, _tableName, toDoItem.Id, toDoItem);
            }
            catch (Exception ex)
            {
                HandleException(ex, "タスク更新");
            }
        }
        
        /// <summary>
        /// ToDoアイテムを削除し、データベースとコレクションから除去します。
        /// </summary>
        /// <param name="toDoItem">削除対象のToDoモデル</param>
        private async Task DeleteToDoItemAsync(ToDoItem toDoItem)
        {
            try
            {
                await _dbManager.DeleteRecordAsync(_databaseName, _tableName, toDoItem.Id);
                var itemToRemove = ToDoItems.FirstOrDefault(vm => vm._toDoItem.Id == toDoItem.Id);
                if (itemToRemove != null)
                {
                    ToDoItems.Remove(itemToRemove);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "タスク削除");
            }
        }
        
        /// <summary>
        /// 天気情報をAPIから取得し、コレクションに反映します。
        /// </summary>
        public async Task LoadWeatherInfoAsync()
        {
            try
            {
                var weatherInfo = await _weatherClient.GetWeatherResponseAsync("Shiga");
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
                HandleException(ex, "天気情報取得");
            }
        }

        /// <summary>
        /// 集約化されたエラーハンドリング
        /// </summary>
        private void HandleException(Exception ex, string operation)
        {
            ErrorMessage = $"{operation}エラー: {ex.Message}";
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