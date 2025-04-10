using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamCommunity.Reviews.Database;
using System.Data.SqlClient;

namespace SteamCommunity.ReviewsTests
{
    [TestClass]
    public class DatabaseConnectionTests
    {
        private IDatabaseConnection _dbConnection;

        [TestInitialize]
        public void SetUp()
        {
            _dbConnection = new DatabaseConnection();
        }

        [TestMethod]
        public void GetConnection_ShouldReturnValidSqlConnection()
        {
            // Arrange

            // Act
            SqlConnection connection = _dbConnection.GetConnection();

            // Assert
            Assert.IsNotNull(connection);
            Assert.AreEqual(System.Data.ConnectionState.Closed, connection.State);
        }

        [TestMethod]
        public void Connect_ShouldOpenConnection()
        {
            // Arrange
            using SqlConnection connection = _dbConnection.GetConnection();

            // Act
            _dbConnection.Connect(connection);

            // Assert
            Assert.AreEqual(System.Data.ConnectionState.Open, connection.State);

            // Cleanup
            _dbConnection.Disconnect(connection);
        }

        [TestMethod]
        public void Disconnect_ShouldCloseConnection()
        {
            // Arrange
            using SqlConnection connection = _dbConnection.GetConnection();
            _dbConnection.Connect(connection);

            // Act
            _dbConnection.Disconnect(connection);

            // Assert
            Assert.AreEqual(System.Data.ConnectionState.Closed, connection.State);
        }
    }
}
