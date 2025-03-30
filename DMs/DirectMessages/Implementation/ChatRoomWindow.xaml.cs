using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using System.ComponentModel;

namespace DirectMessages
{
    public partial class ChatRoomWindow : Window, INotifyPropertyChanged
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

        public partial void Friend_Request_Button_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this.InvertedListView.SelectedItem is Message message)
            {
                switch (this.FriendRequestButtonContent.Equals(ChatRoomWindow.CANCEL_FRIEND_REQUEST_CONTENT))
                {
                    case true:
                        this.service.CancelFriendRequest(message.MessageSenderName);
                        //Content -> the text that appears on the button
                        this.FriendRequestButtonContent = ChatRoomWindow.SEND_FRIEND_REQUEST_CONTENT;
                        break;
                    case false:
                        this.service.SendFriendRequest(message.MessageSenderName);
                        this.FriendRequestButtonContent = ChatRoomWindow.CANCEL_FRIEND_REQUEST_CONTENT;
                        break;
                }
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
                // Change the "content" of the friend request button based on the selected user
                // (if he is already in the friend request list or not)
                switch (this.service.IsInFriendRequests(message.MessageSenderName))
                {
                    case true:
                        this.FriendRequestButtonContent = ChatRoomWindow.CANCEL_FRIEND_REQUEST_CONTENT;
                        break;
                    case false:
                        this.FriendRequestButtonContent = ChatRoomWindow.SEND_FRIEND_REQUEST_CONTENT;
                        break;
                }

                // Check if the current user sent the message, in which case hide these buttons
                switch(message.MessageSenderName == this.userName)
                {
                    case true:
                        this.FriendRequestButton.Visibility = Visibility.Collapsed;
                        this.HideExtraButtonsFromUser();
                        break;
                    case false:
                        this.FriendRequestButton.Visibility = Visibility.Visible;
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

        public partial void DisconnectService()
        {
            // Further call on the service (we attempt at disconnecting the client on window close)
            this.service.DisconnectClient();
        }
        
        private async partial void HandleException(object? sender, ExceptionEventArgs exceptionEventArgs)
        {
            // ContentDialog is a pop up that tells about what exactly happened (the error message)
            ContentDialog errorDialog = new ContentDialog()
            {
                Title = "Request rejected!",
                Content = exceptionEventArgs.Exception.Message,
                CloseButtonText = "Ok",
                XamlRoot = this.Content.XamlRoot,
            };

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
