using Microsoft.UI.Dispatching;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DirectMessages
{
    internal class Client
    {
        private IPEndPoint serverEndPoint;
        private Socket clientSocket;
        private DispatcherQueue uiThread;

        public event EventHandler<MessageEventArgs> NewMessageReceivedEvent;

        private String userName;
        private bool isConnected;
        private bool isAdmin;
        private bool isMuted;
        private bool isHost;

        const int PORT_NUMBER = 6000;
        const int MESSAGE_MAXIMUM_SIZE = 4112;

        public Client(String hostIpAddress, String userName, DispatcherQueue uiThread)
        {
            this.userName = userName;
            this.uiThread = uiThread;

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
                byte[] messageToBytes = Encoding.UTF8.GetBytes(message);

                _ = await clientSocket.SendAsync(messageToBytes, SocketFlags.None);
            }
            catch (Exception)
            {
                throw new Exception("Couldn't send message to server, quiting...");
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
    }
}
