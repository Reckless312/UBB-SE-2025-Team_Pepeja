using System;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;

namespace DirectMessages
{
    public sealed partial class ChatRoomWindow : Window
    {
        // For mute/unmute, make/remove admin, kick user the server will done the final checkup
        // (if the user who initiated the request has a higher rank than the targeted user)

        // Interface used for later assignments

        private IService service;
        private ObservableCollection<Message> messages;

        private String userName;

        public const String SEND_FRIEND_REQUEST_CONTENT = "Send Friend Request";
        public const String CANCEL_FRIEND_REQUEST_CONTENT = "Cancel Friend Request";

        /// <summary>
        /// Property used by the UI to display messages.
        /// </summary>
        public ObservableCollection<Message> Messages
        {
            get => this.messages;
        }

        /// <summary>
        /// Constructor for the ChatRoomWindow class.
        /// </summary>
        /// <param name="userName">Current user name</param>
        /// <param name="userIpAddress">Current user ip address</param>
        /// <param name="serverInviteIp">The ip address of the server that invited the user
        ///                              (can be "None" => the current user invited someone)</param>
        public ChatRoomWindow(String userName, String userIpAddress, String serverInviteIp)
        {
            this.InitializeComponent();
            this.HideExtraButtonsFromUser();

            // The UI thread is further used to invoke events and update states (messages received or current client states)
            Microsoft.UI.Dispatching.DispatcherQueue uiThread = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            
            this.userName = userName;
            this.messages = new ObservableCollection<Message>();
            this.service = new Service(userName, userIpAddress, serverInviteIp, uiThread);

            // "Subscribe" to the service events
            this.service.NewMessageEvent += HandleNewMessage;
            this.service.ClientStatusChangedEvent += HandleUserStatusChange;

            WaitOnConnectionToServer();
        }

        /// <summary>
        /// Sends the message written in the MessageTextBox.
        /// </summary>
        public async void Send_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await this.service.SendMessage(this.MessageTextBox.Text);
            }
            catch (Exception exception)
            {
                await this.ShowError(exception);
            }
            this.MessageTextBox.Text = "";
        }

        /// <summary>
        /// Tries to mute/unmute a user
        /// </summary>
        public void Mute_Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.InvertedListView.SelectedItem is Message selectedMessage)
            {
                this.service.TryChangeMuteStatus(selectedMessage.MessageSenderName);
            }
        }

        /// <summary>
        /// Tries to make a user an admin or remove the status if he already is an admin
        /// </summary>
        public void Admin_Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.InvertedListView.SelectedItem is Message selectedMessage)
            {
                this.service.TryChangeAdminStatus(selectedMessage.MessageSenderName);
            }
        }

        /// <summary>
        /// Tries to kick the user from the chat
        /// </summary>
        public void Kick_Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.InvertedListView.SelectedItem is Message selectedMessage)
            {
                this.service.TryKick(selectedMessage.MessageSenderName);
            }
        }

        /// <summary>
        /// Sends a friend request to the selected user via message
        /// </summary>
        public void Friend_Request_Click(object sender, RoutedEventArgs e)
        {
            if (this.InvertedListView.SelectedItem is Message message)
            {
                switch (true)
                {
                    case true when this.FriendRequestButton.Content.Equals(ChatRoomWindow.CANCEL_FRIEND_REQUEST_CONTENT):
                        this.service.CancelFriendRequest(message.MessageSenderName);
                        break;
                    default:
                        this.service.SendFriendRequest(message.MessageSenderName);
                        break;
                }
            }
        }

        /// <summary>
        /// Clear Displayed messages
        /// </summary>
        public void Clear_Button_Click(object sender, RoutedEventArgs e)
        {
            this.messages.Clear();
        }

        /// <summary>
        /// Listener for a new message selected
        /// Changes visibility or content for the friend request button
        /// </summary>
        public void OnHighlightedMessageChange(object sender, RoutedEventArgs routedEventArgs)
        {
            if(this.InvertedListView.SelectedItem is Message message)
            {
                switch (true)
                {
                    case true when message.MessageSenderName == this.userName:
                        this.FriendRequestButton.Visibility = Visibility.Collapsed;
                        break;
                    case true when this.service.IsInFriendRequests(message.MessageSenderName):
                        this.FriendRequestButton.Visibility= Visibility.Visible;
                        this.FriendRequestButton.Content = ChatRoomWindow.CANCEL_FRIEND_REQUEST_CONTENT;
                        break;
                    default:
                        this.FriendRequestButton.Visibility = Visibility.Visible;
                        this.FriendRequestButton.Content = ChatRoomWindow.SEND_FRIEND_REQUEST_CONTENT;
                        break;
                }
            }
        }
        
        /// <summary>
        /// Listener for ClientChangedStatusEvent
        /// Changes visibility for Admin/Mute/Kick buttons
        /// </summary>
        /// <param name="sender">The service</param>
        /// <param name="clientStatusEventArgs">Contains an instance of ClientStatus Object</param>
        private void HandleUserStatusChange(object? sender, ClientStatusEventArgs clientStatusEventArgs)
        {
            ClientStatus clientStatus = clientStatusEventArgs.ClientStatus;
            switch (true)
            {
                case true when clientStatus.IsHost:
                    this.ShowHostButtons();
                    break;
                case true when clientStatus.IsAdmin:
                    this.ShowAdminButtons();
                    break;
                default:
                    this.HideExtraButtonsFromUser();
                    break;
            }
        }

        /// <summary>
        /// Listener for NewMessageEvent
        /// Adds a new message to be visible on the screen
        /// </summary>
        /// <param name="sender">The service</param>
        /// <param name="messageEventArgs">Contains an instance of a Message object</param>
        private void HandleNewMessage(object? sender, MessageEventArgs messageEventArgs)
        {
            this.messages.Add(messageEventArgs.Message);

            // Only the latest 100 messages are stored
            while (this.messages.Count > 100)
            {
                this.messages.RemoveAt(0);
            }
        }

        /// <summary>
        /// Connects the client to the server
        /// </summary>
        private async void WaitOnConnectionToServer()
        {
            // Wait until XamlRoot is initialized so we can display errors
            while (this.Content.XamlRoot == null)
            {
                await Task.Delay(50);
            }

            try
            {
                await this.service.ConnectUserToServer();
            }
            catch (Exception exception)
            {
                await this.ShowError(exception);
            }
        }

        /// <summary>
        /// Disconnects clients on window close
        /// </summary>
        /// <returns></returns>
        public async Task OnDisconnectService()
        {
            await this.service.DisconnectClient();
        }
        
        /// <summary>
        /// Display a error to the user
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        private async Task ShowError(Exception exception)
        {
            ContentDialog errorDialog = new ContentDialog()
            {
                Title = "An error has occured!",
                Content = exception.Message,
                CloseButtonText = "Ok",
                XamlRoot = this.Content.XamlRoot,
            };

            await errorDialog.ShowAsync();
        }

        /// <summary>
        /// Hides all extra buttons from a regular user
        /// </summary>
        private void HideExtraButtonsFromUser()
        {
            this.AdminButton.Visibility = Visibility.Collapsed;
            this.MuteButton.Visibility = Visibility.Collapsed;
            this.KickButton.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Shows the buttons available for an admin
        /// </summary>
        private void ShowAdminButtons()
        {
            this.MuteButton.Visibility = Visibility.Visible;
            this.KickButton.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Shows the buttons available for a host (all)
        /// </summary>
        private void ShowHostButtons()
        {
            this.AdminButton.Visibility = Visibility.Visible;
            this.ShowAdminButtons();
        }
    }
}
