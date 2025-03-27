using System;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;


namespace DirectMessages
{
    public sealed partial class ChatRoomWindow : Window
    {
        private IService service;
        private ObservableCollection<Message> messages;

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

            Microsoft.UI.Dispatching.DispatcherQueue uiThread = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

            this.messages = new ObservableCollection<Message>();
            this.service = new Service(userName, userIpAddress, serverInviteIp, uiThread);

            this.service.NewMessageEvent += HandleNewMessage;
            WaitOnConnectionToServer();

        }

        private async void WaitOnConnectionToServer()
        {
            while (this.Content.XamlRoot == null)
            {
                await Task.Delay(50);
            }

            try
            {
                await this.service.ConnectUserToServer();
            }
            catch(Exception exception)
            {
                await this.ShowError(exception);
            }
        }

        /// <summary>
        /// Listener for the NewMessageEvent.
        /// </summary>
        private void HandleNewMessage(object? sender, MessageEventArgs messageEventArgs)
        {
            this.messages.Add(messageEventArgs.Message);
        }

        public void Clear_Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Clear Button Clicked");
        }

        public void Admin_Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Admin Button Clicked");
        }

        public void Mute_Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Mute Button Clicked");
        }

        public void Kick_Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Kick Button Clicked");
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
    }
}
