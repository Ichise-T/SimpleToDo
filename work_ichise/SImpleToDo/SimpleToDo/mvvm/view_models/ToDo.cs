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
        public event PropertyChangedEventHandler? PropertyChanged;
        public readonly ToDo _toDo;
        private readonly Action _updateCheckAction;
        private readonly Action _deleteAction;
        
        public ToDoItemViewModel(ToDo toDo, Action updateCheckAction, Action deleteAction)
        {
            _toDo = toDo;
            _updateCheckAction = updateCheckAction;
            _deleteAction = deleteAction;
            UpdateIsCheckedCommand = new RelayCommand(_updateCheckAction);
            DeleteCommand = new RelayCommand(_deleteAction);
        }
        
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
        
        public string? Task_Name => _toDo.Task_Name;
        public ICommand UpdateIsCheckedCommand { get; }
        public ICommand DeleteCommand { get; }
        
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}