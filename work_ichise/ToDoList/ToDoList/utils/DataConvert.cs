using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoList.models;

namespace ToDoList.utils
{
    interface IDataConvert
    {
        List<ToDo> ConvertDataTableToList(DataTable dataTable);
    }

    class DataConverter : IDataConvert
    {
        public List<ToDo> ConvertDataTableToList(DataTable dataTable)
        {
            List<ToDo> toDoList = [];

            foreach (DataRow row in dataTable.Rows)
            {
                ToDo toDo = new()
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Task = row["Task"].ToString(),
                    Checked = Convert.ToBoolean(row["Checked"]),
                };
                toDoList.Add(toDo);
            }

            return toDoList;
        }
    }
}
