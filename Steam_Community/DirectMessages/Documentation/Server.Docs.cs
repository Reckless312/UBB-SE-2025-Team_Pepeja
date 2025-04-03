using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Google.Protobuf;

namespace DirectMessages
{
    public partial class Server
    {
        private Socket serverSocket;
        private IPEndPoint ipEndPoint;
        private System.Threading.Timer? serverTimeout;
        private readonly object lockTimer;

        private ConcurrentDictionary<String, String> addressesAndUserNames;
        private ConcurrentDictionary<Socket, String> socketsAndAddresses;
        private ConcurrentDictionary<String, Socket> userNamesAndSockets;
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

        // Port number is always the same
        public const int PORT_NUMBER = 6000;

        public const int MESSAGE_MAXIMUM_SIZE = 4112;
        public const int USER_NAME_MAXIMUM_SIZE = 512;
        public const int MAXIMUN_NUMBER_OF_ACTIVE_CONNECTIONS = 20;
        public const int NUMBER_OF_QUEUED_CONNECTIONS = 10;
        public const int STARTING_INDEX = 0;
        public const int DISCONNECT_CODE = 0;
        public const int SERVER_TIMEOUT_COUNTDOWN = 180000;
        public const int MINIMUM_CONNECTIONS = 2;
        public const char ADDRESS_SEPARATOR = ':';
        public const String ADMIN_STATUS = "ADMIN";
        public const String MUTE_STATUS = "MUTE";
        public const String KICK_STATUS = "KICK";
        public const String HOST_STATUS = "HOST";
        public const String REGULAR_USER_STATUS = "USER";
        public const String INFO_CHANGE_MUTE_STATUS_COMMAND = "<INFO>|" + MUTE_STATUS + "|<INFO>";
        public const String INFO_CHANGE_ADMIN_STATUS_COMMAND = "<INFO>|" + ADMIN_STATUS + "|<INFO>";
        public const String INFO_CHANGE_KICK_STATUS_COMMAND = "<INFO>|" + KICK_STATUS + "|<INFO>";
        public const String SERVER_REJECT_COMMAND = "Server rejected your command!\n You don't have the rights to that user!";
        public const String SERVER_CAPACITY_REACHED = "Server capacity reached!\n Closing Connection!";

