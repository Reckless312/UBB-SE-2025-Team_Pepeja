using Microsoft.VisualStudio.TestTools.UnitTesting;
using News;
using Moq;
using System.Data;
using System.Data.SqlClient;
using Search;
using Microsoft.UI.Xaml.Controls;


namespace TestNews
{
    [TestClass]
    public sealed class NewsDatabaseTests
    {
        private INewsDatabase _connection;

        [TestInitialize]
        public void SetUp()
        {
            _connection = new NewsDatabase();
        }

        [TestMethod]
        public void GetConnection_ReturnsValidConnection()
        {
            // Act
            SqlConnection connection = _connection.GetConnection();

            // Assert
            Assert.IsNotNull(connection, "Connection should not be null value.");
            Assert.AreEqual(ConnectionState.Closed, connection.State, "Connection should be closed.");
        }

        [TestMethod]
        public void Connect_WithClosedConnection_OpensConnection()
        {
            // Arrange
            using (var connection = _connection.GetConnection())
            {
                // Assert precondition
                Assert.AreEqual(ConnectionState.Closed, connection.State, "Connection should be closed.");

                // Act
                _connection.Connect(connection);

                // Assert
                Assert.AreEqual(ConnectionState.Open, connection.State, "Connection should be open if connection was closed before.");

                connection.Close();
            }
        }

        [TestMethod]
        public void Connect_WithOpenConnection_DoesNothing()
        {
            // Arrange
            using (var connection = _connection.GetConnection())
            {
                connection.Open();
                // Assert precondition
                Assert.AreEqual(ConnectionState.Open, connection.State, "Connection should be open.");

                // Act

                // Assert
                Assert.AreEqual(ConnectionState.Open, connection.State, "Connection should be open if connection was already open.");

                connection.Close();
            }
        }

        [TestMethod]
        public void Connect_WithInvalidConnection_ThrowsException()
        {
            // Arrange
            using (var invalidConnection = new SqlConnection("Server=BAD_SERVER;Database=NonExistentDB;Integrated Security=True;"))
            {
                // Act and Assert
                Assert.ThrowsException<SqlException>(() => _connection.Connect(invalidConnection));
            }
        }

        [TestMethod]
        public void Disconnect_WithOpenConnection_ClosesConnection()
        {
            // Arrange
            using (var connection = _connection.GetConnection())
            {
                _connection.Connect(connection);
                Assert.AreEqual(ConnectionState.Open, connection.State, "Connection should be open.");

                // Act
                _connection.Disconnect(connection);

                // Assert
                Assert.AreEqual(ConnectionState.Closed, connection.State, "Connection should be closed if it was open before.");
            }
        }
    }

    [TestClass]
    public sealed class NewsServiceTests
    {
        private Mock<INewsDatabase> _mockDatabase;
        private News.Service _service;
        private Post validFakePost = new Post
        {
            Id = 99,
            AuthorId = 1,
            Content = "",
            UploadDate = DateTime.Now,
            NrLikes = 0,
            NrDislikes = 0,
            NrComments = 0,
            ActiveUserRating = true
        };

        private Post invalidFakePost = new Post
        {
            Id = -1,
            AuthorId = -1,
            Content = "",
            UploadDate = DateTime.Now,
            NrLikes = 0,
            NrDislikes = 0,
            NrComments = 0,
            ActiveUserRating = true
        };

        [TestInitialize]
        public void SetUp()
        {
            _mockDatabase = new Mock<INewsDatabase>();
            _service = News.Service.Initialize(_mockDatabase.Object);
        }

        [TestMethod]
        public void LikePost_WithValidPostId_ReturnsTrue()
        {
            // Arrange
            var post = validFakePost;

            // Act
            var result = _service.LikePost(post.Id);

            // Assert
            Assert.IsTrue(result, "Result should be true for valid postId.");
        }

        [TestMethod]
        public void LikePost_WithInvalidPostId_ReturnsFalse()
        {
            // Arrange
            var post = invalidFakePost;

            // Act
            var result = _service.LikePost(post.Id);

            // Assert
            Assert.IsFalse(result, "Result should be false for invalid postId.");
        }

        [TestMethod]
        public void FormatAsPost_ShouldWrapTextInHtmlTemplate()
        {
            // Arrange
            string input = "Hello, <spoiler>secret</spoiler> world!";

            // Act
            string result = _service.FormatAsPost(input);

            // Assert: verify that the template is applied and the text is preserved.
            Assert.IsTrue(result.StartsWith("<html>"), "Output should be wrapped in HTML.");
            Assert.IsTrue(result.Contains("<body>"), "Output should contain a body tag.");
            Assert.IsTrue(result.Contains("class=\"spoiler\""), "Spoiler should be converted to a span with a spoiler class.");
            Assert.IsTrue(result.Contains("Hello"), "The formatted text should contain the original text.");
        }
    }
    

    [TestClass]
    public sealed class NewsUsersTests
    {
        [TestMethod]
        public void GetUserById_ExistingId_ReturnsCorrectUser()
        {
            // Arrange
            var users = Users.Instance;
            int existingId = 1;

            // Act
            var user = users.GetUserById(existingId);

            // Assert
            Assert.IsNotNull(user, "Expected a user for a valid id.");
            Assert.AreEqual(existingId, user.id, "The returned user id does not match the expected id.");
        }

        [TestMethod]
        public void GetUserById_NotExistingId_ReturnsNull()
        {
            // Arrange
            var users = Users.Instance;
            int notExistingId = -23;

            // Act
            var user = users.GetUserById(notExistingId);

            // Assert
            Assert.IsNull(user, "Expected null for an id that does not exist.");
        }
    }
}
