using Google.Protobuf;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Steam_Community.DirectMessages.Models;
using System;

namespace Steam_Community.DirectMessages.Tests.Models
{
    [TestClass]
    public class MessageTests
    {
        [TestMethod]
        public void Constructor_CreatesEmptyMessage()
        {
            // Arrange & Act
            Message message = new Message();

            // Assert
            Assert.AreEqual(string.Empty, message.MessageContent);
            Assert.AreEqual(string.Empty, message.MessageDateTime);
            Assert.AreEqual(string.Empty, message.MessageSenderName);
            Assert.AreEqual(string.Empty, message.MessageAligment);
            Assert.AreEqual(string.Empty, message.MessageSenderStatus);
        }

        [TestMethod]
        public void Properties_SetAndGet_ReturnsExpectedValues()
        {
            // Arrange
            Message message = new Message();
            string content = "Test content";
            string dateTime = DateTime.Now.ToString();
            string senderName = "TestUser";
            string alignment = ChatConstants.ALIGNMENT_RIGHT;
            string senderStatus = ChatConstants.ADMIN_STATUS;

            // Act
            message.MessageContent = content;
            message.MessageDateTime = dateTime;
            message.MessageSenderName = senderName;
            message.MessageAligment = alignment;
            message.MessageSenderStatus = senderStatus;

            // Assert
            Assert.AreEqual(content, message.MessageContent);
            Assert.AreEqual(dateTime, message.MessageDateTime);
            Assert.AreEqual(senderName, message.MessageSenderName);
            Assert.AreEqual(alignment, message.MessageAligment);
            Assert.AreEqual(senderStatus, message.MessageSenderStatus);
        }

        [TestMethod]
        public void Clone_CreatesExactCopy()
        {
            // Arrange
            Message original = new Message
            {
                MessageContent = "Test content",
                MessageDateTime = DateTime.Now.ToString(),
                MessageSenderName = "TestUser",
                MessageAligment = ChatConstants.ALIGNMENT_LEFT,
                MessageSenderStatus = ChatConstants.HOST_STATUS
            };

            // Act
            Message clone = original.Clone();

            // Assert
            Assert.AreEqual(original.MessageContent, clone.MessageContent);
            Assert.AreEqual(original.MessageDateTime, clone.MessageDateTime);
            Assert.AreEqual(original.MessageSenderName, clone.MessageSenderName);
            Assert.AreEqual(original.MessageAligment, clone.MessageAligment);
            Assert.AreEqual(original.MessageSenderStatus, clone.MessageSenderStatus);

            // Ensure they are different objects
            Assert.AreNotSame(original, clone);
        }

        [TestMethod]
        public void Equals_SameValues_ReturnsTrue()
        {
            // Arrange
            Message message1 = new Message
            {
                MessageContent = "Test content",
                MessageDateTime = "2023-04-09T12:34:56",
                MessageSenderName = "TestUser",
                MessageAligment = ChatConstants.ALIGNMENT_LEFT,
                MessageSenderStatus = ChatConstants.REGULAR_USER_STATUS
            };

            Message message2 = new Message
            {
                MessageContent = "Test content",
                MessageDateTime = "2023-04-09T12:34:56",
                MessageSenderName = "TestUser",
                MessageAligment = ChatConstants.ALIGNMENT_LEFT,
                MessageSenderStatus = ChatConstants.REGULAR_USER_STATUS
            };

            // Act & Assert
            Assert.IsTrue(message1.Equals(message2));
            Assert.IsTrue(message2.Equals(message1));
        }

        [TestMethod]
        public void Equals_DifferentValues_ReturnsFalse()
        {
            // Arrange
            Message message1 = new Message
            {
                MessageContent = "Test content 1",
                MessageDateTime = DateTime.Now.ToString(),
                MessageSenderName = "TestUser1",
                MessageAligment = ChatConstants.ALIGNMENT_LEFT,
                MessageSenderStatus = ChatConstants.REGULAR_USER_STATUS
            };

            Message message2 = new Message
            {
                MessageContent = "Test content 2",
                MessageDateTime = DateTime.Now.ToString(),
                MessageSenderName = "TestUser2",
                MessageAligment = ChatConstants.ALIGNMENT_RIGHT,
                MessageSenderStatus = ChatConstants.ADMIN_STATUS
            };

            // Act & Assert
            Assert.IsFalse(message1.Equals(message2));
            Assert.IsFalse(message2.Equals(message1));
        }

        [TestMethod]
        public void Equals_NullOrDifferentType_ReturnsFalse()
        {
            // Arrange
            Message message = new Message();
            object differentTypeObject = new object();

            // Act & Assert
            Assert.IsFalse(message.Equals(null));
            Assert.IsFalse(message.Equals(differentTypeObject));
        }

        [TestMethod]
        public void GetHashCode_SameValues_ReturnsSameHashCode()
        {
            // Arrange
            string testContent = "Test content";
            string testDateTime = "2023-04-09T12:34:56";
            string testSenderName = "TestUser";

            Message message1 = new Message
            {
                MessageContent = testContent,
                MessageDateTime = testDateTime,
                MessageSenderName = testSenderName,
                MessageAligment = ChatConstants.ALIGNMENT_LEFT,
                MessageSenderStatus = ChatConstants.REGULAR_USER_STATUS
            };

            Message message2 = new Message
            {
                MessageContent = testContent,
                MessageDateTime = testDateTime,
                MessageSenderName = testSenderName,
                MessageAligment = ChatConstants.ALIGNMENT_LEFT,
                MessageSenderStatus = ChatConstants.REGULAR_USER_STATUS
            };

            // Act & Assert
            Assert.AreEqual(message1.GetHashCode(), message2.GetHashCode());
        }

        [TestMethod]
        public void ToByteArray_AndParseFrom_ReconstructsOriginalMessage()
        {
            // Arrange
            Message original = new Message
            {
                MessageContent = "Test serialization content",
                MessageDateTime = DateTime.Now.ToString(),
                MessageSenderName = "SerializationTestUser",
                MessageAligment = ChatConstants.ALIGNMENT_RIGHT,
                MessageSenderStatus = ChatConstants.ADMIN_STATUS
            };

            // Act
            byte[] serialized = original.ToByteArray();
            Message deserialized = Message.Parser.ParseFrom(serialized);

            // Assert
            Assert.AreEqual(original.MessageContent, deserialized.MessageContent);
            Assert.AreEqual(original.MessageDateTime, deserialized.MessageDateTime);
            Assert.AreEqual(original.MessageSenderName, deserialized.MessageSenderName);
            Assert.AreEqual(original.MessageAligment, deserialized.MessageAligment);
            Assert.AreEqual(original.MessageSenderStatus, deserialized.MessageSenderStatus);
        }
    }
}