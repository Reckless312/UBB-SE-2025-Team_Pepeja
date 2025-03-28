using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DirectMessages
{
    internal class Service: IService
    {
        private Client client;
        private DispatcherQueue uiThread;
        private Server? server;

        public event EventHandler<MessageEventArgs> NewMessageEvent;
        public event EventHandler<ClientStatusEventArgs> ClientStatusChangedEvent;
        public List<String> sentFriendRequests = new List<String>();

        private String userName;
        private String userIpAddress;
        private String serverInviteIp;

        private const String HOST_IP_FINDER = "None";
        private const String ADMIN_STATUS = "ADMIN";
        private const String MUTE_STATUS = "MUTE";
        private const String KICK_STATUS = "KICK";

        public Service(String userName, String userIpAddress, String serverInviteIp, DispatcherQueue uiThread)
        {
            this.userName = userName;
            this.userIpAddress = userIpAddress;
            this.serverInviteIp = serverInviteIp;
            this.uiThread = uiThread;
        }

        public async Task ConnectUserToServer()
        {
            if (serverInviteIp == HOST_IP_FINDER)
            {
                this.serverInviteIp = this.userIpAddress;
                this.server = new Server(this.userIpAddress, this.userName);
                this.server.Start();
            }

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

            this.client.NewMessageReceivedEvent += UpdateNewMessage;
            this.client.ClientStatusChangedEvent += InvokeClientStatusChange;
        }

        private void UpdateNewMessage(object? sender, MessageEventArgs messageEventArgs)
        {
            Message newMessage = messageEventArgs.Message;
            newMessage.MessageAligment = newMessage.MessageSenderName == this.userName ? "Right" : "Left";
            this.uiThread.TryEnqueue(() => this.NewMessageEvent?.Invoke(this, new MessageEventArgs(newMessage)));
        }

        private void InvokeClientStatusChange(object? sender, ClientStatusEventArgs clientStatusEventArgs)
        {
            this.uiThread.TryEnqueue(() => this.ClientStatusChangedEvent?.Invoke(this, new ClientStatusEventArgs(clientStatusEventArgs.ClientStatus)));
        }

        public async Task SendMessage(String message)
        {
            if(message.Length == 0)
            {
                throw new Exception("Message content can't be empty");
            }

            if (!this.client.IsConnected())
            {
                throw new Exception("Client is not connected to server");
            }

            try
            {
                await this.client.SendMessageToServer(message);
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }

            if(this.server?.IsServerRunning() == false)
            {
                throw new Exception("Server timeout has been reached!");
            }
        }

        public async Task DisconnectClient()
        {
            await this.client.Disconnect();
        }

        public async Task TryChangeMuteStatus(String targetedUser)
        {
            String command = "<" + this.userName + ">|" + Server.MUTE_STATUS + "|<" + targetedUser + ">";
            await this.SendMessage(targetedUser);
        }

        public async Task TryChangeAdminStatus(String targetedUser)
        {
            String command = "<" + this.userName + ">|" + Server.ADMIN_STATUS + "|<" + targetedUser + ">";
            await this.SendMessage(targetedUser);
        }

        public async Task TryKick(String targetedUser)
        {
            String command = "<" + this.userName + ">|" + Server.KICK_STATUS + "|<" + targetedUser + ">";
            await this.SendMessage(targetedUser);
        }

        public void SendFriendRequest(String targetedUser)
        {
            this.sentFriendRequests.Add(targetedUser);
        }

        public void CancelFriendRequest(String targetedUser)
        {
            this.sentFriendRequests.Remove(targetedUser);
        }

        public bool IsInFriendRequests(String userName)
        {
            return this.sentFriendRequests.Contains(userName);
        }
    }
}
