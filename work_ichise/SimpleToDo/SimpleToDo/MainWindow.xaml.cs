using System.Windows;
using System.Windows.Input;
using SimpleToDo.mvvm.view_models;
using System.Configuration;

namespace SimpleToDo
{
    /// <summary>
    /// MainWindow.xamlのインタラクションロジック
    /// シンプルなタスク管理アプリケーションのメインウィンドウを提供します。
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// アプリケーションのMainViewModel
        /// このViewModelは、TODOリストのデータと天気情報を管理します。
        /// </summary>
        private readonly MainViewModel _mainViewModel;

        /// <summary>
        /// MainViewModelクラスのコンストラクタ
        /// ウィンドウの初期化とデータベース、API設定を行います。
        /// </summary>
        public MainWindow()
        {
            // XAMLコンポーネントの初期化
            InitializeComponent();

            // データベースとAPI設定を構成ファイルから取得して初期化
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            string databaseName = ConfigurationManager.AppSettings["DatabaseName"] ?? "simple_todo";
            string tableName = ConfigurationManager.AppSettings["TableName"] ?? "todo";
            string openWeatherApiKey = ConfigurationManager.AppSettings["OpenWeatherApiKey"] ?? "";
            
            // ViewModelを初期化とデータコンテキストの設定
            _mainViewModel = new MainViewModel(connectionString, databaseName, tableName, openWeatherApiKey);
            DataContext = _mainViewModel;
            
            // 初期ToDoデータの非同期読み込み
            _mainViewModel.LoadToDoDataAsync();
            
            // ウィンドウの読み込み完了後に天気情報を取得するためのイベントハンドラを登録
            Loaded += MainWindow_Loaded;
        }

        /// <summary>
        /// ウィンドウが読み込まれたときに呼び出されるイベントハンドラ
        /// 天気情報を非同期で取得し、UIに表示します。
        /// </summary>
        /// <param name="sender">イベントの送信元</param>
        /// <param name="e">イベント引数</param>
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // 天気情報を非同期で取得
                await _mainViewModel.LoadWeatherInfoAsync();
            }
            catch (Exception ex)
            {
                // UIスレッドで例外をキャッチしてエラーメッセージを表示
                _mainViewModel.ErrorMessage = $"天気情報の読み込み中にエラーが発生しました: {ex.Message}";
            }
        }
    }
}