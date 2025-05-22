using System.Windows;
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
            _mainViewModel = CreateMainViewModel();
            DataContext = _mainViewModel;

            // ウィンドウの読み込み完了後にデータを初期化するためのイベントハンドラを登録
            Loaded += MainWindow_LoadedAsync;
        }

        /// <summary>
        /// 設定から必要な情報を読み込み、MainViewModelを作成します
        /// </summary>
        /// <returns>初期化されたMainViewModel</returns>
        private static MainViewModel CreateMainViewModel()
        {
            // 設定値を取得し、デフォルト値を設定
            var connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"]?.ConnectionString
                ?? throw new InvalidOperationException("データベース接続文字列が設定されていません。");

            var databaseName = ConfigurationManager.AppSettings["DatabaseName"] ?? "simple_todo";
            var tableName = ConfigurationManager.AppSettings["TableName"] ?? "todo";
            var openWeatherApiKey = ConfigurationManager.AppSettings["OpenWeatherApiKey"] ?? string.Empty;

            return new MainViewModel(connectionString, databaseName, tableName, openWeatherApiKey);
        }

        /// <summary>
        /// ウィンドウが読み込まれたときに呼び出されるイベントハンドラ
        /// ToDoデータと天気情報を非同期で取得し、UIに表示します。
        /// </summary>
        private async void MainWindow_LoadedAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                // ToDoデータの読み込み
                await _mainViewModel.LoadToDoDataAsync();

                // 天気情報APIキーが設定されている場合のみ天気情報を取得
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["OpenWeatherApiKey"]))
                {
                    await _mainViewModel.LoadWeatherInfoAsync();
                }
            }
            catch (Exception ex)
            {
                // 例外をキャッチしてエラーメッセージを表示
                _mainViewModel.ErrorMessage = $"データ読み込み中にエラーが発生しました: {ex.Message}";
            }
        }
    }
}