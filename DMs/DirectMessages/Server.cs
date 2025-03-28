using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Google.Protobuf;
using Windows.Services.Maps;

namespace DirectMessages
{
    internal class Server
    {
        private Socket serverSocket;
        private IPEndPoint ipEndPoint;
        private System.Threading.Timer? serverTimeout;
        private readonly object lockTimer;

        private ConcurrentDictionary<String, String> addressesAndUserNames;
        private ConcurrentDictionary<Socket, String> connectedClients;
        private ConcurrentDictionary<String, bool> mutedUsers;
        private ConcurrentDictionary<String, bool> adminUsers;

        private Regex muteCommandRegex;
        private Regex adminCommandRegex;
        private Regex kickCommandRegex;
        private Regex infoChangeCommandRegex;

        private String hostName;
        private String muteCommandPattern;
        private String adminCommandPattern;
        private String kickCommandPattern;
        private String infoCommandPattern;

        private bool isRunning;

        const int PORT_NUMBER = 6000;
        const int MESSAGE_MAXIMUM_SIZE = 4112;
        const int USER_NAME_MAXIMUM_SIZE = 512;
        const int NUMBER_OF_QUEUED_CONNECTIONS = 10;
        const int STARTING_INDEX = 0;
        const int DISCONNECT_CODE = 0;
        const int SERVER_TIMEOUT_COUNTDOWN = 180000;
        const int MINIMUM_CONNECTIONS = 2;
        const char ADDRESS_SEPARATOR = ':';
        const String ADMIN_STATUS = "ADMIN";
        const String MUTE_STATUS = "MUTE";
        const String KICK_STATUS = "KICK";
        const String HOST_STATUS = "HOST";
        const String REGULAR_USER_STATUS = "USER";
        const String INFO_CHANGE_MUTE_STATUS_COMMAND = "<INFO>|" + MUTE_STATUS + "|<INFO>";
        const String INFO_CHANGE_ADMIN_STATUS_COMMAND = "<INFO>|" + ADMIN_STATUS + "|<INFO>";
        const String INFO_CHANGE_KICK_STATUS_COMMAND = "<INFO>|" + KICK_STATUS + "|<INFO>";

        public Server(String hostAddress, String hostName)
        {
            this.muteCommandPattern = @"^<.*>\|Mute\|<.*>$";
            this.adminCommandPattern = @"^<.*>\|Admin\|<.*>$";
            this.kickCommandPattern = @"^<.*>\|Kick\|<.*>$";
            this.infoCommandPattern = @"^<INFO>\|.*\|<INFO>$";

            this.muteCommandRegex = new Regex(this.muteCommandPattern);
            this.adminCommandRegex = new Regex(this.adminCommandPattern);
            this.kickCommandRegex = new Regex(this.kickCommandPattern);
            this.infoChangeCommandRegex = new Regex(this.infoCommandPattern);

            this.addressesAndUserNames = new ConcurrentDictionary<string, string>();
            this.connectedClients = new ConcurrentDictionary<Socket, string>();
            this.mutedUsers = new ConcurrentDictionary<string, bool>();
            this.adminUsers = new ConcurrentDictionary<string, bool>();

            this.lockTimer = new object();

            this.hostName = hostName;

            try
            {
                this.ipEndPoint = new IPEndPoint(IPAddress.Parse(hostAddress), PORT_NUMBER);
                this.serverSocket = new(this.ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                this.serverSocket.Bind(this.ipEndPoint);
                this.serverSocket.Listen(NUMBER_OF_QUEUED_CONNECTIONS);
            }
            catch (Exception exception)
            {
                throw new ServerException($"Server create error: {exception.Message}");
            }

            this.isRunning = true;
        }

        public async void Start()
        {
            while (this.isRunning)
            {
                try
                {
                    Socket clientSocket = await this.serverSocket.AcceptAsync();

                    String socketNullResult = "Disconnected";
                    String ipAddressAndPort = clientSocket.RemoteEndPoint?.ToString() ?? socketNullResult;

                    String ipAddress = ipAddressAndPort.Substring(STARTING_INDEX, ipAddressAndPort.IndexOf(ADDRESS_SEPARATOR));
                    Debug.Write(ipAddress);
                    this.connectedClients.TryAdd(clientSocket, ipAddress);

                    this.CheckForMinimumConnections();

                    _ = Task.Run(() => HandleClient(clientSocket));
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"Client couldn't connect: {exception.Message}");
                }
            }
        }