        /// <summary>
        /// Creates a server component which will handle messages and handle active users
        /// </summary>
        /// <param name="hostAddress">The ip address of the one who will host the server</param>
        /// <param name="hostName">The name of the one who will host the server</param>
        /// <exception cref="Exception">Ip address of the server provided in the wrong format
        ///                             Ip address provided is not the same with the one for the machine</exception>
        public Server(String hostAddress, String hostName)
        {
            // Info commands change a user status, they are sent by the server after receiving
            // a mute/admin/kick command. They follow the following pattern: "<nameOfTheInvoker>|targetedStatus|<nameOfTheTargetedUser>"

            this.muteCommandPattern = @"^<.*>\|" + Server.MUTE_STATUS + @"\|<.*>$";
            this.adminCommandPattern = @"^<.*>\|" + Server.ADMIN_STATUS + @"\|<.*>$";
            this.kickCommandPattern = @"^<.*>\|" + Server.KICK_STATUS + @"\|<.*>$";
            this.infoCommandPattern = @"^<INFO>\|.*\|<INFO>$";

            this.muteCommandRegex = new Regex(this.muteCommandPattern);
            this.adminCommandRegex = new Regex(this.adminCommandPattern);
            this.kickCommandRegex = new Regex(this.kickCommandPattern);
            this.infoChangeCommandRegex = new Regex(this.infoCommandPattern);

            this.addressesAndUserNames = new ConcurrentDictionary<string, string>();
            this.socketsAndAddresses = new ConcurrentDictionary<Socket, string>();
            this.userNamesAndSockets = new ConcurrentDictionary<string, Socket>();
            this.mutedUsers = new ConcurrentDictionary<string, bool>();
            this.adminUsers = new ConcurrentDictionary<string, bool>();

            // The data structures used for storing informations about the users are thread safe
            // but the server will also work with a timeout, so we use a lock to guarantee safety
            this.lockTimer = new object();

            this.hostName = hostName;

            try
            {
                this.ipEndPoint = new IPEndPoint(IPAddress.Parse(hostAddress), Server.PORT_NUMBER);
                this.serverSocket = new(this.ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                this.serverSocket.Bind(this.ipEndPoint);
                this.serverSocket.Listen(Server.NUMBER_OF_QUEUED_CONNECTIONS);
            }
            catch (Exception exception)
            {
                throw new Exception($"Server create error: {exception.Message}");
            }

            this.isRunning = true;
        }

        /// <summary>
        /// Listens to new connections from clients
        /// </summary>
        /// <exception cref="Exception">Accepting connections goes wrong (the server socket is disposed of, something on the client side...)
        ///                             When trying to get the ip address, if the remote end point is null
        ///                             When connecting, if no server is listening to requests</exception>
        public partial void Start();

        /// <summary>
        /// Listens for the username of the client and adds his data
        /// Handles messages received from the user, while also checking for commands,
        /// verifying if the user has the rights to initiate them and can be acted upon the targeted user
        /// </summary>
        /// <param name="clientSocket">The client socket of the client that the server is handling</param>
        /// <returns>A "promise", which can be waited and observed for any errors, in this case it's not</returns>
        ///  /// <exception cref="Exception">Socket error while waiting for a message)
        ///                                  Client socket being disposed of (from a force disconnect)
        ///                                  No ip address found in the server data
        ///                             When connecting, if no server is listening to requests</exception>
        private partial Task HandleClient(Socket clientSocket);

        /// <summary>
        /// Creates a new message object suitable for network serialization
        /// </summary>
        /// <param name="contentMessage">Message/command received from the user</param>
        /// <param name="userName">The name of the client who sent the message</param>
        /// <returns>A Message object (google protobuf class)</returns>
        private partial Message CreateMessage(String contentMessage, String userName);

        /// <summary>
        /// Sends a message to all connected users
        /// </summary>
        /// <param name="message">Message/command received from the user</param>
        private partial void SendMessageToAllClients(Message message);

        /// <summary>
        /// Send a message to one specific user
        /// </summary>
        /// <param name="message">Message/command received from the user</param>
        /// <param name="clientSocket">The client socket of the client that will receive the message</param>
        private partial void SendMessageToOneClient(Message message, Socket clientSocket);

        /// <summary>
        /// Checks to see if the user is host using their ip address
        /// </summary>
        /// <param name="ipAddress">The ip address of the user who we want to know if it's host</param>
        /// <returns>True if he is the host, false otherwise</returns>
        private partial bool IsHost(String ipAddress);

        /// <summary>
        /// Initializes the server timeout if the number of connected users is < MINIMUM_CONNECTION
        /// ( which is set to 2 ), closing the server at the end if the condition still applies
        /// The timer is set for 3 minutes
        /// </summary>
        private partial void InitializeServerTimeout();

        /// <summary>
        /// Closes connection from the server socket side
        /// </summary>
        private partial void ShutDownServer();

        /// <summary>
        /// Checks the minimum number of connections is met, if not, the server timeout is started
        /// </summary>
        private partial void StartTimeoutIfMinimumConnectionsNotMet();

        /// <summary>
        /// Returns wether the server is running or not
        /// </summary>
        /// <returns>True if it's running, false otherwise</returns>
        public partial bool IsServerRunning();

        /// <summary>
        /// Retrieves the users highest status (Host > Admin > Regular User)
        /// </summary>
        /// <param name="userName">The username of who we want to know the status</param>
        /// <returns></returns>
        private partial String GetHighestStatus(String userName);

        /// <summary>
        /// Checks if the user is allowed to change the status of the targeted user
        /// </summary>
        /// <param name="firstUserStatus">The status of the one who initiated the command</param>
        /// <param name="secondUserStatus">The status of the targeted user</param>
        /// <returns>True if it's able, false otherwise</returns>
        private partial bool IsUserAllowedOnTargetStatusChange(String firstUserStatus, String secondUserStatus);

        /// <summary>
        /// Find the target username from a command
        /// Command follows the pattern : <username>|Status|<username>
        /// </summary>
        /// <param name="command">The command that the user provided to the server</param>
        /// <returns>The username of the targeted user</returns>
        private partial String FindTargetedUserNameFromCommand(String command);

        /// <summary>
        /// Attempts at changing the status of the targeted user (if the user who initiated the request, 
        /// has a higher status). Users are notified of the change. If the status can't be changed,
        /// the user who initiated the command is notified.
        /// </summary>
        /// <param name="command">The command given by the user</param>
        /// <param name="targetedStatus">Can be one of the following values: Mute/Admin/Kick</param>
        /// <param name="userName">The username of the client who sent the request</param>
        /// <param name="statusDataHolder">The servers status data holder (one of the dictionaries), information will be updated</param>
        private partial void TryChangeStatus(String command, String targetedStatus, String userName, Socket userSocket, ConcurrentDictionary<string, bool>? statusDataHolder = null);

        /// <summary>
        /// Removes all data for a specified user
        /// </summary>
        /// <param name="clientSocket">The client socket</param>
        /// <param name="userName">The client username</param>
        /// <param name="ipAddress">The client ip address</param>
        private partial void RemoveClientInformation(Socket clientSocket, String userName, String ipAddress);
    }
}
