namespace SimpleToDo.models
{
    public class ToDo
    {
        public string Table = "todo";
        public int Id { get; set; }
        public string? Task { get; set; }
        public bool  Checked { get; set; } = false;
    }
}
