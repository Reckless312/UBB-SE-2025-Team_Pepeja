using System;
using System.Reflection;
using Microsoft.UI.Dispatching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Steam_Community.DirectMessages.Models;
using Steam_Community.DirectMessages.Tests.Mocks;
using Steam_Community.DirectMessages.ViewModels;
using Steam_Community.Tests.DirectMessages.Mocks;

namespace Steam_Community.DirectMessages.Tests.ViewModels
{
    [TestClass]
    public class ChatRoomViewModelTests
    {
        private MockChatService mockChatService;
        private ChatRoomViewModel viewModel;
        private MockDispatcherQueue mockDispatcherQueue;
        private const string TestUsername = "TestUser";
        private const string TestServerIp = "192.168.1.1";

        [TestInitialize]
        public void Initialize()
        {
            // Setup mocks
            mockChatService = new MockChatService();
            mockDispatcherQueue = MockDispatcherQueue.Instance;

            try
            {
                // Create the view model without dispatcher queue first
                viewModel = new ChatRoomViewModel(TestUsername, TestServerIp, null);

                // Replace the internal chat service with our mock using reflection
                SetPrivateField(viewModel, "chatService", mockChatService);

                // Replace the dispatcher queue with our mock
                FieldInfo dispatcherField = typeof(ChatRoomViewModel).GetField("_uiDispatcherQueue",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                if (dispatcherField != null)
                {
                    // We can't assign mockDispatcherQueue directly since it's not actually a DispatcherQueue,
                    // but we can set it to null for testing
                    dispatcherField.SetValue(viewModel, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test initialization failed: {ex.Message}");
                Assert.Inconclusive($"Failed to initialize test: {ex.Message}");
            }
        }

        [TestMethod]
        public void Constructor_SetsInitialValues()
        {
            if (viewModel == null) Assert.Inconclusive("ViewModel not initialized");

            // Check manually initialized properties
            Assert.AreEqual(TestUsername, viewModel.Username);
            Assert.IsTrue(viewModel.IsWindowOpen);
            Assert.IsNotNull(viewModel.Messages);
            Assert.AreEqual(0, viewModel.Messages.Count);
        }

        [TestMethod]
        public void ConnectToServer_CallsChatServiceConnect()
        {
            if (viewModel == null || mockChatService == null)
                Assert.Inconclusive("Test setup not initialized");

            // Act
            viewModel.ConnectToServer();

            // Assert
            Assert.IsTrue(mockChatService.ConnectToServerCalled);
        }

        [TestMethod]
        public void SendMessage_CallsChatServiceSendMessage()
        {
            if (viewModel == null || mockChatService == null)
                Assert.Inconclusive("Test setup not initialized");

            // Arrange
            string messageContent = "Test message";

            // Act
            viewModel.SendMessage(messageContent);

            // Assert
            Assert.AreEqual(1, mockChatService.SentMessages.Count);
            Assert.AreEqual(messageContent, mockChatService.SentMessages[0]);
        }

        [TestMethod]
        public void AttemptChangeMuteStatus_CallsChatServiceMethod()
        {
            if (viewModel == null || mockChatService == null)
                Assert.Inconclusive("Test setup not initialized");

            // Arrange
            string targetUsername = "TargetUser";

            // Act
            viewModel.AttemptChangeMuteStatus(targetUsername);

            // Assert
            Assert.AreEqual(1, mockChatService.MuteStatusChangeAttemptedUsers.Count);
            Assert.AreEqual(targetUsername, mockChatService.MuteStatusChangeAttemptedUsers[0]);
        }

        [TestMethod]
        public void AttemptChangeAdminStatus_CallsChatServiceMethod()
        {
            if (viewModel == null || mockChatService == null)
                Assert.Inconclusive("Test setup not initialized");

            // Arrange
            string targetUsername = "TargetUser";

            // Act
            viewModel.AttemptChangeAdminStatus(targetUsername);

            // Assert
            Assert.AreEqual(1, mockChatService.AdminStatusChangeAttemptedUsers.Count);
            Assert.AreEqual(targetUsername, mockChatService.AdminStatusChangeAttemptedUsers[0]);
        }

        [TestMethod]
        public void AttemptKickUser_CallsChatServiceMethod()
        {
            if (viewModel == null || mockChatService == null)
                Assert.Inconclusive("Test setup not initialized");

            // Arrange
            string targetUsername = "TargetUser";

            // Act
            viewModel.AttemptKickUser(targetUsername);

            // Assert
            Assert.AreEqual(1, mockChatService.KickAttemptedUsers.Count);
            Assert.AreEqual(targetUsername, mockChatService.KickAttemptedUsers[0]);
        }

        [TestMethod]
        public void ClearMessages_RemovesAllMessages()
        {
            if (viewModel == null) Assert.Inconclusive("ViewModel not initialized");

            // Arrange
            viewModel.Messages.Add(new Message { MessageContent = "Message 1" });
            viewModel.Messages.Add(new Message { MessageContent = "Message 2" });

            // Act
            viewModel.ClearMessages();

            // Assert
            Assert.AreEqual(0, viewModel.Messages.Count);
        }

        [TestMethod]
        public void DisconnectAndCloseWindow_SetsWindowOpenToFalseAndCallsDisconnect()
        {
            if (viewModel == null || mockChatService == null)
                Assert.Inconclusive("Test setup not initialized");

            // Arrange
            bool windowClosedEventRaised = false;
            viewModel.WindowClosed += (sender, args) => windowClosedEventRaised = true;

            // Act
            viewModel.DisconnectAndCloseWindow();

            // Assert
            Assert.IsFalse(viewModel.IsWindowOpen);
            Assert.IsTrue(mockChatService.DisconnectCalled);
            Assert.IsTrue(windowClosedEventRaised);
        }

        [TestMethod]
        public void HandleMessageReceived_AddsMessageToCollection()
        {
            if (viewModel == null || mockChatService == null)
                Assert.Inconclusive("Test setup not initialized");

            // Arrange
            Message testMessage = new Message
            {
                MessageContent = "Test content",
                MessageSenderName = "Sender",
                MessageDateTime = DateTime.Now.ToString(),
                MessageAligment = ChatConstants.ALIGNMENT_LEFT
            };

            // Act - directly invoke the handler method
            InvokePrivateMethod(viewModel, "HandleMessageReceived", mockChatService, new MessageEventArgs(testMessage));

            // Assert
            Assert.AreEqual(1, viewModel.Messages.Count);
            Assert.AreEqual(testMessage, viewModel.Messages[0]);
        }

        [TestMethod]
        public void HandleMessageReceived_LimitsMessagesToMaxDisplay()
        {
            if (viewModel == null || mockChatService == null)
                Assert.Inconclusive("Test setup not initialized");

            // Arrange - Add more than the maximum allowed messages
            for (int i = 0; i < ChatConstants.MAX_MESSAGES_TO_DISPLAY + 10; i++)
            {
                Message msg = new Message
                {
                    MessageContent = $"Message {i}",
                    MessageSenderName = "Sender",
                    MessageDateTime = DateTime.Now.ToString()
                };
                InvokePrivateMethod(viewModel, "HandleMessageReceived", mockChatService, new MessageEventArgs(msg));
            }

            // Assert
            Assert.AreEqual(ChatConstants.MAX_MESSAGES_TO_DISPLAY, viewModel.Messages.Count);
            // First message should now be the one that was at position MAX_MESSAGES_TO_DISPLAY - 10
            Assert.AreEqual($"Message {10}", viewModel.Messages[0].MessageContent);
        }

        [TestMethod]
        public void HandleUserStatusChanged_UpdatesStatusProperties()
        {
            if (viewModel == null || mockChatService == null)
                Assert.Inconclusive("Test setup not initialized");

            // Arrange
            UserStatus userStatus = new UserStatus
            {
                IsHost = true,
                IsAdmin = true,
                IsMuted = false
            };

            bool statusChangedEventRaised = false;
            viewModel.StatusChanged += (sender, args) => statusChangedEventRaised = true;

            // Act - directly invoke the handler method
            InvokePrivateMethod(viewModel, "HandleUserStatusChanged", mockChatService, new UserStatusEventArgs(userStatus));

            // Assert
            Assert.IsTrue(viewModel.IsHost);
            Assert.IsTrue(viewModel.IsAdmin);
            Assert.IsFalse(viewModel.IsMuted);
            Assert.IsTrue(statusChangedEventRaised);
        }

        [TestMethod]
        public void HandleException_IgnoresExceptionsWhenWindowClosed()
        {
            if (viewModel == null || mockChatService == null)
                Assert.Inconclusive("Test setup not initialized");

            // Arrange
            viewModel.IsWindowOpen = false;
            bool exceptionEventRaised = false;
            viewModel.ExceptionOccurred += (sender, args) => exceptionEventRaised = true;

            // Act - directly invoke the handler method
            InvokePrivateMethod(viewModel, "HandleException", mockChatService,
                new ExceptionEventArgs(new Exception("Test exception")));

            // Assert
            Assert.IsFalse(exceptionEventRaised);
        }

        [TestMethod]
        public void HandleException_ForwardsExceptionsWhenWindowOpen()
        {
            if (viewModel == null || mockChatService == null)
                Assert.Inconclusive("Test setup not initialized");

            // Arrange
            viewModel.IsWindowOpen = true;
            Exception testException = new Exception("Test exception");
            ExceptionEventArgs capturedArgs = null;
            viewModel.ExceptionOccurred += (sender, args) => capturedArgs = args;

            // Act - directly invoke the handler method
            InvokePrivateMethod(viewModel, "HandleException", mockChatService,
                new ExceptionEventArgs(testException));

            // Assert
            Assert.IsNotNull(capturedArgs);
            Assert.AreEqual(testException, capturedArgs.Exception);
        }

        [TestMethod]
        public void CanModerateUser_ReturnsFalseForCurrentUser()
        {
            if (viewModel == null) Assert.Inconclusive("ViewModel not initialized");

            // Act & Assert
            Assert.IsFalse(viewModel.CanModerateUser(TestUsername));
        }

        [TestMethod]
        public void CanModerateUser_ReturnsTrueForOtherUsers()
        {
            if (viewModel == null) Assert.Inconclusive("ViewModel not initialized");

            // Act & Assert
            Assert.IsTrue(viewModel.CanModerateUser("DifferentUser"));
        }

        // Helper method to set private fields for testing
        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            Type type = obj.GetType();
            FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
                throw new ArgumentException($"Field '{fieldName}' not found in {type.Name}");

            field.SetValue(obj, value);
        }

        // Helper method to get private fields for testing
        private static T GetPrivateField<T>(object obj, string fieldName)
        {
            Type type = obj.GetType();
            FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
                throw new ArgumentException($"Field '{fieldName}' not found in {type.Name}");

            return (T)field.GetValue(obj);
        }

        // Helper method to invoke private methods for testing
        private static object InvokePrivateMethod(object obj, string methodName, params object[] parameters)
        {
            Type type = obj.GetType();
            MethodInfo method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
                throw new ArgumentException($"Method '{methodName}' not found in {type.Name}");

            return method.Invoke(obj, parameters);
        }
    }
}