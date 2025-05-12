using System.Data;
using SimpleToDo.mvvm.models;
using SimpleToDo.utils;

namespace SimpleToDo.Test
{
    public class UtilTests
    {
        [Fact(DisplayName = "DataTableからListへの変換")]
        public void ConvertDataTableToListShouldReturnCorrectList()
        {
            // Arrange
            DataTable dataTable = new();
            dataTable.Columns.Add("Id", typeof(int));
            dataTable.Columns.Add("Task_Name", typeof(string));
            dataTable.Columns.Add("Is_Checked", typeof(bool));

            dataTable.Rows.Add(1, "Task 1", true);
            dataTable.Rows.Add(2, "Task 2", false);

            // Act
            List<ToDo> result = new DataConverter().ConvertDataTableToList(dataTable);

            // Assert
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