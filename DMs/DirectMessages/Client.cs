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
        // Commands are initiated by clients, they can be done via buttons or typed,
        // the server will still check for mismatches
        // Commands are checked via regex

        // From the <INFO>|something|<INFO> the user can receive in place of something:
        // ADMIN, MUTE, KICK => to which the client changes his new status

        private IPEndPoint serverEndPoint;
        private Socket clientSocket;
        private DispatcherQueue uiThread;
        private Regex infoChangeCommandRegex;
        
        // Separate class created to be used in events
        private ClientStatus clientStatus;

        public event EventHandler<MessageEventArgs> NewMessageReceivedEvent;
        public event EventHandler<ClientStatusEventArgs> ClientStatusChangedEvent;

        private String userName;
        private String infoChangeCommandPattern;

        /// <summary>
        /// Constructor for the client class
        /// </summary>
        /// <param name="hostIpAddress">Ip address for the end point</param>
        /// <param name="userName">Current client username</param>
        /// <param name="uiThread">Thread for updating the main window</param>
        /// <exception cref="Exception">EndPoint / IpAddress Errors</exception>
        public Client(String hostIpAddress, String userName, DispatcherQueue uiThread)
        {
            this.userName = userName;
            this.infoChangeCommandPattern = @"^<INFO>\|.*\|<INFO>$";
            this.uiThread = uiThread;

            // The host status is set after a client is created by the service
            this.clientStatus = new ClientStatus();

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

        /// <summary>
        /// Establish a connection to the server via end point
        /// </summary>
        /// <returns>A promise</returns>
        /// <exception cref="Exception">IO Errors</exception>
        public async Task ConnectToServer()
        {
            try
            {
                await clientSocket.ConnectAsync(serverEndPoint);

                // The server waits for the client to provide his username,
                // so that it can store it
                byte[] userNameToBytes = Encoding.UTF8.GetBytes(userName);
                _ = await clientSocket.SendAsync(userNameToBytes, SocketFlags.None);

                // Initialize a new task, a new thread in the thread pool,
                // to listen for incoming messages
                _ = Task.Run(() => ReceiveMessage());

                this.clientStatus.IsConnected = true;

                this.uiThread.TryEnqueue(() => this.ClientStatusChangedEvent?.Invoke(this, new ClientStatusEventArgs(clientStatus)));
            }
            catch(Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }

        /// <summary>
        /// Sends the provided message to the server
        /// </summary>
        /// <param name="message">Message to be sent</param>
        /// <returns>A promise</returns>
        /// <exception cref="Exception">Muted / IO Errors</exception>
        public async Task SendMessageToServer(String message)
        {
            try
            {
                byte[] messageToBytes = Encoding.UTF8.GetBytes(message);

                _ = await clientSocket.SendAsync(messageToBytes, SocketFlags.None);
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }

        /// <summary>
        /// Receive messages from the server and signal them to the service 
        /// </summary>
        /// <returns>A promise</returns>
        private async Task ReceiveMessage()
        {
            try
            {
                while (true)
                {
                    // For simplicity MESSAGE_MAXIMUM_SIZE is initialy set to a huge value,
                    // to be able to store UTF8 characters in one go
                    byte[] messageBuffer = new byte[Server.MESSAGE_MAXIMUM_SIZE];
                    int messageLength = await clientSocket.ReceiveAsync(messageBuffer, SocketFlags.None);

                    if (messageLength == Server.DISCONNECT_CODE)
                    {
                        break;
                    }

                    // Message class is provided by Google's ProtoBuf
                    // (easy solution for serializing data over the network)
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
                // An exception would mean something went wrong on the server,
                // hence we close the connection and update the IsConnected property,
                // to allow the main thread to throw errors, errors from here won't be catched
            }
            finally
            {
                this.CloseConnection();
            }
        }

        /// <summary>
        /// Gets the client status IsConnected Property
        /// </summary>
        /// <returns>True or False</returns>
        public bool IsConnected()
        {
            return this.clientStatus.IsConnected;
        }

        /// <summary>
        /// Updates the client status
        /// </summary>
        /// <param name="newStatus">Status keyword provided by the server</param>
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
                    this.CloseConnection();
                    break;
                default:
                    break;
            }

            this.uiThread.TryEnqueue(() => this.ClientStatusChangedEvent?.Invoke(this, new ClientStatusEventArgs(clientStatus)));
        }

        /// <summary>
        /// Sets the client as host
        /// </summary>
        public void SetIsHost()
        {
            this.clientStatus.IsHost = true;
        }

        /// <summary>
        /// Disconnects the client on shutdown
        /// </summary>
        /// <returns>A promise</returns>
        public async Task Disconnect()
        {
            try
            {
                _ = await clientSocket.SendAsync(new byte[Server.DISCONNECT_CODE], SocketFlags.None);
                this.CloseConnection();
            }
            catch (Exception)
            {
                // Ignore the exception (can happen if the client already disconnected)
            }
        }

        /// <summary>
        /// Closes the socket connection
        /// </summary>
        private void CloseConnection()
        {
            this.clientStatus.IsConnected = false;
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
    }
}
