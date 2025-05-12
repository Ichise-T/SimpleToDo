namespace SimpleToDo.mvvm.models
{
    /// <summary>
    /// ToDoリストの1件分のデータを表すモデルクラス。
    /// タスクのID、内容、完了状態を保持します。
    /// </summary>
    public class ToDo
    {
        /// <summary>
        /// タスクの一意なID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// タスクの内容（タイトルや説明）
        /// </summary>
        public string? Task_Name { get; set; }

        /// <summary>
        /// タスクが完了しているかどうか
        /// </summary>
        public bool Is_Checked { get; set; } = false;
    }
}