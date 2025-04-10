using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steam_Community.DirectMessages.Interfaces;
using Steam_Community.DirectMessages.Models;

namespace Steam_Community.DirectMessages.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of IChatService for testing.
    /// </summary>
    public class MockChatService : IChatService
    {
        public event EventHandler<MessageEventArgs>? MessageReceived;
        public event EventHandler<UserStatusEventArgs>? UserStatusChanged;
        public event EventHandler<ExceptionEventArgs>? ExceptionOccurred;

        public bool ConnectToServerCalled { get; private set; }
        public List<string> SentMessages { get; private set; } = new List<string>();
        public bool DisconnectCalled { get; private set; }
        public List<string> MuteStatusChangeAttemptedUsers { get; private set; } = new List<string>();
        public List<string> AdminStatusChangeAttemptedUsers { get; private set; } = new List<string>();
        public List<string> KickAttemptedUsers { get; private set; } = new List<string>();

        public void ConnectToServer()
        {
            ConnectToServerCalled = true;
        }

        public void SendMessage(string messageContent)
        {
            SentMessages.Add(messageContent);
        }

        public void DisconnectFromServer()
        {
            DisconnectCalled = true;
        }

        public void AttemptChangeMuteStatus(string targetUsername)
        {
            MuteStatusChangeAttemptedUsers.Add(targetUsername);
        }

        public void AttemptChangeAdminStatus(string targetUsername)
        {
            AdminStatusChangeAttemptedUsers.Add(targetUsername);
        }

        public void AttemptKickUser(string targetUsername)
        {
            KickAttemptedUsers.Add(targetUsername);
        }

        // Helper methods to trigger events for testing
        public void TriggerMessageReceived(Message message)
        {
            MessageReceived?.Invoke(this, new MessageEventArgs(message));
        }

        public void TriggerUserStatusChanged(UserStatus userStatus)
        {
            UserStatusChanged?.Invoke(this, new UserStatusEventArgs(userStatus));
        }

        public void TriggerExceptionOccurred(Exception exception)
        {
            ExceptionOccurred?.Invoke(this, new ExceptionEventArgs(exception));
        }
    }

    /// <summary>
    /// Mock implementation of INetworkClient for testing.
    /// </summary>
    public class MockNetworkClient : INetworkClient
    {
        public event EventHandler<MessageEventArgs>? MessageReceived;
        public event EventHandler<UserStatusEventArgs>? UserStatusChanged;

        public bool IsConnectedValue { get; set; }
        public bool ConnectCalled { get; private set; }
        public List<string> SentMessages { get; private set; } = new List<string>();
        public bool DisconnectCalled { get; private set; }
        public bool SetAsHostCalled { get; private set; }

        public Task ConnectToServer()
        {
            ConnectCalled = true;
            return Task.CompletedTask;
        }

        public bool IsConnected()
        {
            return IsConnectedValue;
        }

        public void SetAsHost()
        {
            SetAsHostCalled = true;
        }

        public Task SendMessageToServer(string messageContent)
        {
            SentMessages.Add(messageContent);
            return Task.CompletedTask;
        }

        public void Disconnect()
        {
            DisconnectCalled = true;
        }

        // Helper methods to trigger events for testing
        public void TriggerMessageReceived(Message message)
        {
            MessageReceived?.Invoke(this, new MessageEventArgs(message));
        }

        public void TriggerUserStatusChanged(UserStatus userStatus)
        {
            UserStatusChanged?.Invoke(this, new UserStatusEventArgs(userStatus));
        }
    }

    /// <summary>
    /// Mock implementation of INetworkServer for testing.
    /// </summary>
    public class MockNetworkServer : INetworkServer
    {
        public bool StartCalled { get; private set; }
        public bool IsRunningValue { get; set; }
        public List<Message> CreatedMessages { get; private set; } = new List<Message>();

        public void Start()
        {
            StartCalled = true;
        }

        public bool IsRunning()
        {
            return IsRunningValue;
        }

        public Message CreateMessage(string messageContent, string senderUsername)
        {
            var message = new Message
            {
                MessageContent = messageContent,
                MessageSenderName = senderUsername,
                MessageDateTime = DateTime.Now.ToString(),
                MessageAligment = ChatConstants.ALIGNMENT_LEFT,
                MessageSenderStatus = ChatConstants.REGULAR_USER_STATUS
            };

            CreatedMessages.Add(message);
            return message;
        }
    }
}