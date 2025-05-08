using SimpleToDo.models;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using SimpleToDo.utils;

namespace SimpleToDo.components
{
    /// <summary>
    /// 単一のToDoアイテムの状態や操作（チェック・削除）を管理するViewModelクラスです。
    /// </summary>
    public class ToDoItemViewModel(ToDo toDo, Action updateCheckAction, Action deleteAction) : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public readonly ToDo _toDo = toDo;
        public bool Is_Checked
        {
            get => _toDo.Is_Checked;
            set
            {
                if (_toDo.Is_Checked != value)
                {
                    _toDo.Is_Checked = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Is_Checked)));
                    UpdateIsCheckedCommand.Execute(null);
                }
            }
        }
        public string? Task_Name => _toDo.Task_Name;
        public ICommand UpdateIsCheckedCommand { get; } = new RelayCommand(updateCheckAction);
        public ICommand DeleteCommand { get; } = new RelayCommand(deleteAction);
    }

    /// <summary>
    /// ToDoアイテムのリストを管理し、データベースとの連携やアイテムの追加・削除・更新を行うViewModelクラスです。
    /// </summary>
    public class ToDoItemListViewModel
    {
        public ObservableCollection<ToDoItemViewModel> ToDoItems { get; }
        private readonly string _databaseName;
        private readonly string _tableName;
        private readonly Action<string, string, long, object>? _updateRecord;
        private readonly Action<string, string, long>? _deleteRecord;

        public ToDoItemListViewModel(string databaseName, string tableName, List<ToDo> toDoList,
            Action<string, string, long, object>? updateRecord,
            Action<string, string, long>? deleteRecord)
        {
            _databaseName = databaseName;
            _tableName = tableName;
            _updateRecord = updateRecord;
            _deleteRecord = deleteRecord;

            ToDoItems = [.. toDoList.Select(toDo =>
                    new ToDoItemViewModel(
                        toDo,
                        () => UpdateCheck(toDo),
                        () => DeleteToDoItem(toDo)
                    )
                )];
        }

        private void UpdateCheck(ToDo toDo)
        {
            _updateRecord?.Invoke(_databaseName, _tableName, toDo.Id, new ToDo
            {
                Task_Name = toDo.Task_Name,
                Is_Checked = toDo.Is_Checked
            });
        }

        private void DeleteToDoItem(ToDo toDo)
        {
            var task = ToDoItems.FirstOrDefault(x => x._toDo.Id == toDo.Id);
            if (task != null)
            {
                ToDoItems.Remove(task);
                _deleteRecord?.Invoke(_databaseName, _tableName, toDo.Id);
            }
        }
    } 
}