using System;
using System.Reflection;
using Moq;
using Microsoft.UI.Dispatching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Steam_Community.DirectMessages.Models;
using Steam_Community.DirectMessages.Services;
using Steam_Community.DirectMessages.Tests.Mocks;

namespace Steam_Community.DirectMessages.Tests.Services
{
    [TestClass]
    public class ChatServiceTests
    {
        private ChatService chatService;
        private MockNetworkClient mockNetworkClient;
        private MockNetworkServer mockNetworkServer;
        private DispatcherQueue dispatcherQueue;
        private const string TestUsername = "TestUser";
        private const string TestLocalIp = "192.168.1.1";
        private const string TestServerIp = "192.168.1.2";

        [TestInitialize]
        public void Initialize()
        {

            // Create a mock DispatcherQueue
            var mockDispatcherQueue = new Mock<DispatcherQueue>();
            mockDispatcherQueue
                .Setup(x => x.TryEnqueue(It.IsAny<DispatcherQueueHandler>()))
                .Returns(true)
                .Callback<DispatcherQueueHandler>(handler => handler());

            dispatcherQueue = mockDispatcherQueue.Object;

            // Setup mocks
            mockNetworkClient = new MockNetworkClient();
            mockNetworkServer = new MockNetworkServer();

            // Get real dispatcher queue
            //dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            // Create chat service
            chatService = new ChatService(TestUsername, TestServerIp, dispatcherQueue);

            // Override private fields
            SetPrivateField(chatService, "_networkClient", mockNetworkClient);
            SetPrivateField(chatService, "_userIpAddress", TestLocalIp);
        }

        [TestMethod]
        public void Constructor_SetsCorrectValues()
        {
            // Assert - Use reflection to check private fields
            Assert.AreEqual(TestUsername, GetPrivateField<string>(chatService, "_username"));
            Assert.AreEqual(TestServerIp, GetPrivateField<string>(chatService, "_serverInviteIpAddress"));
            Assert.AreEqual(dispatcherQueue, GetPrivateField<DispatcherQueue>(chatService, "_uiDispatcherQueue"));
        }

        [TestMethod]
        public void ConnectToServer_AsClient_ConnectsClient()
        {
            // Act
            chatService.ConnectToServer();

            // Assert
            Assert.IsTrue(mockNetworkClient.ConnectCalled);
            Assert.IsFalse(mockNetworkClient.SetAsHostCalled);
        }

        [TestMethod]
        public void ConnectToServer_AsHost_CreatesServerAndConnectsClient()
        {
            // Arrange
            ChatService hostChatService = new ChatService(TestUsername, ChatConstants.HOST_IP_FINDER, dispatcherQueue);

            // Setup mocks in the host service
            SetPrivateField(hostChatService, "_networkClient", mockNetworkClient);
            SetPrivateField(hostChatService, "_networkServer", mockNetworkServer);
            SetPrivateField(hostChatService, "_userIpAddress", TestLocalIp);

            // Act
            hostChatService.ConnectToServer();

            // Assert
            Assert.IsTrue(mockNetworkClient.ConnectCalled);
            Assert.IsTrue(mockNetworkClient.SetAsHostCalled);
            // Can't verify mockNetworkServer.StartCalled due to the way we're setting up the test
        }

        [TestMethod]
        public void SendMessage_ValidMessage_SendsToClient()
        {
            // Arrange
            string testMessage = "Test message content";
            mockNetworkClient.IsConnectedValue = true;

            // Act
            chatService.SendMessage(testMessage);

            // Assert
            Assert.AreEqual(1, mockNetworkClient.SentMessages.Count);
            Assert.AreEqual(testMessage, mockNetworkClient.SentMessages[0]);
        }

        [TestMethod]
        public void SendMessage_EmptyMessage_ReportsException()
        {
            // Arrange
            string emptyMessage = "";
            bool exceptionReported = false;

            chatService.ExceptionOccurred += (sender, args) => {
                exceptionReported = true;
                Assert.IsTrue(args.Exception.Message.Contains("empty"));
            };

            // Act
            chatService.SendMessage(emptyMessage);

            // Assert
            Assert.IsTrue(exceptionReported);
        }

        [TestMethod]
        public void SendMessage_ClientNotConnected_ReportsException()
        {
            // Arrange
            string testMessage = "Test message";
            mockNetworkClient.IsConnectedValue = false;
            bool exceptionReported = false;

            chatService.ExceptionOccurred += (sender, args) => {
                exceptionReported = true;
                Assert.IsTrue(args.Exception.Message.Contains("not connected"));
            };

            // Act
            chatService.SendMessage(testMessage);

            // Assert
            Assert.IsTrue(exceptionReported);
        }

        [TestMethod]
        public void SendMessage_ServerNotRunning_ReportsException()
        {
            // Arrange
            string testMessage = "Test message";
            mockNetworkClient.IsConnectedValue = true;
            mockNetworkServer.IsRunningValue = false;

            // Set the network server field
            SetPrivateField(chatService, "_networkServer", mockNetworkServer);

            bool exceptionReported = false;
            chatService.ExceptionOccurred += (sender, args) => {
                exceptionReported = true;
                Assert.IsTrue(args.Exception.Message.Contains("timeout"));
            };

            // Act
            chatService.SendMessage(testMessage);

            // Assert
            Assert.IsTrue(exceptionReported);
        }

