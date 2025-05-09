using System.Windows;
using System.Windows.Input;
using System.Data;
using SimpleToDo.mvvm.view_models;
using SimpleToDo.mvvm.models;
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
        private readonly MainViewModel _mainViewModel;
        
        public MainWindow()
        {
            InitializeComponent();

            // データベースとAPI設定を初期化
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            string databaseName = ConfigurationManager.AppSettings["DatabaseName"] ?? "simple_todo";
            string tableName = ConfigurationManager.AppSettings["TableName"] ?? "todo";
            string openWeatherApiKey = ConfigurationManager.AppSettings["OpenWeatherApiKey"] ?? "";
            
            // ViewModelを初期化
            _mainViewModel = new MainViewModel(connectionString, databaseName, tableName, openWeatherApiKey);
            DataContext = _mainViewModel;
            
            // 初期データの読み込み
            _mainViewModel.LoadToDoDataAsync();
            
            // 非同期で天気情報を読み込む（イベントハンドラに移動）
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await _mainViewModel.LoadWeatherInfoAsync();
            }
            catch (Exception ex)
            {
                // UIスレッドで例外をキャッチして処理できる
                _mainViewModel.ErrorMessage = $"天気情報の読み込み中にエラーが発生しました: {ex.Message}";
            }
        }

        private async void AppendTaskButton_Click(object sender, RoutedEventArgs e)
        {
            await _mainViewModel.AddToDoItemAsync(TextBoxInputTask.Text);
            TextBoxInputTask.Text = "";
        }

        private void TextBoxInputTask_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AppendTaskButton_Click(sender, e);
            }
        }
    }
}