        private async Task HandleClient(Socket clientSocket)
        {
            try
            {
                byte[] userNameBuffer = new byte[USER_NAME_MAXIMUM_SIZE];
                int userNameLength = await clientSocket.ReceiveAsync(userNameBuffer, SocketFlags.None);

                String userName = Encoding.UTF8.GetString(userNameBuffer, STARTING_INDEX, userNameLength);
                String ipAddress = this.connectedClients.GetValueOrDefault(clientSocket) ?? "";
                this.addressesAndUserNames.TryAdd(ipAddress, userName);
                this.adminUsers.TryAdd(userName, false);
                this.mutedUsers.TryAdd(userName, false);

                while (this.isRunning)
                {
                    byte[] messageBuffer = new byte[MESSAGE_MAXIMUM_SIZE];
                    int charactersReceivedCount = await clientSocket.ReceiveAsync(messageBuffer, SocketFlags.None);

                    if(!this.isRunning)
                    {
                        break;
                    }

                    String messageContentReceived = Encoding.UTF8.GetString(messageBuffer, STARTING_INDEX, charactersReceivedCount);

                    if (this.infoChangeCommandRegex.IsMatch(messageContentReceived))
                    {
                        continue;
                    }

                    if (charactersReceivedCount == DISCONNECT_CODE)
                    {
                        switch (this.IsHost(ipAddress))
                        {
                            case true:
                                messageContentReceived = "Host disconnected";
                                this.SendMessageToAllClients(CreateMessage(messageContentReceived, userName));
                                this.ShutDownServer();
                                break;
                            case false:
                                messageContentReceived = "Disconnected";
                                this.SendMessageToAllClients(CreateMessage(messageContentReceived, userName));
                                this.CheckForMinimumConnections();
                                break;
                        }

                        this.RemoveClientInformation(clientSocket, userName, ipAddress);
                        break;
                    }

                    bool commandFound = true, hasBeenKicked = false;

                    switch (true)
                    {
                        case true when this.muteCommandRegex.IsMatch(messageContentReceived):
                            this.TryChangeStatus(messageContentReceived, MUTE_STATUS, userName, this.mutedUsers);
                            this.SendMessageToOneClient(CreateMessage(INFO_CHANGE_MUTE_STATUS_COMMAND, userName), clientSocket);
                            break;
                        case true when this.adminCommandRegex.IsMatch(messageContentReceived):
                            this.TryChangeStatus(messageContentReceived, ADMIN_STATUS, userName, this.adminUsers);
                            this.SendMessageToOneClient(CreateMessage(INFO_CHANGE_ADMIN_STATUS_COMMAND, userName), clientSocket);
                            break;
                        case true when this.kickCommandRegex.IsMatch(messageContentReceived):
                            this.TryChangeStatus(messageContentReceived, KICK_STATUS, userName);
                            this.SendMessageToOneClient(CreateMessage(INFO_CHANGE_KICK_STATUS_COMMAND, userName), clientSocket);
                            this.RemoveClientInformation(clientSocket, userName, ipAddress);
                            hasBeenKicked = true;
                            break;
                        default:
                            commandFound = false;
                            break;
                    }

                    if(hasBeenKicked)
                    {
                        break;
                    }

                    if (commandFound)
                    { 
                        continue;
                    }

                    this.SendMessageToAllClients(CreateMessage(messageContentReceived, userName));
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

        private Message CreateMessage(String contentMessage, String userName)
        {
            Message message = new Message
            {
                MessageContent = contentMessage,
                MessageDateTime = DateTime.Now.ToString(),
                MessageSenderName = userName,
                MessageAligment = "Left",
                MessageSenderStatus = this.GetHighestStatus(userName),
            };

            return message;
        }

        private void SendMessageToAllClients(Message message)
        {
            foreach (KeyValuePair<Socket, String> clientSocketsAndAddresses in this.connectedClients)
            {
                this.SendMessageToOneClient(message, clientSocketsAndAddresses.Key);
            }
        }

        private void SendMessageToOneClient(Message message, Socket clientSocket)
        {
            byte[] messageBytes = message.ToByteArray();
            clientSocket.SendAsync(messageBytes, SocketFlags.None);
        }

        private bool IsHost(String ipAddress)
        {
            return ipAddress == ipEndPoint.Address.ToString();
        }

        private void InitializeServerTimeout()
        {
            lock (this.lockTimer)
            {
                this.serverTimeout?.Dispose();
                this.serverTimeout = new System.Threading.Timer((_) =>
                {
                    if (connectedClients.Count < MINIMUM_CONNECTIONS)
                    {
                        this.ShutDownServer();
                    }
                }, null, SERVER_TIMEOUT_COUNTDOWN, System.Threading.Timeout.Infinite);
            }
        }

        private void ShutDownServer()
        {
            if(this.serverSocket.Connected == true)
            {
                this.serverSocket.Shutdown(SocketShutdown.Both);
            }
            this.serverSocket.Close();
            this.isRunning = false;
        }

        private void CheckForMinimumConnections()
        {
            if (this.connectedClients.Count < MINIMUM_CONNECTIONS)
            {
                this.InitializeServerTimeout();
            }
        }

        public bool IsServerRunning()
        {
            return this.isRunning;
        }

        private String GetHighestStatus(String userName)
        {
            switch (true)
            {
                case true when this.hostName == userName:
                    return HOST_STATUS;
                case true when this.adminUsers.ContainsKey(userName):
                    return ADMIN_STATUS;
                default:
                    return REGULAR_USER_STATUS;
            }
        }

        private bool IsUserAllowedOnTargetStatusChange(String firstUserStatus, String secondUserStatus)
        {
            return (firstUserStatus == HOST_STATUS && secondUserStatus != HOST_STATUS) || (firstUserStatus == ADMIN_STATUS && secondUserStatus == REGULAR_USER_STATUS);
        }
        
        private String FindTargetedUserNameFromCommand(String Command)
        {
            int commandTargetIndex = 2;
            char commandSeparator = '|';
            String commandTarget = Command.Split(commandSeparator)[commandTargetIndex];

            int nameStartIndex = 1, nameEndIndex = commandTarget.Length - 2;
            String targetedUserName = commandTarget.Substring(nameStartIndex, nameEndIndex);

            return targetedUserName;
        }

        private void TryChangeStatus(String command, String targetedStatus, String userName, ConcurrentDictionary<string, bool>? statusDataHolder = null)
        {
            String targetedUserName = this.FindTargetedUserNameFromCommand(command);

            String userStatus = this.GetHighestStatus(userName);
            String targetedUserStatus = this.GetHighestStatus(targetedUserName);

            if (this.IsUserAllowedOnTargetStatusChange(userStatus, targetedUserStatus))
            {
                bool isStatus = statusDataHolder?.AddOrUpdate(targetedUserName, false, (key, oldValue) => !oldValue) ?? false;
                String messageContent;

                if (targetedStatus.Equals(MUTE_STATUS))
                {
                    switch (isStatus)
                    {
                        case true:
                            messageContent = $"{targetedUserName} has been muted";
                            break;
                        case false:
                            messageContent = $"{targetedUserName} has been unmuted";
                            break;
                    }
                }
                else if (targetedStatus.Equals(ADMIN_STATUS))
                {
                    switch (isStatus)
                    {
                        case true:
                            messageContent = $"{targetedUserName} has been granted admin status";
                            break;
                        case false:
                            messageContent = $"{targetedUserName} has been removed from admin status";
                            break;
                    }
                }
                else
                {
                    messageContent = $"{targetedUserName} has been kicked";
                }
                this.SendMessageToAllClients(CreateMessage(messageContent, userName));
            }
        }

        private void RemoveClientInformation(Socket clientSocket, String userName, String ipAddress)
        {
            this.addressesAndUserNames.TryRemove(ipAddress, out _);
            this.connectedClients.TryRemove(clientSocket, out _);
            this.adminUsers.TryRemove(userName, out _);
            this.mutedUsers.TryRemove(userName, out _);
        }
    }
}
