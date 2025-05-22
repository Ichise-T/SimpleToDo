using Moq;
using SimpleToDo.services.database;
using System.Data;
using SimpleToDo.mvvm.models;
using SimpleToDo.services.database.interfaces;

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
        public static async Task UpdateRecordShouldConstructCorrectQueryAndAddParameters()
        {
            // Arrange: モックのDB接続・コマンド・パラメータコレクションを用意
            var mockConnection = new Mock<IDbConnectionWrapper>();
            var mockCommand = new Mock<IDbCommandWrapper>();
            var mockParameters = new Mock<IDataParameterCollection>();

            // コマンド生成時にモックコマンドを返す
            mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);
            // コマンドのパラメータコレクションにモックを返す
            mockCommand.Setup(c => c.Parameters).Returns(mockParameters.Object);

            // DatabaseCrudManager用のファクトリ関数
            var connectionFactory = new Func<Task<IDbConnectionWrapper>>(() =>
                Task.FromResult(mockConnection.Object));
            var dbManager = new DatabaseCrudManager(connectionFactory);

            // テスト用ToDoレコード
            ToDoItem testToDoRecord = new()
            {
                Task_Name = "test task 10",
                Is_Checked = true
            };
            string databaseName = "test_todo";
            string tableName = "todo";
            long recordId = 1;

            // コマンドテキストのセットアップ
            mockCommand.SetupProperty(c => c.CommandText);
            // パラメータ生成時にモックパラメータを返す
            mockCommand.Setup(c => c.CreateParameter()).Returns(() =>
            {
                var parameter = new Mock<IDbDataParameter>();
                parameter.SetupProperty(p => p.ParameterName);
                parameter.SetupProperty(p => p.Value);
                return parameter.Object;
            });

            // パラメータ追加時の戻り値
            mockParameters.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

            // Act: UpdateRecordAsyncを実行
            await dbManager.UpdateRecordAsync(databaseName, tableName, recordId, testToDoRecord);

            // Assert: クエリやパラメータ追加が正しいか検証
            var expectedQuery = $"UPDATE {tableName} SET task_name = @Task_Name, is_checked = @Is_Checked WHERE id = @Id";
            Assert.Equal(expectedQuery, mockCommand.Object.CommandText);

            // パラメータ生成が3回呼ばれているか
            mockCommand.Verify(c => c.CreateParameter(), Times.Exactly(3));
            // 非同期実行が1回呼ばれているか
            mockCommand.Verify(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()), Times.Once);

            // 各パラメータが正しく追加されたか
            mockParameters.Verify(p => p.Add(It.Is<IDbDataParameter>(param =>
                param.ParameterName == "@Task_Name" && param.Value != null && param.Value.ToString() == "test task 10")), Times.Once);

            mockParameters.Verify(p => p.Add(It.Is<IDbDataParameter>(param =>
                param.ParameterName == "@Is_Checked" && param.Value != null && (bool)param.Value == true)), Times.Once);

            mockParameters.Verify(p => p.Add(It.Is<IDbDataParameter>(param =>
                param.ParameterName == "@Id" && param.Value != null && (long)param.Value == 1)), Times.Once);
        }
    }
}