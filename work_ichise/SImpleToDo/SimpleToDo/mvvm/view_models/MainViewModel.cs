using System.Collections.ObjectModel;

namespace SimpleToDo.mvvm.view_models
{
    public class MainViewModel
    {
        public ObservableCollection<WeatherInfoItemViewModel> WeatherInfoItems { get; set; } = [];
        public ObservableCollection<ToDoItemViewModel> ToDoItems { get; set; } = [];

        public void RemoveToDoItem(ToDoItemViewModel item)
        {
            ToDoItems.Remove(item);
        }
    }
}
