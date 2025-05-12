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
        public readonly ToDo _toDo;

        /// <summary>
        /// チェック状態更新時に呼び出すアクション
        /// </summary>
        private readonly Action _updateCheckAction;

        /// <summary>
        /// 削除時に呼び出すアクション
        /// </summary>
        private readonly Action _deleteAction;
        
        /// <summary>
        /// ToDoモデル・コマンド・アクションを受け取ってViewModelを初期化します。
        /// </summary>
        public ToDoItemViewModel(ToDo toDo, Action updateCheckAction, Action deleteAction)
        {
            _toDo = toDo;
            _updateCheckAction = updateCheckAction;
            _deleteAction = deleteAction;
            UpdateIsCheckedCommand = new RelayCommand(_updateCheckAction); // チェック状態変更コマンド
            DeleteCommand = new RelayCommand(_deleteAction);               // 削除コマンド
        }
        
        /// <summary>
        /// チェック状態（完了/未完了）を取得・設定します。
        /// 設定時はプロパティ変更通知とアクション呼び出しを行います。
        /// </summary>
        public bool Is_Checked
        {
            get => _toDo.Is_Checked;
            set
            {
                if (_toDo.Is_Checked != value)
                {
                    _toDo.Is_Checked = value;
                    OnPropertyChanged(nameof(Is_Checked));
                    _updateCheckAction();
                }
            }
        }
        
        /// <summary>
        /// タスク名（タイトルや説明）を取得します。
        /// </summary>
        public string? Task_Name => _toDo.Task_Name;

        /// <summary>
        /// チェック状態変更用コマンド
        /// </summary>
        public ICommand UpdateIsCheckedCommand { get; }

        /// <summary>
        /// 削除用コマンド
        /// </summary>
        public ICommand DeleteCommand { get; }
        
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