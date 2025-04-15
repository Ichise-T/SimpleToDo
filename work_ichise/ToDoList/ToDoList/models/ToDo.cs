using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoList.models
{
    public class ToDo
    {
        public string Table = "todo";
        public int Id { get; set; }
        public string? Task { get; set; }
        public bool  Checked { get; set; } = false;
    }
}
