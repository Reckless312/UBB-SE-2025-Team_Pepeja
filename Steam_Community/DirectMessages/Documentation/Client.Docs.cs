using Microsoft.UI.Dispatching;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DirectMessages
{
    public partial class Client
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

        /// <summary>
        /// Creates the client component used to connect to a server
        /// </summary>
        /// <param name="hostIpAddress">The ip address of the user who sent the invite</param>
        /// <param name="userName">The name of the user who joined the chat room</param>
        /// <param name="uiThread">Updating the ui can be done using only the main thread</param>
        /// <exception cref="Exception">Ip address of the server provided in the wrong format</exception>
        public Client(String hostIpAddress, String userName, DispatcherQueue uiThread)
        {
            this.userName = userName;
            
            // A client will only receive valid commands in the form "<INFO>|Status|<INFO>"
            // Once he receives that status, he will change the value to it's negation (admin -> !admin) from the client status
            this.infoChangeCommandPattern = @"^<INFO>\|.*\|<INFO>$";
            this.uiThread = uiThread;

            this.clientStatus = new ClientStatus();

            // Command matches are found via regex
            this.infoChangeCommandRegex = new Regex(this.infoChangeCommandPattern);

            try
            {
                this.serverEndPoint = new IPEndPoint(IPAddress.Parse(hostIpAddress), Server.PORT_NUMBER);
                this.clientSocket = new(serverEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }

        /// <summary>
        /// Connects the client to the created end point
        /// Creates a new "thread" for the client to receive messages in paralel
        /// </summary>
        /// <returns>A "promise", which can be waited and observed for any errors</returns>
        /// <exception cref="Exception">No server is listening to requests
        ///                             Server closed connection before sending the client username</exception>
        public partial Task ConnectToServer();

        /// <summary>
        /// Sends the provided message to the server
        /// </summary>
        /// <param name="message">The input provided by the user in the textbox
        ///                       Or commands</param>
        /// <returns>A "promise", which can be waited and observed for any errors</returns>
        /// <exception cref="Exception">Server closed connection before sending the message</exception>
        
        public partial Task SendMessageToServer(String message);

        /// <summary>
        /// Receive messages from the server and signals the event for the service
        /// </summary>
        /// <returns>A "promise", which can be waited and observed for any errors</returns>
        /// /// <exception cref="Exception">Server closed connection before receiving the message
        ///                                 Trying to access the client socket but it's disposed</exception>
        private partial Task ReceiveMessage();

        /// <summary>
        /// Returns the client connection status
        /// </summary>
        /// <returns>True if it's still connected, false otherwise</returns>
        public partial bool IsConnected();

        /// <summary>
        /// Updates the client status
        /// </summary>
        /// <param name="newStatus">Status keyword provided by the server</param>
        private partial void UpdateClientStatus(String newStatus);

        /// <summary>
        /// Sets the client status to host
        /// </summary>
        public partial void SetIsHost();

        /// <summary>
        /// Attempts to disconnects the client from the server (if the client didn't already somehow disconnect)
        /// </summary>
        public partial void Disconnect();

        /// <summary>
        /// Closes client connection to the server
        /// </summary>
        private partial void CloseConnection();
    }
}
