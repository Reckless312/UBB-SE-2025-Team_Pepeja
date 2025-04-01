using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using System.ComponentModel;

namespace DirectMessages
{
    public partial class ChatRoomWindow : Window
    {
        public partial void Send_Button_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            this.service.SendMessage(this.MessageTextBox.Text);
            //Clear the input
            this.MessageTextBox.Text = "";
        }

        public partial void Mute_Button_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this.InvertedListView.SelectedItem is Message selectedMessage)
            {
                this.service.TryChangeMuteStatus(selectedMessage.MessageSenderName);
            }
        }

        public partial void Admin_Button_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this.InvertedListView.SelectedItem is Message selectedMessage)
            {
                this.service.TryChangeAdminStatus(selectedMessage.MessageSenderName);
            }
        }

        public partial void Kick_Button_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this.InvertedListView.SelectedItem is Message selectedMessage)
            {
                this.service.TryKick(selectedMessage.MessageSenderName);
            }
        }

        public partial void Clear_Button_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            this.messages.Clear();
        }

        public partial void OnHighlightedMessageChange(object sender, RoutedEventArgs routedEventArgs)
        {
            if(this.InvertedListView.SelectedItem is Message message)
            {
                // Check if the current user sent the message, in which case hide these buttons
                switch(message.MessageSenderName == this.userName)
                {
                    case true:
                        this.HideExtraButtonsFromUser();
                        break;
                    case false:
                        this.ShowAvailableButtons();
                        break;
                }
            }
        }
        
        private partial void HandleUserStatusChange(object? sender, ClientStatusEventArgs clientStatusEventArgs)
        {
            ClientStatus clientStatus = clientStatusEventArgs.ClientStatus;

            // Store the values locally, to update ui dinamically (ex. on selecting a new message)
            this.isHost = clientStatus.IsHost;
            this.isAdmin = clientStatus.IsAdmin;
            this.isMuted = clientStatus.IsMuted;

            this.ShowAvailableButtons();
        }

        private partial void HandleNewMessage(object? sender, MessageEventArgs messageEventArgs)
        {
            this.messages.Add(messageEventArgs.Message);

            // If the user has more than 100 message, we delete the oldest message, like specified in the
            // requirements of the dms
            while (this.messages.Count > 100)
            {
                this.messages.RemoveAt(0);
            }
        }

        private async partial void WaitAndConnectToTheServer()
        {
            // "XamlRoot" is required to display the errors
            while (this.Content.XamlRoot == null)
            {
                await Task.Delay(50);
            }
            this.service.ConnectUserToServer();
        }

        public partial void DisconnectService(object sender, WindowEventArgs args)
        {
            this.IsOpen = false;

            // Further call on the service (we attempt at disconnecting the client on window close)
            this.service.DisconnectClient();

            // Alert listeners about window closure
            if (this.WindowClosed != null)
            {
                this.WindowClosed.Invoke(this, true);
            }
        }
        
        private async partial void HandleException(object? sender, ExceptionEventArgs exceptionEventArgs)
        {
            // If somebody created this class, they could get an error if the window was closed fast
            // since the socket will attempt to connect for around 15 - 30 seconds
            if (!this.IsOpen)
            {
                return;
            }

            // ContentDialog is a pop up that tells about what exactly happened (the error message)
            ContentDialog errorDialog = new ContentDialog()
            {
                Title = "Request rejected!",
                Content = exceptionEventArgs.Exception.Message,
                CloseButtonText = "Ok",
                XamlRoot = this.Content.XamlRoot,
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(230, 219, 112, 147)), 
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White),
                CornerRadius = new CornerRadius(8)
            };

            // AI generated style for the pop up (it fits with the background)
            errorDialog.Resources["ContentDialogButtonBackground"] =
                new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.ColorHelper.FromArgb(255, 219, 112, 147));

            errorDialog.Resources["ContentDialogButtonForeground"] =
                new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.Colors.White);

            await errorDialog.ShowAsync();
        }

        private partial void HideExtraButtonsFromUser()
        {
            this.AdminButton.Visibility = Visibility.Collapsed;
            this.MuteButton.Visibility = Visibility.Collapsed;
            this.KickButton.Visibility = Visibility.Collapsed;
        }

        private partial void ShowAdminButtons()
        {
            this.MuteButton.Visibility = Visibility.Visible;
            this.KickButton.Visibility = Visibility.Visible;
        }

        private partial void ShowHostButtons()
        {
            this.AdminButton.Visibility = Visibility.Visible;
            this.ShowAdminButtons();
        }

        private partial void ShowAvailableButtons()
        {
            if (this.isHost)
            {
                this.ShowHostButtons();
            }
            else if (this.isAdmin)
            {
                this.ShowAdminButtons();
            }
            else
            {
                this.HideExtraButtonsFromUser();
            }

            // On mute, don't allow the user to send a message (hide the button)
            switch (this.isMuted)
            {
                case true:
                    this.SendButton.Visibility = Visibility.Collapsed;
                    break;
                case false:
                    this.SendButton.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
