using Moq;
using SimpleToDo.services;
using System.Data;
using SimpleToDo.models;

namespace SimpleToDo.Test
{
    /// <summary>
    /// DatabaseCrudManagerの各メソッドの動作を検証する単体テストクラスです。
    /// </summary>
    public class DatabaseCrudManagerTests
    {
        /// <summary>
        /// UpdateRecordメソッドが正しいSQLクエリを構築し、適切なパラメータを追加しているかを検証します。
        /// </summary>
        [Fact(DisplayName = "UpdateRecordは正しいクエリを構築し、パラメータを追加する必要がある")]
        public static void UpdateRecordShouldConstructCorrectQueryAndAddParameters()
        {
            //----------------------
            // Arrange：テスト準備
            //----------------------
            var mockConnection = new Mock<IDbConnection>();
            var mockCommand = new Mock<IDbCommand>();
            mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);
            var mockParameters = new Mock<IDataParameterCollection>();
            mockCommand.Setup(c => c.Parameters).Returns(mockParameters.Object);

            var connectionFactory = new Func<IDbConnection>(() => mockConnection.Object);
            var dbManager = new DatabaseCrudManager(connectionFactory);

            ToDo testToDoRecord = new()
            {
                TaskName = "Test Task",
                IsChecked = true
            };
            string databaseName = "test_todo";
            string tableName = "todo";
            long recordId = 1;

            // Mock command behavior：モックコマンドの動作を設定
            mockCommand.SetupProperty(c => c.CommandText);
            mockCommand.Setup(c => c.CreateParameter()).Returns(() =>
            {
                // CreateParameterがモックされたIDbDataParameterを返す
                var parameter = new Mock<IDbDataParameter>();
                parameter.SetupProperty(p => p.ParameterName); // ParameterNameをプロパティの設定可能にする*
                parameter.SetupProperty(p => p.Value); // Valueをプロパティの設定可能にする*
                return parameter.Object;
            });                          
            
            // Mock Parameters.Add behavior：Parameters.Addの動作を設定
            mockParameters.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

            //----------------------------------
            // Act：テスト対象のメソッドを実行
            //----------------------------------
            dbManager.UpdateRecord(databaseName, tableName, recordId, testToDoRecord);

            //---------------------
            // Assert：結果を検証
            //---------------------
            // Verify the SQL query
            var expectedQuery = $"UPDATE {tableName} SET task_name = @task_name, is_checked = @is_checked WHERE id = @id";
            Assert.Equal(expectedQuery, mockCommand.Object.CommandText);

            // Verify the parameters
            mockCommand.Verify(c => c.CreateParameter(), Times.Exactly(3)); // 3つのパラメータが作成されることを確認
            mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once); // ExecuteNonQueryが1回呼ばれることを確認

            // Verify parameter names and values
            mockParameters.Verify(p => p.Add(It.Is<IDbDataParameter>(param =>
                param.ParameterName == "@task_name" && param.Value != null && param.Value.ToString() == "Test task_name")), Times.Once);

            mockParameters.Verify(p => p.Add(It.Is<IDbDataParameter>(param =>
                param.ParameterName == "@is_checked" && param.Value != null && (bool)param.Value == true)), Times.Once);

            mockParameters.Verify(p => p.Add(It.Is<IDbDataParameter>(param =>
                param.ParameterName == "@id" && param.Value != null && (long)param.Value == 1)), Times.Once);
        }           
    }
}
