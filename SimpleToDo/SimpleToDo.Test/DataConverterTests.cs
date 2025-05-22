using System.Data;
using SimpleToDo.mvvm.models;
using SimpleToDo.utils;

namespace SimpleToDo.Test
{
    /// <summary>
    /// ユーティリティクラスの単体テストをまとめたクラス。
    /// </summary>
    public class DataConverterTests
    {
        /// <summary>
        /// DataTableからToDoリストへの変換が正しく行われるかをテストします。
        /// </summary>
        [Fact(DisplayName = "DataTableからListへの変換")]
        public void ConvertDataTableToListShouldReturnCorrectList()
        {
            // Arrange: テスト用のDataTableを作成し、サンプルデータを追加
            DataTable dataTable = new();
            dataTable.Columns.Add("Id", typeof(int));
            dataTable.Columns.Add("Task_Name", typeof(string));
            dataTable.Columns.Add("Is_Checked", typeof(bool));

            dataTable.Rows.Add(1, "Task 1", true);
            dataTable.Rows.Add(2, "Task 2", false);

            // Act: DataConverterでDataTableをToDoリストに変換
            List<ToDoItem> result = new DataConverter().ConvertDataTableToList(dataTable);

            // Assert: 変換結果が期待通りか検証
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Task 1", result[0].Task_Name);
            Assert.True(result[0].Is_Checked);
            Assert.Equal(2, result[1].Id);
            Assert.Equal("Task 2", result[1].Task_Name);
            Assert.False(result[1].Is_Checked);
        }
    }
}