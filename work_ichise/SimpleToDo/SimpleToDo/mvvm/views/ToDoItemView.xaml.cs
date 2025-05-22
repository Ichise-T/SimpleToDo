using SimpleToDo.mvvm.view_models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SimpleToDo.mvvm.views
{
    /// <summary>
    /// ToDo.xaml の相互作用ロジック - ToDoリスト操作用のUIコントロール
    /// </summary>
    public partial class ToDo : UserControl
    {
        /// <summary>
        /// DataContextをMainViewModelにキャストして取得するプロパティ
        /// </summary>
        private MainViewModel? ViewModel => DataContext as MainViewModel;

        /// <summary>
        /// コンストラクタ - コントロールの初期化を行います
        /// </summary>
        public ToDo()
        {
            InitializeComponent();

            // フォーカス設定をコンストラクタで行う
            Loaded += (_, _) => TextBoxInputTask?.Focus();
        }

        /// <summary>
        /// タスクを追加する共通処理
        /// </summary>
        /// <returns>追加処理の完了を表すTask</returns>
        private async Task AddTaskAsync()
        {
            // 入力内容の検証
            string? text = TextBoxInputTask?.Text?.Trim();
            if (string.IsNullOrEmpty(text) || ViewModel == null)
                return;

            try
            {
                // UIの操作を無効化
                IsEnabled = false;

                // タスクの追加処理
                await ViewModel.AddToDoItemAsync(text);

                // 入力欄のクリア
                TextBoxInputTask?.Clear();
            }
            catch (Exception ex)
            {
                // エラー処理
                if (ViewModel != null)
                {
                    ViewModel.ErrorMessage = $"タスク追加エラー: {ex.Message}";
                }
            }
            finally
            {
                // UIの操作を再有効化
                IsEnabled = true;
                TextBoxInputTask?.Focus();
            }
        }

        /// <summary>
        /// タスク追加ボタンがクリックされたときのイベントハンドラ
        /// </summary>
        private async void AppendTaskButton_Click(object sender, RoutedEventArgs e)
        {
            await AddTaskAsync();
        }

        /// <summary>
        /// テキストボックスでキーが押されたときのイベントハンドラ
        /// </summary>
        private async void TextBoxInputTask_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true; // イベントが処理済みとしてマーク
                await AddTaskAsync();
            }
        }
    }
}