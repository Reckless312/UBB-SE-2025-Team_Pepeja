using System;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using System.ComponentModel;

namespace DirectMessages
{
    /// <summary>
    /// For test purposes: It's recommended that virtual machines are used to simulate different
    /// ip addresses, but multiple instances can be started from the same machine with the issue that
    /// the server will hold the same ip for different usernames (aka if the host who is also a user
    /// kicks the user, the host is kicked because of ip addresses)
    /// Connection also requires the true ip address of the host, otherwise the server will not be created
    /// Not tested outside of local network
    /// Issues tend to rise if closing the application directly (not by closing the window), like
    /// not removing the client from the server so the socket is disposable (that should be fixed but
    /// other thing may arise)
    /// </summary>
    public partial class ChatRoomWindow : Window
    {
        private IService service;
        private ObservableCollection<Message> messages;

        private String userName;

        private bool isAdmin;
        private bool isHost;
        private bool isMuted;

        public event EventHandler<bool> WindowClosed;

        /// <summary>
        /// This property is bound to the ListView from the View
        /// </summary>
        public ObservableCollection<Message> Messages
        {
            get => this.messages;
        }

        /// <summary>
        /// This property is used to trigger a change in the text shown by the friend
        /// request button
        /// </summary>

        private bool IsOpen { get; set; }

        /// <summary>
        /// Creates a new window representing a chat room for users
        /// </summary>
        /// <param name="userName">The name of the user who joined the chat room</param>
        /// <param name="serverInviteIp">The ip of the person who invited the user
        ///                              Don't provide the argument if you want to host</param>
        public ChatRoomWindow(String userName, String serverInviteIp=Service.HOST_IP_FINDER)
        {
            this.InitializeComponent();

            //Extra buttons: Admin/Mute/Kick
            this.HideExtraButtonsFromUser();

            //In the client we use the thread pool, but we need to update the ui in the main thread, so we capture it
            Microsoft.UI.Dispatching.DispatcherQueue uiThread = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

            this.userName = userName;
            this.IsOpen = true;
            this.messages = new ObservableCollection<Message>();
            this.service = new Service(userName, serverInviteIp, uiThread);

            //Events -> if something happened, alert the listeners, in this case we are the listeners
            //          and we assign functions for each trigger of an event
            this.service.NewMessageEvent += HandleNewMessage;
            this.service.ClientStatusChangedEvent += HandleUserStatusChange;
            this.service.ExceptionEvent += HandleException;

            this.Closed += this.DisconnectService;

            WaitAndConnectToTheServer();
        }

        /// <summary>
        /// Will take the input provided by the user in the textbox and will send it to the other connected
        /// users via the server
        /// Parameters from the function overload are not used
        /// </summary>
        public partial void Send_Button_Click(object sender, RoutedEventArgs routedEventArgs);

        /// <summary>
        /// Will take the name of the sender of the selected message and will send a command to the
        /// server to either mute / unmute the user (depends on his current status)
        /// Parameters from the function overload are not used
        /// </summary>
        public partial void Mute_Button_Click(object sender, RoutedEventArgs routedEventArgs);

        /// <summary>
        /// Will take the name of the sender of the selected message and will send a command to the
        /// server to either make the user an admin / remove the admin status from the user
        /// (depends on his current status)
        /// Parameters from the function overload are not used
        /// </summary>
        public partial void Admin_Button_Click(object sender, RoutedEventArgs routedEventArgs);

        /// <summary>
        /// Will take the name of the sender of the selected message and will send a command to the
        /// server to kick the user
        /// Parameters from the function overload are not used
        /// </summary>
        public partial void Kick_Button_Click(object sender, RoutedEventArgs routedEventArgs);

        /// <summary>
        /// Clears all messages that are shown on the screen
        /// </summary>
        public partial void Clear_Button_Click(object sender, RoutedEventArgs routedEventArgs);

        /// <summary>
        /// When the user changes selected messages, some buttons will show/hide
        /// For example if the user selects himself, all buttons beside "clear" will disappear
        /// Otherwise, the displayed buttons will be those that the user has access to
        /// Host has access to all buttons, Admin doesn't have access to "Admin", and the regular
        /// user also doesn't have access to "Mute" or "Kick"
        /// </summary>
        public partial void OnHighlightedMessageChange(object sender, RoutedEventArgs routedEventArgs);

        /// <summary>
        /// Service will give back information about the current clients status (HOST, ADMIN, USER),
        /// once received, the buttons available change ("Mute" status will hide the "send" button)
        /// </summary>
        /// <param name="sender">The one who initiated the event is the service in our case</param>
        /// <param name="clientStatusEventArgs">Events require a separate class which contains a getter
        ///                                     for the "observed" object (in our case ClientStatus)
        ///                                     https://learn.microsoft.com/en-us/dotnet/standard/events/</param>
        private partial void HandleUserStatusChange(object? sender, ClientStatusEventArgs clientStatusEventArgs);

        /// <summary>
        /// Service will give back messages sent from thhey can be displayed
        /// Message is a auto generated class using Google protobuf, alle server to the client, so towing for simple serialization of data
        /// across the network
        /// </summary>
        /// <param name="sender">The one who initiated the event is the service in our case</param>
        /// <param name="messageEventArgs">Events require a separate class which contains a getter
        ///                                     for the "observed" object (in our case Message)
        ///                                     https://learn.microsoft.com/en-us/dotnet/standard/events/</param>
        private partial void HandleNewMessage(object? sender, MessageEventArgs messageEventArgs);

        /// <summary>
        /// Connects the current client to the server
        /// Also "Waits" for the window to initialize it's components, allowing it to display errors
        /// </summary>
        private partial void WaitAndConnectToTheServer(); 

        /// <summary>
        /// When the client closes the window, this function is triggered, and will attempt at
        /// closing the connection to the server ("attempt" - could be already disconnected)
        /// </summary>
        public partial void DisconnectService(object sender, WindowEventArgs args);    

        /// <summary>
        /// Displays the error propagated from the server to the user
        /// This is vague, as in a errors can happen from network issues or service related
        /// (length of message, client already disconnected, etc.)
        /// Will be explored in the service, client, and server for each one that can happen (and I found)
        /// </summary>
        /// <param name="exception">A service / network related issue</param>
        private partial void HandleException(object? sender, ExceptionEventArgs exceptionEventArgs);

        /// <summary>
        /// Hides Mute/Admin/Kick buttons
        /// </summary>
        private partial void HideExtraButtonsFromUser();

        /// <summary>
        /// Shows Mute/Kick buttons
        /// </summary>
        private partial void ShowAdminButtons();

        /// <summary>
        /// Shows Mute/Kick/Admin buttons
        /// </summary>
        private partial void ShowHostButtons();

        /// <summary>
        /// Will check the current status (Mute/Admin/Host/RegularUser) of the user
        /// and will display the available buttons
        /// </summary>
        private partial void ShowAvailableButtons();
    }
}

