using System.Data;
using SimpleToDo.mvvm.models;

namespace SimpleToDo.utils
{
    public interface IDataConvert
    {
        public List<ToDo> ConvertDataTableToList(DataTable dataTable);
    }

    public class DataConverter : IDataConvert
    {
        public List<ToDo> ConvertDataTableToList(DataTable dataTable)
        {
            List<ToDo> toDoList = [];

            foreach (DataRow row in dataTable.Rows)
            {
                ToDo toDo = new()
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Task_Name = row["Task_Name"].ToString(),
                    Is_Checked = Convert.ToBoolean(row["Is_Checked"]),
                };
                toDoList.Add(toDo);
            }

            return toDoList;
        }
    }
}
