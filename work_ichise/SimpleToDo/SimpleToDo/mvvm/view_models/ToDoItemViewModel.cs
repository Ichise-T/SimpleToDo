using SimpleToDo.mvvm.models;
using System.Windows.Input;
using System.ComponentModel;
using SimpleToDo.utils;

namespace SimpleToDo.mvvm.view_models
{
    /// <summary>
    /// 単一のToDoアイテムの状態や操作（チェック・削除）を管理するViewModelクラスです。
    /// </summary>
    public class ToDoItemViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// プロパティ変更通知イベント（INotifyPropertyChangedの実装）
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 管理対象のToDoモデル
        /// </summary>
        public readonly ToDoItem _toDoItem;

        /// <summary>
        /// チェック状態更新時に呼び出す非同期処理
        /// </summary>
        private readonly Func<Task> _updateCheckAction;

        /// <summary>
        /// 削除時に呼び出す非同期処理
        /// </summary>
        private readonly Func<Task> _deleteAction;

        /// <summary>
        /// ToDoモデル・コマンド・アクションを受け取ってViewModelを初期化します。
        /// </summary>
        public ToDoItemViewModel(ToDoItem toDoItem, Func<Task> updateCheckAction, Func<Task> deleteAction)
        {
            _toDoItem = toDoItem ?? throw new ArgumentNullException(nameof(toDoItem));
            _updateCheckAction = updateCheckAction ?? throw new ArgumentNullException(nameof(updateCheckAction));
            _deleteAction = deleteAction ?? throw new ArgumentNullException(nameof(deleteAction));

            // チェック状態変更コマンドと削除コマンドを初期化
            UpdateIsCheckedCommand = new AsyncRelayCommand(ExecuteUpdateIsCheckedAsync);
            DeleteCommand = new AsyncRelayCommand(ExecuteDeleteAsync);
        }

        /// <summary>
        /// チェック状態（完了/未完了）を取得・設定します。
        /// 設定時はプロパティ変更通知とアクション呼び出しを行います。
        /// </summary>
        public bool Is_Checked
        {
            get => _toDoItem.Is_Checked;
            set
            {
                if (_toDoItem.Is_Checked == value)
                    return;

                _toDoItem.Is_Checked = value;
                OnPropertyChanged(nameof(Is_Checked));
                _ = _updateCheckAction();
            }
        }

        /// <summary>
        /// タスク名（タイトルや説明）を取得します。
        /// </summary>
        public string Task_Name => _toDoItem.Task_Name;

        /// <summary>
        /// チェック状態変更用コマンド
        /// </summary>
        public ICommand UpdateIsCheckedCommand { get; }

        /// <summary>
        /// 削除用コマンド
        /// </summary>
        public ICommand DeleteCommand { get; }

        /// <summary>
        /// チェック状態更新コマンドの実行処理
        /// </summary>
        private async Task ExecuteUpdateIsCheckedAsync()
        {
            await _updateCheckAction();
        }

        /// <summary>
        /// 削除コマンドの実行処理
        /// </summary>
        private async Task ExecuteDeleteAsync()
        {
            await _deleteAction();
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