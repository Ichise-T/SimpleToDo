using Moq;
using MySql.Data.MySqlClient;
using SimpleToDo.services;

namespace SimpleToDo.Test
{
    public class DatabaseCrudManagerTests
    {
        [Fact(DisplayName = "Recordの上書き")]
        public static void UpdateRecordShouldUpdateRecordInDatabase()
        {
            // Arrange
            var mockConnection = new Mock<MySqlConnection>();
            var mockCommand = new Mock<MySqlCommand>();
            var mockConnectionFactory = new Mock<MySqlConnection>();

            mockConnectionFactory.Setup(f => f).Returns(mockConnection.Object);
            mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);
        }
    }
}
