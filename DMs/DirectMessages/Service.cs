using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DirectMessages
{
    internal class Service: IService
    {
        // Server is only made by one user, for the others, it's null
        // Client is usually safely created, but can be null if the host doesn't succesfully create a server
        // Repository exchanged in favor of the server
        // Interface for server not implemented because it's redundant

        private Client? client;
        private DispatcherQueue uiThread;
        private Server? server;

        public event EventHandler<MessageEventArgs> NewMessageEvent;
        public event EventHandler<ClientStatusEventArgs> ClientStatusChangedEvent;

        private List<String> sentFriendRequests = new List<String>();

        private String userName;
        private String userIpAddress;
        private String serverInviteIp;

        public const String HOST_IP_FINDER = "None";

        /// <summary>
        /// Constructor for the Service class
        /// </summary>
        /// <param name="userName">Current client username</param>
        /// <param name="userIpAddress">Current client ip address</param>
        /// <param name="serverInviteIp">Ip address of the person who invited the client</param>
        /// <param name="uiThread">Thread for updating the main window</param>
        public Service(String userName, String userIpAddress, String serverInviteIp, DispatcherQueue uiThread)
        {
            this.userName = userName;
            this.userIpAddress = userIpAddress;
            this.serverInviteIp = serverInviteIp;
            this.uiThread = uiThread;
        }

        /// <summary>
        /// Creates a server if user is host
        /// Connects the user to the affiliated server
        /// </summary>
        /// <returns>A promise</returns>
        /// <exception cref="Exception">Server Creation / User Connection Errors</exception>
        public async Task ConnectUserToServer()
        {
            if (this.serverInviteIp == HOST_IP_FINDER)
            {
                this.serverInviteIp = this.userIpAddress;
                this.server = new Server(this.userIpAddress, this.userName);
                this.server.Start();
            }

            // Not reachable if the server threw an error
            this.client = new Client(serverInviteIp, userName, this.uiThread);

            if (serverInviteIp == userIpAddress)
            {
                this.client.SetIsHost();
            }

            await this.client.ConnectToServer();

            if (!this.client.IsConnected())
            {
                throw new Exception("Couldn't connect to server");
            }

            // "Subscribe" to the client events
            this.client.NewMessageReceivedEvent += UpdateNewMessage;
            this.client.ClientStatusChangedEvent += InvokeClientStatusChange;
        }

        /// <summary>
        /// Sends provided message to the server
        /// </summary>
        /// <param name="message">Message to be sent</param>
        /// <returns>A promise</returns>
        /// <exception cref="Exception"> Message / Connection / Server Errors </exception>
        public async Task SendMessage(String message)
        {
            // A message of length 0 couls also be taken as a disconnect over the network
            // But it's mostly taken as a no-point to do action
            if(message.Length == 0)
            {
                throw new Exception("Message content can't be empty");
            }
            
            // ? -> could be null, ?? -> what to return in case of null
            if (!this.client?.IsConnected() ?? true)
            {
                throw new Exception("Client is not connected to server");
            }

            try
            {
                // Can't await a null object
                if(this.client != null)
                {
                    await this.client.SendMessageToServer(message);
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }

            // Server has a timer for inactivity ( < 2 people connected)
            if(this.server?.IsServerRunning() == false)
            {
                throw new Exception("Server timeout has been reached!");
            }
        }

        /// <summary>
        /// Listener for NewMessageEvent
        /// Aligns the messages for current client
        /// Invokes the "subscribers" for the event from the service
        /// </summary>
        private void UpdateNewMessage(object? sender, MessageEventArgs messageEventArgs)
        {
            Message newMessage = messageEventArgs.Message;
            newMessage.MessageAligment = newMessage.MessageSenderName == this.userName ? "Right" : "Left";
            this.uiThread.TryEnqueue(() => this.NewMessageEvent?.Invoke(this, new MessageEventArgs(newMessage)));
        }

        /// <summary>
        /// Disconnects Client on shutdown
        /// </summary>
        /// <returns>A promise</returns>
        public async Task DisconnectClient()
        {
            if(this.client != null)
            {
                await this.client.Disconnect();
            }
        }

        /// <summary>
        /// Attempts to mute/unmute a targeted user
        /// </summary>
        /// <param name="targetedUser">User to be affected</param>
        /// <returns>A promise</returns>
        public async Task TryChangeMuteStatus(String targetedUser)
        {
            String command = "<" + this.userName + ">|" + Server.MUTE_STATUS + "|<" + targetedUser + ">";
            await this.SendMessage(command);
        }

        /// <summary>
        /// Attempts make an admin or remove the status for a user
        /// </summary>
        /// <param name="targetedUser">User to be affected</param>
        /// <returns>A promise</returns>
        public async Task TryChangeAdminStatus(String targetedUser)
        {
            String command = "<" + this.userName + ">|" + Server.ADMIN_STATUS + "|<" + targetedUser + ">";
            await this.SendMessage(command);
        }

        /// <summary>
        /// Attempts to kick a targeted user
        /// </summary>
        /// <param name="targetedUser">User to be affected</param>
        /// <returns>A promise</returns>
        public async Task TryKick(String targetedUser)
        {
            String command = "<" + this.userName + ">|" + Server.KICK_STATUS + "|<" + targetedUser + ">";
            await this.SendMessage(command);
        }

        /// <summary>
        /// Listener for ClientStatusChangeEvent
        /// Invokes the "subscribers" for the event from the service
        /// </summary>
        private void InvokeClientStatusChange(object? sender, ClientStatusEventArgs clientStatusEventArgs)
        {
            this.uiThread.TryEnqueue(() => this.ClientStatusChangedEvent?.Invoke(this, new ClientStatusEventArgs(clientStatusEventArgs.ClientStatus)));
        }

        /// <summary>
        /// Sends a friend request to the user
        /// </summary>
        /// <param name="targetedUser">User to be affected</param>
        public void SendFriendRequest(String targetedUser)
        {
            this.sentFriendRequests.Add(targetedUser);
            
            // TO DO: Make connection to FriendRequest Component
        }

        /// <summary>
        /// Cancels the friend request sent to a user
        /// </summary>
        /// <param name="targetedUser">User to be affected</param>
        public void CancelFriendRequest(String targetedUser)
        {
            this.sentFriendRequests.Remove(targetedUser);

            // TO DO: Make connection to FriendRequest Component
        }

        /// <summary>
        /// Checks if the user is in the friend request list
        /// </summary>
        /// <param name="targetedUser">User to be affected</param>
        /// <returns>True or False</returns>
        public bool IsInFriendRequests(String userName)
        {
            return this.sentFriendRequests.Contains(userName);
        }
    }
}
