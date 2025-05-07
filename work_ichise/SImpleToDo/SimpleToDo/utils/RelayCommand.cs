using System.Windows.Input;

namespace SimpleToDo.utils
{
    /// <summary>
    /// パラメータなしのアクションを実行するための汎用的なICommand実装クラスです。
    /// ボタンやメニューなどのコマンドバインディングに利用できます。
    /// </summary>
    public class RelayCommand(Action execute) : ICommand
    {
        private readonly Action _execute = execute;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute();
    }
}
