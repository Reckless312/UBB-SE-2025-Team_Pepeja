using System;
using Microsoft.UI.Dispatching;
using Steam_Community.DirectMessages.Models;

namespace Steam_Community.DirectMessages.Documentation
{
    /// <summary>
    /// Documentation for the ChatService class which provides chat services for 
    /// connecting users, sending messages, and managing user statuses.
    /// This class is separate from the implementation and serves as documentation only.
    /// </summary>
    public class ChatServiceDocs
    {
        /// <summary>
        /// Creates the handler for any operations or for errors encountered
        /// </summary>
        /// <param name="username">The name of the user who joined the chat room</param>
        /// <param name="serverInviteIpAddress">The ip address of the user who sent the invite,
        ///                              will be HOST_IP_FINDER if the user is the host</param>
        /// <param name="uiDispatcherQueue">Updating the ui can be done using only the main thread</param>
        public ChatServiceDocs(String username, String serverInviteIpAddress, DispatcherQueue uiDispatcherQueue)
        {
            // This is just documentation, no implementation
        }

        /// <summary>
        /// If the user is the host, a server is created on his end
        /// After that (host or not), a client is connected to the specified server (the host
        /// will connect to his own server as a client)
        /// </summary>
        /// <exception cref="Exception">Ip address of the server provided in the wrong format
        ///                             Ip address provided is not the same with the one for the machine (for the host)
        ///                             When connecting, if no server is listening to requests</exception>
        public void ConnectToServer()
        {
            // This is just documentation, no implementation
        }

        /// <summary>
        /// Sends the given message to all connected users
        /// </summary>
        /// <param name="messageContent">The input provided by the user in the textbox</param>
        /// <exception cref="Exception">Length of message is 0, could mean a disconnect from the server point of view
        ///                             If the client is not connected or is null (something went wrong on creation)
        ///                             Server timeout has been executed (3 min with less than 2 connections)</exception>
        public void SendMessage(String messageContent)
        {
            // This is just documentation, no implementation
        }

        /// <summary>
        /// The service receives a message from the client and will align messages according 
        /// to the sender (if the user is the sender, message will be on the right, otherwise, on the left)
        /// Will alert the UI about the upcoming message
        /// </summary>
        /// <param name="sender">The client initiated this event</param>
        /// <param name="messageEventArgs">Contains the new message received</param>
        private void HandleMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            // This is just documentation, no implementation
        }

        /// <summary>
        /// Disconnects the client from the server
        /// </summary>
        public void DisconnectFromServer()
        {
            // This is just documentation, no implementation
        }

        /// <summary>
        /// Attempts to change the mute status of the specified user.
        /// Can be given or removed by admins and host
        /// </summary>
        /// <param name="targetUsername">The username of the target user</param>
        public void AttemptChangeMuteStatus(String targetUsername)
        {
            // This is just documentation, no implementation
        }

        /// <summary>
        /// Attempts to change the admin status of the specified user.
        /// Can only be given by host
        /// </summary>
        /// <param name="targetUsername">The username of the target user</param>
        public void AttemptChangeAdminStatus(String targetUsername)
        {
            // This is just documentation, no implementation
        }

        /// <summary>
        /// Attempts to kick the specified user from the chat room.
        /// Can be initiated by admins and host
        /// </summary>
        /// <param name="targetUsername">The username of the target user</param>
        public void AttemptKickUser(String targetUsername)
        {
            // This is just documentation, no implementation
        }

        /// <summary>
        /// The service receives the new status from the client and alerts the ui about it
        /// </summary>
        /// <param name="sender">The client initiated this event</param>
        /// <param name="userStatusEventArgs">Contains the user status object</param>
        private void HandleUserStatusChanged(object sender, UserStatusEventArgs userStatusEventArgs)
        {
            // This is just documentation, no implementation
        }

        /// <summary>
        /// Dynamically search for the user ip address
        /// Could have bugs related to the position of the ip in the array resulted by DNS
        /// </summary>
        /// <returns>The local ip of the current user</returns>
        public static String GetLocalIpAddress()
        {
            // This is just documentation, no implementation
            return null;
        }
    }
}