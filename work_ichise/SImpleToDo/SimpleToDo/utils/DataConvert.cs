using System.Data;
using SimpleToDo.mvvm.models;

namespace SimpleToDo.utils
{
    /// <summary>
    /// DataTableからToDoリストへの変換インターフェース
    /// </summary>
    public interface IDataConvert
    {
        /// <summary>
        /// DataTableをToDoオブジェクトのリストに変換します。
        /// </summary>
        /// <param name="dataTable">変換対象のDataTable</param>
        /// <returns>ToDoオブジェクトのリスト</returns>
        public List<ToDo> ConvertDataTableToList(DataTable dataTable);
    }

    /// <summary>
    /// DataTableをToDoリストに変換する実装クラス
    /// </summary>
    public class DataConverter : IDataConvert
    {
        /// <summary>
        /// DataTableの各行をToDoオブジェクトに変換し、リストとして返します。
        /// </summary>
        /// <param name="dataTable">変換対象のDataTable</param>
        /// <returns>ToDoオブジェクトのリスト</returns>
        public List<ToDo> ConvertDataTableToList(DataTable dataTable)
        {
            List<ToDo> toDoList = [];

            foreach (DataRow row in dataTable.Rows)
            {
                // DataRowからToDoオブジェクトを生成
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
