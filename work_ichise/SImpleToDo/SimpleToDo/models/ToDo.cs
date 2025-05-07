namespace SimpleToDo.models
{
    public class ToDo
    {
        public long Id { get; set; }
        public string? TaskName { get; set; }
        public bool  IsChecked { get; set; } = false;
    }
}
