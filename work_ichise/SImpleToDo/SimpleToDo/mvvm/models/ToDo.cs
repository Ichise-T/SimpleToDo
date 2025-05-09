namespace SimpleToDo.mvvm.models
{
    public class ToDo
    {
        public long Id { get; set; }
        public string? Task_Name { get; set; }
        public bool  Is_Checked { get; set; } = false;
    }
}
