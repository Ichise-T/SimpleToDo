using System.Windows.Input;

namespace SimpleToDo.utils
{    
    /// <summary>
    /// 非同期処理に対応した汎用的なICommand実装クラスです。
    /// 非同期操作を実行するボタンやメニューなどに使用できます。
    /// </summary>
    /// <remarks>
    /// 非同期処理を実行するAsyncRelayCommandを初期化します。
    /// </remarks>
    /// <param name="executeAsync">実行する非同期処理</param>
    public class AsyncRelayCommand(Func<Task> executeAsync) : ICommand
    {
        // 実行する非同期処理
        private readonly Func<Task> _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
        // 非同期処理実行中フラグ
        private bool _isExecuting;

    /// <summary>
    /// コマンドの実行可能状態が変化したときに発生します。
    /// </summary>
    public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// コマンド実行可能状態を通知します。
        /// </summary>
        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// コマンドが実行可能かどうかを判定します。
        /// 既に実行中の場合はfalseを返します。
        /// </summary>
        public bool CanExecute(object? parameter) => !_isExecuting;

        /// <summary>
        /// コマンド実行時に呼び出され、非同期処理を開始します。
        /// 既に実行中の場合は何もしません。
        /// </summary>
        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            try
            {
                _isExecuting = true;
                OnCanExecuteChanged();
                
                await _executeAsync();
            }
            finally
            {
                _isExecuting = false;
                OnCanExecuteChanged();
            }
        }
    }    
}