using System;
using System.Net;

namespace DirectMessages
{
    public partial class Service : IService
    {
        public async partial void ConnectUserToServer()
        {
            try
            {
                switch (this.serverInviteIp == Service.HOST_IP_FINDER)
                {
                    case true:
                        this.serverInviteIp = this.userIpAddress;
                        // Errors can be: wrong ip address (doesn't match with the machine ip) or wrong format for the ip
                        this.server = new Server(this.userIpAddress, this.userName);
                        this.server.Start();
                        // Errors can be: wrong format for the ip address
                        this.client = new Client(serverInviteIp, userName, this.uiThread);
                        this.client.SetIsHost();
                        break;
                    case false:
                        // Errors can be: wrong format for the ip address
                        this.client = new Client(serverInviteIp, userName, this.uiThread);
                        break;
                }

                // First time the clients connects, he sends his username to the server
                // The function returns a "Task" (a promise that the function will finish in the future)
                // Await is used (the current function becomes async) so we can receive any errors that happened
                // in that "promise"
                await this.client.ConnectToServer();

                // The above function will throw errors if the provided ip address is not valid or
                // if something went wrong while sending his username across the network

                // Assign listeners for the Client class events
                this.client.NewMessageReceivedEvent += UpdateNewMessage;
                this.client.ClientStatusChangedEvent += InvokeClientStatusChange;
            }
            catch (Exception exception)
            {
                // Alert the UI about the new exception
                this.uiThread.TryEnqueue(() => this.ExceptionEvent?.Invoke(this, new ExceptionEventArgs(exception)));
            }
        }

        public partial void SendMessage(String message)
        {
            try
            {
                // A message sent over the network of length 0 is interpreted as a disconnect
                if (message.Length == 0)
                {
                    throw new Exception("Message content can't be empty");
                }

                // ? -> could be null, ?? -> what to return in case of null
                // Will be null if any errors appeared on server creation (for host) or on
                // client connection (for everyone)
                if (!this.client?.IsConnected() ?? true)
                {
                    throw new Exception("Client is not connected to server");
                }

                this.client?.SendMessageToServer(message);

                // The server exists only for the host, for others it's null
                // Null == false won't throw the exception
                // The server has a timer that's start once it detects that there are less than 2
                // users connected, once the timer is elapsed, the connection will close
                if (this.server?.IsServerRunning() == false)
                {
                    throw new Exception("Server timeout has been reached!");
                }
            }
            catch (Exception exception)
            {
                // Alert the UI about the new exception
                this.uiThread.TryEnqueue(() => this.ExceptionEvent?.Invoke(this, new ExceptionEventArgs(exception)));
            }
        }

        private partial void UpdateNewMessage(object? sender, MessageEventArgs messageEventArgs)
        {
            Message newMessage = messageEventArgs.Message;
            // Messages are sent with a left alligment by the server
            // The service aligns the messages for each client so that messages sent by the user
            // are on the right, received messages are on the left
            newMessage.MessageAligment = newMessage.MessageSenderName == this.userName ? "Right" : "Left";
            // Proceeds to alert the ui about the new aligned message
            this.uiThread.TryEnqueue(() => this.NewMessageEvent?.Invoke(this, new MessageEventArgs(newMessage)));
        }

        public partial void DisconnectClient()
        {
            // Further call to disconnect the client on window close
            this.client?.Disconnect();
        }

        public partial void TryChangeMuteStatus(String targetedUser)
        {
            // Commands can be found in more detail on the Server class
            // They follow a defined patter like "<something>|something|<something>"
            // Any user can send a message that is a command, even if they don't have access to
            // but the server will check for user status in the chat (this is where the try comes from)
            // Change means that it can become true or false
            String command = "<" + this.userName + ">|" + Server.MUTE_STATUS + "|<" + targetedUser + ">";
            this.SendMessage(command);
        }

        public partial void TryChangeAdminStatus(String targetedUser)
        {
            String command = "<" + this.userName + ">|" + Server.ADMIN_STATUS + "|<" + targetedUser + ">";
            this.SendMessage(command);
        }

        public partial void TryKick(String targetedUser)
        {
            String command = "<" + this.userName + ">|" + Server.KICK_STATUS + "|<" + targetedUser + ">";
            this.SendMessage(command);
        }

        private partial void InvokeClientStatusChange(object? sender, ClientStatusEventArgs clientStatusEventArgs)
        {
            // Alerts the ui about a user status change
            this.uiThread.TryEnqueue(() => this.ClientStatusChangedEvent?.Invoke(this, new ClientStatusEventArgs(clientStatusEventArgs.ClientStatus)));
        }

        public static partial String GetIpAddressOfCurrentUser()
        {
            try
            {
                String hostName = Dns.GetHostName();

                System.Net.IPAddress[] ipAddresses = System.Net.Dns.GetHostEntry(hostName).AddressList;
                String ipAddress = null;

                foreach (System.Net.IPAddress ip in ipAddresses)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) 
                    {
                        ipAddress = ip.ToString();
                        break;
                    }
                }

                if (ipAddress == null)
                {
                    throw new Exception("no ip");
                }

                return ipAddress;
            }
            catch (Exception)
            {
                return Service.GET_IP_REPLACER;
            }
        }
    }
}
