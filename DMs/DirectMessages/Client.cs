using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;

namespace DirectMessages
{
    internal class Client
    {
        private IPEndPoint serverEndPoint;
        private Socket clientSocket;
        private DispatcherQueue dispatch;

        public event EventHandler<MessageEventArgs> NewMessage;

        private String userName;

        const int PORT_NUMBER = 6000;

        public Client(String hostIpAddress, String userName, DispatcherQueue dispatch)
        {
            this.userName = userName;
            this.dispatch = dispatch;
            try
            {
                this.serverEndPoint = new IPEndPoint(IPAddress.Parse(hostIpAddress), PORT_NUMBER);
                this.clientSocket = new(serverEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                _ = this.ConnectToServer();
            }
            catch(Exception exception)
            {
                Console.WriteLine($"Client create error: {exception.Message}");
            }
        }

        private async Task ConnectToServer()
        {
            try
            {
                await clientSocket.ConnectAsync(serverEndPoint);

                byte[] messageBytes = Encoding.ASCII.GetBytes(userName);

                _ = await clientSocket.SendAsync(messageBytes, SocketFlags.None);

                _ = Task.Run(() => ReceiveMessage());
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
                byte[] messageBytes = Encoding.ASCII.GetBytes(message);

                _ = await clientSocket.SendAsync(messageBytes, SocketFlags.None);
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }

        private async Task ReceiveMessage()
        {
            try
            {
                while (true)
                {
                    byte[] messageBuffer = new byte[1024];
                    int messageLength = await clientSocket.ReceiveAsync(messageBuffer, SocketFlags.None);
                    if (messageLength == 0)
                    {
                        break;
                    }
                    Message message = Message.Parser.ParseFrom(messageBuffer, 0, messageLength);
                    this.dispatch.TryEnqueue(() => NewMessage?.Invoke(this, new MessageEventArgs(message)));
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
            finally
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
        }
    }
}
