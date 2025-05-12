using System.Windows.Input;

namespace SimpleToDo.utils
{
    /// <summary>
    /// パラメータなしのアクションを実行するための汎用的なICommand実装クラスです。
    /// ボタンやメニューなどのコマンドバインディングに利用できます。
    /// </summary>
    public class RelayCommand(Action execute) : ICommand
    {
        // 実行するアクションを保持
        private readonly Action _execute = execute;

        /// <summary>
        /// コマンドの実行可能状態が変化したときに発生します。
        /// WPFのCommandManagerと連携しています。
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// コマンドが実行可能かどうかを判定します。
        /// 常にtrueを返します。
        /// </summary>
        public bool CanExecute(object? parameter) => true;

        /// <summary>
        /// コマンド実行時に呼び出され、指定されたアクションを実行します。
        /// </summary>        
        public void Execute(object? parameter) => _execute();
    }
}
