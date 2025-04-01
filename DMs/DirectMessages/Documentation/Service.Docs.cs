using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;

namespace DirectMessages
{
    public partial class Service : IService
    {
        // Server is only made by the host, for other users, it will be null

        private Client? client;
        private DispatcherQueue uiThread;
        private Server? server;

        public event EventHandler<MessageEventArgs> NewMessageEvent;
        public event EventHandler<ClientStatusEventArgs> ClientStatusChangedEvent;
        public event EventHandler<ExceptionEventArgs> ExceptionEvent;

        private String userName;
        private String userIpAddress;
        private String serverInviteIp;

        public const String HOST_IP_FINDER = "None";
        public const String GET_IP_REPLACER = "NULL";

        /// <summary>
        /// Creates the handler for any operations or for errors encountered
        /// </summary>
        /// <param name="userName">The name of the user who joined the chat room</param>
        /// <param name="serverInviteIp">The ip address of the user who sent the invite,
        ///                              will be HOST_IP_FINDER if the user is the host</param>
        /// <param name="uiThread">Updating the ui can be done using only the main thread</param>
        public Service(String userName, String serverInviteIp, DispatcherQueue uiThread)
        {
            this.userName = userName;
            this.userIpAddress = Service.GetIpAddressOfCurrentUser();
            this.serverInviteIp = serverInviteIp;
            this.uiThread = uiThread;
        }

        /// <summary>
        /// If the user is the host, a server is created on his end
        /// After that (host or not), a client is connected to the specified server (the host
        /// will connect to his own server as a client)
        /// </summary>
        /// <exception cref="Exception">Ip address of the server provided in the wrong format
        ///                             Ip address provided is not the same with the one for the machine (for the host)
        ///                             When connecting, if no server is listening to requests</exception>
        public partial void ConnectUserToServer();

        /// <summary>
        /// Sends the given message to all connected users
        /// </summary>
        /// <param name="message">The input provided by the user in the textbox</param>
        /// <exception cref="Exception">Length of message is 0, could mean a disconnect from the server point of view
        ///                             If the client is not connected or is null (something went wrong on creation)
        ///                             Server timeout has been executed (3 min with less than 2 connections)</exception>
        public partial void SendMessage(String message);

        /// <summary>
        /// The service receives a message from the client and will align messages according 
        /// to the sender (if the user is the sender, message will be on the right, otherwise, on the left)
        /// Will alert the UI about the upcoming message
        /// </summary>
        /// <param name="sender">The client initiated this event</param>
        /// <param name="messageEventArgs">Contains the new message received</param>
        private partial void UpdateNewMessage(object? sender, MessageEventArgs messageEventArgs);

        /// <summary>
        /// Disconnects the client from the server on window close
        /// </summary>
        public partial void DisconnectClient();

        // Changing a status depends on the current user (the one who initiated this action) status,
        // alas, the change might or might not take effect, in which case, the server displays a message only
        // to the person who initiated the action

        /// <summary>
        /// The "Mute" status will influence the user capability of sending a message to the connected users
        /// Can be given or removed by admins and host
        /// </summary>
        /// <param name="targetedUser">The name of the user provided from the received messages (on select and button interaction)</param>
        public partial void TryChangeMuteStatus(String targetedUser);

        /// <summary>
        /// The "Admin" status will make the user be able to moderate the chat users (those who are just regular users)
        /// Can only be given by host
        /// </summary>
        /// <param name="targetedUser">The name of the user provided from the received messages (on select and button interaction)</param>
        public partial void TryChangeAdminStatus(String targetedUser);

        /// <summary>
        /// The "Kick" status will make the user disconnect from the chat room 
        /// Can be given by admins and host
        /// </summary>
        /// <param name="targetedUser">The name of the user provided from the received messages (on select and button interaction)</param>
        public partial void TryKick(String targetedUser);

        /// <summary>
        /// The service receives the new status from the client and alerts the ui about it
        /// </summary>
        /// <param name="sender">The client initiated this event</param>
        /// <param name="clientStatusEventArgs">Contains the client status object</param>
        private partial void InvokeClientStatusChange(object? sender, ClientStatusEventArgs clientStatusEventArgs);

        /// <summary>
        /// Dynamically search for the user ip address
        /// Could have bugs related to the position of the ip in the array resulted by DNS
        /// </summary>
        /// <returns>The local ip of the current user</returns>
        public static partial String GetIpAddressOfCurrentUser();
    }
}
