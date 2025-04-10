using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;

using Steam_Community.DirectMessages.Interfaces;
using Steam_Community.DirectMessages.Models;

namespace Steam_Community.DirectMessages.ViewModels
{
    /// <summary>
    /// View model for the chat room UI, handling the presentation logic.
    /// </summary>
    public class ChatRoomViewModel
    {
        private IChatService chatService;

        /// <summary>
        /// Collection of messages to display in the UI.
        /// </summary>
        public ObservableCollection<Message> Messages { get; }

        /// <summary>
        /// The username of the current user.
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// Event raised when the window is closed.
        /// </summary>
        public event EventHandler<bool> WindowClosed;

        /// <summary>
        /// Gets or sets whether the chat window is open.
        /// </summary>
        public bool IsWindowOpen { get; set; }

        /// <summary>
        /// Gets or sets whether the current user is an admin.
        /// </summary>
        public bool IsAdmin { get; private set; }

        /// <summary>
        /// Gets or sets whether the current user is the host.
        /// </summary>
        public bool IsHost { get; private set; }

        /// <summary>
        /// Gets or sets whether the current user is muted.
        /// </summary>
        public bool IsMuted { get; private set; }

        /// <summary>
        /// Event raised when an exception occurs.
        /// </summary>
        public event EventHandler<ExceptionEventArgs> ExceptionOccurred;

        /// <summary>
        /// Event raised when the user's status changes.
        /// </summary>
        public event EventHandler StatusChanged;

        /// <summary>
        /// Initializes a new instance of the ChatRoomViewModel class.
        /// </summary>
        /// <param name="username">The username of the current user.</param>
        /// <param name="serverInviteIpAddress">The IP address of the server host, or HOST_IP_FINDER if this user is the host.</param>
        /// <param name="uiDispatcherQueue">The dispatcher queue for the UI thread.</param>
        public ChatRoomViewModel(string username, string serverInviteIpAddress, DispatcherQueue uiDispatcherQueue)
        {
            this.Username = username;
            this.IsWindowOpen = true;
            this.Messages = new ObservableCollection<Message>();

            // Create chat service
            this.chatService = new Steam_Community.DirectMessages.Services.ChatService(username, serverInviteIpAddress, uiDispatcherQueue);

            // Register event handlers
            this.chatService.MessageReceived += HandleMessageReceived;
            this.chatService.UserStatusChanged += HandleUserStatusChanged;
            this.chatService.ExceptionOccurred += HandleException;
        }

        /// <summary>
        /// Connects to the chat server.
        /// </summary>
        public void ConnectToServer()
        {
            this.chatService.ConnectToServer();
        }

        /// <summary>
        /// Sends a message to all connected users.
        /// </summary>
        /// <param name="messageContent">The content of the message to send.</param>
        public void SendMessage(string messageContent)
        {
            this.chatService.SendMessage(messageContent);
        }

        /// <summary>
        /// Attempts to change the mute status of the specified user.
        /// </summary>
        /// <param name="targetUsername">The username of the target user.</param>
        public void AttemptChangeMuteStatus(string targetUsername)
        {
            this.chatService.AttemptChangeMuteStatus(targetUsername);
        }

        /// <summary>
        /// Attempts to change the admin status of the specified user.
        /// </summary>
        /// <param name="targetUsername">The username of the target user.</param>
        public void AttemptChangeAdminStatus(string targetUsername)
        {
            this.chatService.AttemptChangeAdminStatus(targetUsername);
        }

        /// <summary>
        /// Attempts to kick the specified user from the chat room.
        /// </summary>
        /// <param name="targetUsername">The username of the target user.</param>
        public void AttemptKickUser(string targetUsername)
        {
            this.chatService.AttemptKickUser(targetUsername);
        }

        /// <summary>
        /// Clears all messages from the display.
        /// </summary>
        public void ClearMessages()
        {
            this.Messages.Clear();
        }

        /// <summary>
        /// Disconnects from the server and notifies listeners that the window is closed.
        /// </summary>
        public void DisconnectAndCloseWindow()
        {
            this.IsWindowOpen = false;
            this.chatService.DisconnectFromServer();
            this.WindowClosed?.Invoke(this, true);
        }

        /// <summary>
        /// Handles a received message.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="messageEventArgs">The event arguments containing the message.</param>
        private void HandleMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            this.Messages.Add(messageEventArgs.Message);

            // Limit the number of displayed messages
            while (this.Messages.Count > ChatConstants.MAX_MESSAGES_TO_DISPLAY)
            {
                this.Messages.RemoveAt(0);
            }
        }

        /// <summary>
        /// Handles a user status change.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="userStatusEventArgs">The event arguments containing the user status.</param>
        private void HandleUserStatusChanged(object sender, UserStatusEventArgs userStatusEventArgs)
        {
            UserStatus userStatus = userStatusEventArgs.UserStatus;

            // Update status properties
            this.IsHost = userStatus.IsHost;
            this.IsAdmin = userStatus.IsAdmin;
            this.IsMuted = userStatus.IsMuted;

            // Notify UI of status change
            this.StatusChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles an exception.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="exceptionEventArgs">The event arguments containing the exception.</param>
        private void HandleException(object sender, ExceptionEventArgs exceptionEventArgs)
        {
            // If window is closed, ignore the exception
            if (!this.IsWindowOpen)
            {
                return;
            }

            // Forward exception to UI
            this.ExceptionOccurred?.Invoke(this, exceptionEventArgs);
        }

        /// <summary>
        /// Checks if the user should be able to moderate another user.
        /// </summary>
        /// <param name="targetUsername">The username of the target user.</param>
        /// <returns>True if moderation should be allowed, false otherwise.</returns>
        public bool CanModerateUser(string targetUsername)
        {
            // Users cannot moderate themselves
            return targetUsername != this.Username;
        }
    }
}