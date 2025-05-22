using SimpleToDo.mvvm.view_models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleToDo.mvvm.views
{
    /// <summary>
    /// ToDo.xaml の相互作用ロジック
    /// </summary>
    public partial class ToDo : UserControl
    {
        private MainViewModel? MainViewModel => DataContext as MainViewModel;

        public ToDo()
        {
            InitializeComponent();
        }

        /// <summary>
        /// タスク追加ボタンがクリックされたときのイベントハンドラ
        /// 新しいタスクを追加し、テキストボックスをクリアします。
        /// </summary>
        /// <param name="sender">イベントの送信元</param>
        /// <param name="e">イベント引数</param>
        private async void AppendTaskButton_Click(object sender, RoutedEventArgs e)
        {
            // TextBoxInputTaskがnullまたはそのTextプロパティが空の場合は処理を終了
            if (TextBoxInputTask?.Text == null || string.IsNullOrWhiteSpace(TextBoxInputTask?.Text))
                return;

            // ViewModelを使用してタスクを追加
            if (MainViewModel != null)
            {
                await MainViewModel.AddToDoItemAsync(TextBoxInputTask.Text);
            }
            TextBoxInputTask.Clear();
        }

        /// <summary>
        /// テキストボックスでEnterキーが押されたときのイベントハンドラ
        /// タスク追加ボタンをクリックするのと同じ処理を実行します。
        /// </summary>
        /// <param name="sender">イベントの送信元</param>
        /// <param name="e">イベント引数</param>
        /// <remarks>
        /// このメソッドは、Enterキーが押されたときにタスクを追加するためのショートカットとして機能します。
        /// </remarks>
        private void TextBoxInputTask_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AppendTaskButton_Click(sender, e);
            }
        }
    }
}
