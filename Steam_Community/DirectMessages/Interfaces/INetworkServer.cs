using System;
using System.Threading.Tasks;
using Steam_Community.DirectMessages.Models;

namespace Steam_Community.DirectMessages.Interfaces
{
    /// <summary>
    /// Defines the contract for a network server that manages chat connections.
    /// </summary>
    public interface INetworkServer
    {
        /// <summary>
        /// Starts the server and begins listening for client connections.
        /// </summary>
        void Start();

        /// <summary>
        /// Checks if the server is currently running.
        /// </summary>
        /// <returns>True if the server is running, false otherwise.</returns>
        bool IsRunning();

        /// <summary>
        /// Creates a message object from the provided content and sender name.
        /// </summary>
        /// <param name="messageContent">The content of the message.</param>
        /// <param name="senderUsername">The username of the message sender.</param>
        /// <returns>A new Message object.</returns>
        Message CreateMessage(string messageContent, string senderUsername);
    }
}