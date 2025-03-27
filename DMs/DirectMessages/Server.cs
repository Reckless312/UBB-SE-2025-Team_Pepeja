using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;

namespace DirectMessages
{
    internal class Server
    {
        private Socket serverSocket;
        private IPEndPoint ipEndPoint;

        private ConcurrentDictionary<String, String> addressesAndUserNames;
        private ConcurrentDictionary<Socket, String> connectedClients;

        private String hostName;

        const int PORT_NUMBER = 6000;
        const int MESSAGE_SIZE = 512;
        const int NUMBER_OF_QUEUED_CONNECTIONS = 10;
        const int STARTING_INDEX = 0;
        const int DISCONNECT_CODE = 0;
        const char ADDRESS_SEPARATOR = ':';

        public Server(String hostAddress, String hostName)
        {
            this.addressesAndUserNames = new ConcurrentDictionary<string, string>();
            this.connectedClients = new ConcurrentDictionary<Socket, string>();

            this.hostName = hostName;

            try
            {
                this.ipEndPoint = new IPEndPoint(IPAddress.Parse(hostAddress), PORT_NUMBER);
                this.serverSocket = new(this.ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                this.serverSocket.Bind(this.ipEndPoint);
                this.serverSocket.Listen(NUMBER_OF_QUEUED_CONNECTIONS);
            }
            catch(Exception exception)
            {
                throw new ServerException($"Server create error: {exception.Message}");
            }
        }

        public async void Start()
        {
            while (true)
            {
                try
                {
                    Socket clientSocket = await this.serverSocket.AcceptAsync();
                    
                    String socketNullResult = "Disconnected";
                    String ipAddressAndPort = clientSocket.RemoteEndPoint?.ToString() ?? socketNullResult;
 
                    String ipAddress = ipAddressAndPort.Substring(STARTING_INDEX, ipAddressAndPort.IndexOf(ADDRESS_SEPARATOR));
                    this.connectedClients.TryAdd(clientSocket, ipAddress);

                    _ = Task.Run(() => HandleClient(clientSocket));
                }
                catch(Exception exception)
                {
                    Debug.WriteLine($"Client couldn't connect: {exception.Message}");
                }
            }
        }

        private async Task HandleClient(Socket clientSocket)
        {
            try
            {
                byte[] userNameBuffer = new byte[MESSAGE_SIZE];
                int userNameLength = await clientSocket.ReceiveAsync(userNameBuffer, SocketFlags.None);

                String userName = Encoding.ASCII.GetString(userNameBuffer, STARTING_INDEX, userNameLength);
                String ipAddress = this.connectedClients.GetValueOrDefault(clientSocket) ?? "";
                this.addressesAndUserNames.TryAdd(ipAddress, userName);

                while (true)
                {
                    byte[] messageBuffer = new byte[MESSAGE_SIZE];
                    int charactersReceivedCount = await clientSocket.ReceiveAsync(messageBuffer, SocketFlags.None);

                    String messageContentReceived = Encoding.ASCII.GetString(messageBuffer, STARTING_INDEX, charactersReceivedCount);

                    if (charactersReceivedCount == DISCONNECT_CODE)
                    {
                        switch(this.IsHost(ipAddress)){
                            case true:
                                messageContentReceived = "Host disconnected";
                                this.SendMessageToClients(CreateMessage(messageContentReceived, ipAddress));
                                this.serverSocket.Shutdown(SocketShutdown.Both);
                                this.serverSocket.Close();
                                break;
                            case false:
                                messageContentReceived = "Disconnected";
                                this.SendMessageToClients(CreateMessage(messageContentReceived, ipAddress));
                                break;
                        }

                        this.addressesAndUserNames.TryRemove(ipAddress, out _);
                        this.connectedClients.TryRemove(clientSocket, out _);
                        break;
                    }

                    this.SendMessageToClients(CreateMessage(messageContentReceived, ipAddress));
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Client had an error: {exception.Message}");
            }
            finally
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
        }

        private Message CreateMessage(String contentMessage, String ipAddress)
        {
            Message message = new Message
            {
                MessageContent = contentMessage,
                MessageDateTime = DateTime.Now.ToString(),
                MessageSenderName = this.addressesAndUserNames.GetValueOrDefault(ipAddress),
                MessageAligment = "Left",
            };

            return message;
        }

        private void SendMessageToClients(Message message)
        {
            foreach (KeyValuePair<Socket, String> clientAddress in this.connectedClients)
            {
                byte[] messageBytes = message.ToByteArray();
                clientAddress.Key.SendAsync(messageBytes, SocketFlags.None);
            }
        }

        private bool IsHost(String ipAddress)
        {
            return IPAddress.Parse(ipAddress) == ipEndPoint.Address;
        }
    }
}
