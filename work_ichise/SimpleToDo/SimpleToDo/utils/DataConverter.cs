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
        List<ToDoItem> ConvertDataTableToList(DataTable dataTable);
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
        /// <exception cref="ArgumentNullException">dataTableがnullの場合</exception>
        public List<ToDoItem> ConvertDataTableToList(DataTable dataTable)
        {
            ArgumentNullException.ThrowIfNull(dataTable);

            // 効率化のためにサイズを事前に指定してリストを初期化
            var toDoList = new List<ToDoItem>(dataTable.Rows.Count);

            // 必要なカラムが存在するか検証
            bool hasRequiredColumns = dataTable.Columns.Contains("Id") &&
                                    dataTable.Columns.Contains("Task_Name") &&
                                    dataTable.Columns.Contains("Is_Checked");

            if (!hasRequiredColumns && dataTable.Rows.Count > 0)
                throw new InvalidOperationException("必要なカラム(Id, Task_Name, Is_Checked)が見つかりません。");

            foreach (DataRow row in dataTable.Rows)
            {
                try
                {
                    toDoList.Add(new ToDoItem
                    {
                        Id = row["Id"] != DBNull.Value ? Convert.ToInt64(row["Id"]) : 0,
                        Task_Name = row["Task_Name"] != DBNull.Value ? row["Task_Name"].ToString() ?? string.Empty : string.Empty,
                        Is_Checked = row["Is_Checked"] != DBNull.Value && Convert.ToBoolean(row["Is_Checked"])
                    });
                }
                catch (Exception ex) when (ex is InvalidCastException || ex is FormatException)
                {
                    // データ型変換エラーのハンドリング - ログ出力や代替処理をここに追加可能
                    // 現在はエラー行をスキップして続行
                }
            }

            return toDoList;
        }

        /// <summary>
        /// LINQ を使用して DataTable を ToDoItem リストに変換します。
        /// 大量のデータ処理に適したパフォーマンス重視の実装です。
        /// </summary>
        /// <param name="dataTable">変換対象のDataTable</param>
        /// <returns>ToDoオブジェクトのリスト</returns>
        public static List<ToDoItem> ConvertDataTableToListLinq(DataTable dataTable)
        {
            ArgumentNullException.ThrowIfNull(dataTable);

            return dataTable.AsEnumerable()
                .Select(row => new ToDoItem
                {
                    Id = row.Field<long>("Id"),
                    Task_Name = row.Field<string>("Task_Name") ?? string.Empty,
                    Is_Checked = row.Field<bool>("Is_Checked")
                })
                .ToList();
        }
    }
}