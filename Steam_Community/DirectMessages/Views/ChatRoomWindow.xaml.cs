using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Steam_Community.DirectMessages.ViewModels;
using Steam_Community.DirectMessages.Models;

namespace Steam_Community.DirectMessages.Views
{
    /// <summary>
    /// Represents a window for chatting with other users.
    /// </summary>
    public partial class ChatRoomWindow : Window
    {
        private ChatRoomViewModel viewModel;

        /// <summary>
        /// Event that is raised when the window is closed.
        /// </summary>
        public event EventHandler<bool> WindowClosed;

        /// <summary>
        /// Gets the collection of messages to display.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<Message> Messages
        {
            get => this.viewModel.Messages;
        }

        /// <summary>
        /// Initializes a new instance of the ChatRoomWindow class.
        /// </summary>
        /// <param name="username">The username of the current user.</param>
        /// <param name="serverInviteIpAddress">The IP address of the server host, or HOST_IP_FINDER if this user is the host.</param>
        public ChatRoomWindow(string username, string serverInviteIpAddress = ChatConstants.HOST_IP_FINDER)
        {
            this.InitializeComponent();

            // Hide moderation buttons initially
            this.HideModeratorButtons();

            // Get UI thread dispatcher for UI updates
            Microsoft.UI.Dispatching.DispatcherQueue uiDispatcherQueue =
                Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

            // Create view model
            this.viewModel = new ChatRoomViewModel(username, serverInviteIpAddress, uiDispatcherQueue);

            // Register event handlers
            this.viewModel.StatusChanged += HandleStatusChanged;
            this.viewModel.ExceptionOccurred += HandleException;
            this.viewModel.WindowClosed += (sender, args) => this.WindowClosed?.Invoke(this, args);

            // Register window closed event
            this.Closed += this.HandleWindowClosed;

            // Connect to server
            ConnectToServer();
        }

        /// <summary>
        /// Sends the message entered by the user.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="routedEventArgs">The event arguments.</param>
        public void SendButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            string messageContent = this.MessageTextBox.Text;
            this.viewModel.SendMessage(messageContent);

            // Clear input field
            this.MessageTextBox.Text = "";
        }

        /// <summary>
        /// Attempts to change the mute status of the selected user.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="routedEventArgs">The event arguments.</param>
        public void MuteButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this.InvertedListView.SelectedItem is Message selectedMessage)
            {
                this.viewModel.AttemptChangeMuteStatus(selectedMessage.MessageSenderName);
            }
        }

        /// <summary>
        /// Attempts to change the admin status of the selected user.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="routedEventArgs">The event arguments.</param>
        public void AdminButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this.InvertedListView.SelectedItem is Message selectedMessage)
            {
                this.viewModel.AttemptChangeAdminStatus(selectedMessage.MessageSenderName);
            }
        }

        /// <summary>
        /// Attempts to kick the selected user from the chat room.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="routedEventArgs">The event arguments.</param>
        public void KickButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this.InvertedListView.SelectedItem is Message selectedMessage)
            {
                this.viewModel.AttemptKickUser(selectedMessage.MessageSenderName);
            }
        }

        /// <summary>
        /// Clears all messages from the display.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="routedEventArgs">The event arguments.</param>
        public void ClearButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            this.viewModel.ClearMessages();
        }

        /// <summary>
        /// Updates the available moderation buttons based on the selected message.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="routedEventArgs">The event arguments.</param>
        public void OnSelectedMessageChanged(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this.InvertedListView.SelectedItem is Message selectedMessage)
            {
                // Hide moderation buttons if the user selected their own message
                if (selectedMessage.MessageSenderName == this.viewModel.Username)
                {
                    this.HideModeratorButtons();
                }
                else
                {
                    this.UpdateAvailableButtonsBasedOnUserStatus();
                }
            }
        }

        /// <summary>
        /// Updates the UI when the user's status changes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void HandleStatusChanged(object sender, EventArgs eventArgs)
        {
            this.UpdateAvailableButtonsBasedOnUserStatus();
        }

        /// <summary>
        /// Connects to the server after waiting for the UI to initialize.
        /// </summary>
        private async void ConnectToServer()
        {
            // Wait for XamlRoot to be available (needed for error dialogs)
            while (this.Content.XamlRoot == null)
            {
                await Task.Delay(ChatConstants.CONNECTION_CHECK_DELAY_MS);
            }

            this.viewModel.ConnectToServer();
        }

        /// <summary>
        /// Handles window closing by disconnecting from the server.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The event arguments.</param>
        public void HandleWindowClosed(object sender, WindowEventArgs args)
        {
            this.viewModel.DisconnectAndCloseWindow();
        }

        /// <summary>
        /// Displays an error dialog for exceptions.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="exceptionEventArgs">The event arguments containing the exception.</param>
        private async void HandleException(object sender, ExceptionEventArgs exceptionEventArgs)
        {
            // Create error dialog
            ContentDialog errorDialog = new ContentDialog()
            {
                Title = "Request rejected!",
                Content = exceptionEventArgs.Exception.Message,
                CloseButtonText = "Ok",
                XamlRoot = this.Content.XamlRoot,
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.ColorHelper.FromArgb(230, 219, 112, 147)),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White),
                CornerRadius = new CornerRadius(8)
            };

            // Set dialog button styles
            errorDialog.Resources["ContentDialogButtonBackground"] =
                new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.ColorHelper.FromArgb(255, 219, 112, 147));

            errorDialog.Resources["ContentDialogButtonForeground"] =
                new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White);

            // Show the dialog
            await errorDialog.ShowAsync();
        }

        /// <summary>
        /// Hides all moderation buttons.
        /// </summary>
        private void HideModeratorButtons()
        {
            this.AdminButton.Visibility = Visibility.Collapsed;
            this.MuteButton.Visibility = Visibility.Collapsed;
            this.KickButton.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Shows buttons available to admin users.
        /// </summary>
        private void ShowAdminButtons()
        {
            this.MuteButton.Visibility = Visibility.Visible;
            this.KickButton.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Shows buttons available to host users.
        /// </summary>
        private void ShowHostButtons()
        {
            this.AdminButton.Visibility = Visibility.Visible;
            this.ShowAdminButtons();
        }

        /// <summary>
        /// Updates the visible buttons based on the user's status.
        /// </summary>
        private void UpdateAvailableButtonsBasedOnUserStatus()
        {
            // Show/hide moderation buttons based on user status
            if (this.viewModel.IsHost)
            {
                this.ShowHostButtons();
            }
            else if (this.viewModel.IsAdmin)
            {
                this.ShowAdminButtons();
            }
            else
            {
                this.HideModeratorButtons();
            }

            // Show/hide send button based on mute status
            this.SendButton.Visibility = this.viewModel.IsMuted
                ? Visibility.Collapsed
                : Visibility.Visible;
        }
    }
}