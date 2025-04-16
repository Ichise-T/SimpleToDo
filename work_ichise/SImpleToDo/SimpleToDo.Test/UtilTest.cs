using System.Data;
using SimpleToDo.models;
using SimpleToDo.utils;
using System.Collections.Generic;
using Xunit;
using MaterialDesignThemes.Wpf.Converters;
using MySqlX.XDevAPI.Common;


namespace SimpleToDo.Test
{
    public class UtilTest
    {
        [Fact(DisplayName = "DataTable‚©‚çList‚Ö‚Ì•ÏŠ·")]
        public void ConvertDataTableToListShouldReturnCorrectList()
        {
            // Arrange
            DataTable dataTable = new();
            dataTable.Columns.Add("Id", typeof(int));
            dataTable.Columns.Add("Task", typeof(string));
            dataTable.Columns.Add("Checked", typeof(bool));

            dataTable.Rows.Add(1, "Task 1", true);
            dataTable.Rows.Add(2, "Task 2", false);

            // Act
            List<ToDo> result = new DataConverter().ConvertDataTableToList(dataTable);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Task 1", result[0].Task);
            Assert.True(result[0].Checked);
            Assert.Equal(2, result[1].Id);
            Assert.Equal("Task 2", result[1].Task);
            Assert.False(result[1].Checked);
        }
    }
}