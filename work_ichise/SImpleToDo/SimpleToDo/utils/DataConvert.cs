using System.Data;
using SimpleToDo.models;

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
                    Task = row["Task"].ToString(),
                    Checked = Convert.ToBoolean(row["Checked"]),
                };
                toDoList.Add(toDo);
            }

            return toDoList;
        }
    }
}
