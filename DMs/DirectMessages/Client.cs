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

        public event EventHandler<MessageEventArgs> NewMessageReceivedEvent;

        private String userName;
        private String infoChangeCommandPattern;
        private bool isConnected;
        private bool isAdmin;
        private bool isMuted;
        private bool isHost;

        const int PORT_NUMBER = 6000;
        const int MESSAGE_MAXIMUM_SIZE = 4112;
        const String ADMIN_STATUS = "ADMIN";
        const String MUTE_STATUS = "MUTE";
        const String KICK_STATUS = "KICK";

        public Client(String hostIpAddress, String userName, DispatcherQueue uiThread)
        {
            this.userName = userName;
            this.infoChangeCommandPattern = @"^<INFO>\|.*\|<INFO>$";
            this.uiThread = uiThread;

            this.infoChangeCommandRegex = new Regex(this.infoChangeCommandPattern);

            try
            {
                this.serverEndPoint = new IPEndPoint(IPAddress.Parse(hostIpAddress), PORT_NUMBER);
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

                this.isConnected = true;
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
                if (this.isMuted)
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
                    byte[] messageBuffer = new byte[MESSAGE_MAXIMUM_SIZE];
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
            return this.isConnected;
        }

        private void CloseConnection()
        {
            this.isConnected = false;
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }

        private void UpdateClientStatus(String newStatus)
        {
            switch (newStatus)
            {
                case ADMIN_STATUS:
                    this.isAdmin = !this.isAdmin;
                    break;
                case MUTE_STATUS:
                    this.isMuted = !this.isMuted;
                    break;
                case KICK_STATUS:
                    this.isConnected = false;
                    break;
                default:
                    break;
            }
        }

        public void SetIsHost()
        {
            this.isHost = true;
        }

        public bool IsHost()
        {
            return this.isHost;
        }

        public bool IsAdmin()
        {
            return this.isAdmin;
        }

        public bool IsRegularUser()
        {
            return !(this.isHost || this.isAdmin);
        }
    }
}
