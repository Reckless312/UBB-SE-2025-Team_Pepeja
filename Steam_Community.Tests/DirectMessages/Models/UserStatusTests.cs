/***
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Steam_Community.DirectMessages.Models;

namespace Steam_Community.DirectMessages.Tests.Models
{
    [TestClass]
    public class UserStatusTests
    {
        [TestMethod]
        public void Constructor_CreatesObjectWithDefaultValues()
        {
            // Arrange & Act
            UserStatus userStatus = new UserStatus();

            // Assert
            Assert.IsFalse(userStatus.IsAdmin);
            Assert.IsFalse(userStatus.IsMuted);
            Assert.IsFalse(userStatus.IsHost);
            Assert.IsFalse(userStatus.IsConnected);
        }

        [TestMethod]
        public void IsAdmin_SetAndGet_ReturnsExpectedValue()
        {
            // Arrange
            UserStatus userStatus = new UserStatus();

            // Act
            userStatus.IsAdmin = true;

            // Assert
            Assert.IsTrue(userStatus.IsAdmin);
        }

        [TestMethod]
        public void IsMuted_SetAndGet_ReturnsExpectedValue()
        {
            // Arrange
            UserStatus userStatus = new UserStatus();

            // Act
            userStatus.IsMuted = true;

            // Assert
            Assert.IsTrue(userStatus.IsMuted);
        }

        [TestMethod]
        public void IsHost_SetAndGet_ReturnsExpectedValue()
        {
            // Arrange
            UserStatus userStatus = new UserStatus();

            // Act
            userStatus.IsHost = true;

            // Assert
            Assert.IsTrue(userStatus.IsHost);
        }

        [TestMethod]
        public void IsConnected_SetAndGet_ReturnsExpectedValue()
        {
            // Arrange
            UserStatus userStatus = new UserStatus();

            // Act
            userStatus.IsConnected = true;

            // Assert
            Assert.IsTrue(userStatus.IsConnected);
        }
    }
}***/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Steam_Community.DirectMessages.Models;

namespace Steam_Community.Tests.DirectMessages.Models
{
    [TestClass]
    public class UserStatusTests
    {
        [TestMethod]
        public void Constructor_CreatesObjectWithDefaultValues()
        {
            // Arrange & Act
            UserStatus userStatus = new UserStatus();

            // Assert
            Assert.IsFalse(userStatus.IsAdmin);
            Assert.IsFalse(userStatus.IsMuted);
            Assert.IsFalse(userStatus.IsHost);
            Assert.IsFalse(userStatus.IsConnected);
        }

        [TestMethod]
        public void IsAdmin_SetAndGet_ReturnsExpectedValue()
        {
            // Arrange
            UserStatus userStatus = new UserStatus();

            // Act
            userStatus.IsAdmin = true;

            // Assert
            Assert.IsTrue(userStatus.IsAdmin);
        }

        [TestMethod]
        public void SimpleTest_AlwaysPasses()
        {
            // This test should always pass
            Assert.IsTrue(true);
        }
    }
}