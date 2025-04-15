using System;
using Steam_Community.DirectMessages.Models;

namespace Steam_Community.DirectMessages.Interfaces
{
    /// <summary>
    /// Defines the contract for the chat service, which handles communication between the UI and the network.
    /// </summary>
    public interface IChatService
    {
        /// <summary>
        /// Event that is raised when a new message is received.
        /// </summary>
        event EventHandler<MessageEventArgs> MessageReceived;

        /// <summary>
        /// Event that is raised when a user's status changes.
        /// </summary>
        event EventHandler<UserStatusEventArgs> UserStatusChanged;

        /// <summary>
        /// Event that is raised when an exception occurs in the service.
        /// </summary>
        event EventHandler<ExceptionEventArgs> ExceptionOccurred;

        /// <summary>
        /// Connects the current user to the chat server.
        /// </summary>
        void ConnectToServer();

        /// <summary>
        /// Sends a message to all connected users.
        /// </summary>
        /// <param name="messageContent">The content of the message to send.</param>
        void SendMessage(string messageContent);

        /// <summary>
        /// Disconnects the user from the chat server.
        /// </summary>
        void DisconnectFromServer();

        /// <summary>
        /// Attempts to change the mute status of the specified user.
        /// </summary>
        /// <param name="targetUsername">The username of the target user.</param>
        void AttemptChangeMuteStatus(string targetUsername);

        /// <summary>
        /// Attempts to change the admin status of the specified user.
        /// </summary>
        /// <param name="targetUsername">The username of the target user.</param>
        void AttemptChangeAdminStatus(string targetUsername);

        /// <summary>
        /// Attempts to kick the specified user from the chat room.
        /// </summary>
        /// <param name="targetUsername">The username of the target user.</param>
        void AttemptKickUser(string targetUsername);
    }
}