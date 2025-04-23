namespace SimpleToDo.models
{
    public class ToDo
    {
        public long Id { get; set; }
        public string? Task { get; set; }
        public bool  Checked { get; set; } = false;
    }
}
