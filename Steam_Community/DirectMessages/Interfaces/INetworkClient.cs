using System;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Steam_Community.DirectMessages.Models;

namespace Steam_Community.DirectMessages.Interfaces
{
    /// <summary>
    /// Defines the contract for a network client that communicates with a chat server.
    /// </summary>
    public interface INetworkClient
    {
        /// <summary>
        /// Event that is raised when a new message is received from the server.
        /// </summary>
        event EventHandler<MessageEventArgs> MessageReceived;

        /// <summary>
        /// Event that is raised when the user's status changes.
        /// </summary>
        event EventHandler<UserStatusEventArgs> UserStatusChanged;

        /// <summary>
        /// Connects to the chat server.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ConnectToServer();

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="messageContent">The content of the message to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendMessageToServer(string messageContent);

        /// <summary>
        /// Checks if the client is connected to the server.
        /// </summary>
        /// <returns>True if connected, false otherwise.</returns>
        bool IsConnected();

        /// <summary>
        /// Sets the client as the host of the chat room.
        /// </summary>
        void SetAsHost();

        /// <summary>
        /// Disconnects from the server.
        /// </summary>
        void Disconnect();
    }
}