        [TestMethod]
        public void HandleMessageReceived_SetsMessageAlignmentBasedOnSender()
        {
            // Arrange
            Message messageFromCurrentUser = new Message { MessageSenderName = TestUsername };
            Message messageFromOtherUser = new Message { MessageSenderName = "OtherUser" };

            bool messageEventRaised = false;
            Message receivedMessage = null;

            chatService.MessageReceived += (sender, args) => {
                messageEventRaised = true;
                receivedMessage = args.Message;
            };

            // Act - Test message from current user
            mockNetworkClient.TriggerMessageReceived(messageFromCurrentUser);

            // Assert
            Assert.IsTrue(messageEventRaised);
            Assert.AreEqual(ChatConstants.ALIGNMENT_RIGHT, receivedMessage.MessageAligment);

            // Reset
            messageEventRaised = false;
            receivedMessage = null;

            // Act - Test message from other user
            mockNetworkClient.TriggerMessageReceived(messageFromOtherUser);

            // Assert
            Assert.IsTrue(messageEventRaised);
            Assert.AreEqual(ChatConstants.ALIGNMENT_LEFT, receivedMessage.MessageAligment);
        }

        [TestMethod]
        public void DisconnectFromServer_CallsClientDisconnect()
        {
            // Act
            chatService.DisconnectFromServer();

            // Assert
            Assert.IsTrue(mockNetworkClient.DisconnectCalled);
        }

        [TestMethod]
        public void AttemptChangeMuteStatus_SendsCorrectCommand()
        {
            // Arrange
            string targetUsername = "TargetUser";
            mockNetworkClient.IsConnectedValue = true;

            // Act
            chatService.AttemptChangeMuteStatus(targetUsername);

            // Assert
            Assert.AreEqual(1, mockNetworkClient.SentMessages.Count);
            string expectedCommand = $"<{TestUsername}>|{ChatConstants.MUTE_STATUS}|<{targetUsername}>";
            Assert.AreEqual(expectedCommand, mockNetworkClient.SentMessages[0]);
        }

        [TestMethod]
        public void AttemptChangeAdminStatus_SendsCorrectCommand()
        {
            // Arrange
            string targetUsername = "TargetUser";
            mockNetworkClient.IsConnectedValue = true;

            // Act
            chatService.AttemptChangeAdminStatus(targetUsername);

            // Assert
            Assert.AreEqual(1, mockNetworkClient.SentMessages.Count);
            string expectedCommand = $"<{TestUsername}>|{ChatConstants.ADMIN_STATUS}|<{targetUsername}>";
            Assert.AreEqual(expectedCommand, mockNetworkClient.SentMessages[0]);
        }

        [TestMethod]
        public void AttemptKickUser_SendsCorrectCommand()
        {
            // Arrange
            string targetUsername = "TargetUser";
            mockNetworkClient.IsConnectedValue = true;

            // Act
            chatService.AttemptKickUser(targetUsername);

            // Assert
            Assert.AreEqual(1, mockNetworkClient.SentMessages.Count);
            string expectedCommand = $"<{TestUsername}>|{ChatConstants.KICK_STATUS}|<{targetUsername}>";
            Assert.AreEqual(expectedCommand, mockNetworkClient.SentMessages[0]);
        }

        [TestMethod]
        public void HandleUserStatusChanged_ForwardsStatusToUI()
        {
            // Arrange
            UserStatus userStatus = new UserStatus { IsAdmin = true, IsHost = true };

            bool statusChangedEventRaised = false;
            UserStatus reportedStatus = null;

            chatService.UserStatusChanged += (sender, args) => {
                statusChangedEventRaised = true;
                reportedStatus = args.UserStatus;
            };

            // Act
            mockNetworkClient.TriggerUserStatusChanged(userStatus);

            // Assert
            Assert.IsTrue(statusChangedEventRaised);
            Assert.AreEqual(userStatus, reportedStatus);
        }

        [TestMethod]
        public void GetLocalIpAddress_ReturnsIpOrDefault()
        {
            // Act
            string ipAddress = ChatService.GetLocalIpAddress();

            // Assert
            if (ChatConstants.DEBUG_MODE)
            {
                // In debug mode, it should return the debug host IP
                Assert.AreEqual(ChatConstants.DEBUG_HOST_IP, ipAddress);
            }
            else
            {
                // In real mode, we at least check that it returns something reasonable
                Assert.IsTrue(ipAddress != null);
                // It should be a valid IP address or the replacer value
                Assert.IsTrue(
                    ipAddress == ChatConstants.GET_IP_REPLACER ||
                    System.Text.RegularExpressions.Regex.IsMatch(ipAddress, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$")
                );
            }
        }

        // Helper methods to set and get private fields for testing
        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            Type type = obj.GetType();
            FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(obj, value);
        }

        private static T GetPrivateField<T>(object obj, string fieldName)
        {
            Type type = obj.GetType();
            FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)field.GetValue(obj);
        }
    }
}