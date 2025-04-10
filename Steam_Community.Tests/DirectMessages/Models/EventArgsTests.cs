using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Steam_Community.DirectMessages.Models;

namespace Steam_Community.DirectMessages.Tests.Models
{
    [TestClass]
    public class EventArgsTests
    {
        [TestMethod]
        public void ExceptionEventArgs_Constructor_SetsExceptionProperty()
        {
            // Arrange
            Exception testException = new Exception("Test exception");

            // Act
            ExceptionEventArgs args = new ExceptionEventArgs(testException);

            // Assert
            Assert.AreEqual(testException, args.Exception);
        }

        [TestMethod]
        public void MessageEventArgs_Constructor_SetsMessageProperty()
        {
            // Arrange
            Message testMessage = new Message
            {
                MessageContent = "Test content",
                MessageSenderName = "TestUser",
                MessageDateTime = DateTime.Now.ToString(),
                MessageAligment = ChatConstants.ALIGNMENT_LEFT,
                MessageSenderStatus = ChatConstants.REGULAR_USER_STATUS
            };

            // Act
            MessageEventArgs args = new MessageEventArgs(testMessage);

            // Assert
            Assert.AreEqual(testMessage, args.Message);
        }

        [TestMethod]
        public void UserStatusEventArgs_Constructor_SetsUserStatusProperty()
        {
            // Arrange
            UserStatus testStatus = new UserStatus
            {
                IsAdmin = true,
                IsHost = false,
                IsMuted = false,
                IsConnected = true
            };

            // Act
            UserStatusEventArgs args = new UserStatusEventArgs(testStatus);

            // Assert
            Assert.AreEqual(testStatus, args.UserStatus);
        }
    }
}