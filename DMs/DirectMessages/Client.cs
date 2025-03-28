using Microsoft.UI.Dispatching;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DirectMessages
{
    internal class Client
    {
        private IPEndPoint serverEndPoint;
        private Socket clientSocket;
        private DispatcherQueue uiThread;
        private Regex infoChangeCommandRegex;
        private ClientStatus clientStatus;

        public event EventHandler<MessageEventArgs> NewMessageReceivedEvent;
        public event EventHandler<ClientStatusEventArgs> ClientStatusChangedEvent;

        private String userName;
        private String infoChangeCommandPattern;

        public Client(String hostIpAddress, String userName, DispatcherQueue uiThread)
        {
            this.userName = userName;
            this.infoChangeCommandPattern = @"^<INFO>\|.*\|<INFO>$";
            this.uiThread = uiThread;

            this.clientStatus = new ClientStatus(false, false, false, false);

            this.infoChangeCommandRegex = new Regex(this.infoChangeCommandPattern);

            try
            {
                this.serverEndPoint = new IPEndPoint(IPAddress.Parse(hostIpAddress), Server.PORT_NUMBER);
                this.clientSocket = new(serverEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            }
            catch(Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }

        public async Task ConnectToServer()
        {
            try
            {
                await clientSocket.ConnectAsync(serverEndPoint);

                byte[] userNameToBytes = Encoding.UTF8.GetBytes(userName);
                _ = await clientSocket.SendAsync(userNameToBytes, SocketFlags.None);

                _ = Task.Run(() => ReceiveMessage());

                this.clientStatus.IsConnected = true;

                this.uiThread.TryEnqueue(() => this.ClientStatusChangedEvent?.Invoke(this, new ClientStatusEventArgs(clientStatus)));
            }
            catch(Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }

        public async Task SendMessageToServer(String message)
        {
            try
            {
                if (this.clientStatus.IsMuted)
                {
                    throw new Exception("You are muted, you can't send messages");
                }

                byte[] messageToBytes = Encoding.UTF8.GetBytes(message);

                _ = await clientSocket.SendAsync(messageToBytes, SocketFlags.None);
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }

        public async Task Disconnect()
        {
            try
            {
                _ = await clientSocket.SendAsync(new byte[0], SocketFlags.None);
                this.CloseConnection();
            }
            catch (Exception)
            {
                // Exception could be: already disconnected, server disconnected beforehand...
            }
        }

        private async Task ReceiveMessage()
        {
            try
            {
                while (true)
                {
                    byte[] messageBuffer = new byte[Server.MESSAGE_MAXIMUM_SIZE];
                    int messageLength = await clientSocket.ReceiveAsync(messageBuffer, SocketFlags.None);

                    if (messageLength == 0)
                    {
                        break;
                    }

                    Message message = Message.Parser.ParseFrom(messageBuffer, 0, messageLength);

                    if (this.infoChangeCommandRegex.IsMatch(message.MessageContent))
                    {
                        int newInfoIndex = 1;
                        char commandSeparator = '|';
                        String newInfo = message.MessageContent.Split(commandSeparator)[newInfoIndex];

                        this.UpdateClientStatus(newInfo);
                        continue;
                    }

                    this.uiThread.TryEnqueue(() => NewMessageReceivedEvent?.Invoke(this, new MessageEventArgs(message)));
                }
            }
            catch (Exception)
            {
                // Can't send exception up since we are in a task
            }
            finally
            {
                this.CloseConnection();
            }
        }
        public bool IsConnected()
        {
            return this.clientStatus.IsConnected;
        }

        private void CloseConnection()
        {
            this.clientStatus.IsConnected = false;
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }

        private void UpdateClientStatus(String newStatus)
        {
            switch (newStatus)
            {
                case Server.ADMIN_STATUS:
                    this.clientStatus.IsAdmin = !this.clientStatus.IsAdmin;
                    break;
                case Server.MUTE_STATUS:
                    this.clientStatus.IsMuted = !this.clientStatus.IsMuted;
                    break;
                case Server.KICK_STATUS:
                    this.clientStatus.IsConnected = false;
                    break;
                default:
                    break;
            }

            this.uiThread.TryEnqueue(() => this.ClientStatusChangedEvent?.Invoke(this, new ClientStatusEventArgs(clientStatus)));
        }

        public void SetIsHost()
        {
            this.clientStatus.IsHost = true;
        }
    }
}
