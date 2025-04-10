using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Steam_Community.DirectMessages.Models;
using Steam_Community.DirectMessages.Interfaces;

namespace Steam_Community.DirectMessages.Services
{
    /// <summary>
    /// Implements network client functionality for connecting to and communicating with the chat server.
    /// </summary>
    public class NetworkClient : INetworkClient
    {
        private IPEndPoint _serverEndPoint;
        private Socket _clientSocket;
        private DispatcherQueue _uiDispatcherQueue;
        private Regex _infoChangeCommandRegex;

        private UserStatus _userStatus;

        public event EventHandler<MessageEventArgs> MessageReceived;
        public event EventHandler<UserStatusEventArgs> UserStatusChanged;

        private string _username;
        private readonly string _infoChangeCommandPattern = @"^<INFO>\|.*\|<INFO>$";

        /// <summary>
        /// Initializes a new instance of the NetworkClient class.
        /// </summary>
        /// <param name="hostIpAddress">The IP address of the server host.</param>
        /// <param name="username">The username of the current user.</param>
        /// <param name="uiDispatcherQueue">The dispatcher queue for the UI thread.</param>
        /// <exception cref="Exception">Thrown when the IP address is invalid.</exception>
        public NetworkClient(string hostIpAddress, string username, DispatcherQueue uiDispatcherQueue)
        {
            _username = username;
            _uiDispatcherQueue = uiDispatcherQueue;
            _userStatus = new UserStatus();
            _infoChangeCommandRegex = new Regex(_infoChangeCommandPattern);

            try
            {
                _serverEndPoint = new IPEndPoint(IPAddress.Parse(hostIpAddress), ChatConstants.PORT_NUMBER);
                _clientSocket = new Socket(_serverEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to create network client: {exception.Message}");
            }
        }

        /// <summary>
        /// Connects to the chat server.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when connection fails.</exception>
        public async Task ConnectToServer()
        {
            try
            {
                // Connect to the server
                await _clientSocket.ConnectAsync(_serverEndPoint);

                // Send the username to the server
                byte[] userNameBytes = Encoding.UTF8.GetBytes(_username);
                _ = await _clientSocket.SendAsync(userNameBytes, SocketFlags.None);

                // Start receiving messages in the background
                _ = Task.Run(() => ReceiveMessages());

                _userStatus.IsConnected = true;

                // Notify UI of status change
                _uiDispatcherQueue.TryEnqueue(() =>
                    UserStatusChanged?.Invoke(this, new UserStatusEventArgs(_userStatus)));
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to connect to server: {exception.Message}");
            }
        }

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="messageContent">The content of the message to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when sending fails.</exception>
        public async Task SendMessageToServer(string messageContent)
        {
            try
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(messageContent);
                _ = await _clientSocket.SendAsync(messageBytes, SocketFlags.None);
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to send message: {exception.Message}");
            }
        }

        /// <summary>
        /// Continuously receives messages from the server.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ReceiveMessages()
        {
            try
            {
                while (true)
                {
                    // Receive message from server
                    byte[] messageBuffer = new byte[ChatConstants.MESSAGE_MAXIMUM_SIZE];
                    int messageLength = await _clientSocket.ReceiveAsync(messageBuffer, SocketFlags.None);

                    // Check for disconnection
                    if (messageLength == ChatConstants.DISCONNECT_CODE)
                    {
                        break;
                    }

                    // Parse the message
                    Message message = Message.Parser.ParseFrom(messageBuffer, ChatConstants.STARTING_INDEX, messageLength);

                    // Check if this is a status command
                    if (_infoChangeCommandRegex.IsMatch(message.MessageContent))
                    {
                        int statusIndex = 1;
                        char commandSeparator = '|';
                        string newStatus = message.MessageContent.Split(commandSeparator)[statusIndex];

                        UpdateUserStatus(newStatus);
                        continue;
                    }

                    // Notify UI of the new message
                    _uiDispatcherQueue.TryEnqueue(() =>
                        MessageReceived?.Invoke(this, new MessageEventArgs(message)));
                }
            }
            catch (Exception)
            {
                // Handle disconnection
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Checks if the client is connected to the server.
        /// </summary>
        /// <returns>True if connected, false otherwise.</returns>
        public bool IsConnected()
        {
            return _userStatus.IsConnected;
        }

        /// <summary>
        /// Updates the user's status based on a status command.
        /// </summary>
        /// <param name="newStatus">The new status to apply.</param>
        private void UpdateUserStatus(string newStatus)
        {
            switch (newStatus)
            {
                case ChatConstants.ADMIN_STATUS:
                    _userStatus.IsAdmin = !_userStatus.IsAdmin;
                    break;
                case ChatConstants.MUTE_STATUS:
                    _userStatus.IsMuted = !_userStatus.IsMuted;
                    break;
                case ChatConstants.KICK_STATUS:
                    CloseConnection();
                    break;
                default:
                    // Ignore invalid status commands
                    break;
            }

            // Notify UI of status change
            _uiDispatcherQueue.TryEnqueue(() =>
                UserStatusChanged?.Invoke(this, new UserStatusEventArgs(_userStatus)));
        }

        /// <summary>
        /// Sets the client as the host of the chat room.
        /// </summary>
        public void SetAsHost()
        {
            _userStatus.IsHost = true;
        }

        /// <summary>
        /// Disconnects from the server.
        /// </summary>
        public async void Disconnect()
        {
            try
            {
                // Send empty message to indicate disconnection
                _ = await _clientSocket.SendAsync(new byte[ChatConstants.DISCONNECT_CODE], SocketFlags.None);
                CloseConnection();
            }
            catch (Exception)
            {
                // Ignore exceptions during disconnection
            }
        }

        /// <summary>
        /// Closes the connection to the server.
        /// </summary>
        private void CloseConnection()
        {
            _userStatus.IsConnected = false;

            try
            {
                _clientSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
                // Socket might already be closed
            }

            _clientSocket.Close();
        }
    }
}