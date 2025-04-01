using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using DirectMessages;
using System.ComponentModel;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Search
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class SearchWindow : Window
    {
        private Service service;
        private User currentUser;

        public ObservableCollection<User> displayedUsers;
        public ObservableCollection<User> chatInvitesFromUsers;

        private bool isHosting;

        private const int HARDCODED_USER_ID = 1;
        private const String HARDCODED_USER_NAME = "Cora";

        public const String SEND_MESSAGE_REQUEST_CONTENT = "Invite to Chat";
        public const String CANCEL_MESSAGE_REQUEST_CONTENT = "Cancel Invite";

        public SearchWindow()
        {
            this.InitializeComponent();
            
            // Hardcoded, assumed we should know about the current user
            this.currentUser = new User(SearchWindow.HARDCODED_USER_ID, SearchWindow.HARDCODED_USER_NAME, DirectMessages.Service.GET_IP_REPLACER);
            this.displayedUsers = new ObservableCollection<User>();
            this.chatInvitesFromUsers = new ObservableCollection<User>();
            this.service = new Service();

            this.currentUser.IpAddress = this.service.UpdateCurrentUserIpAddress(this.currentUser.Id);
            this.isHosting = false;

            this.FillInvites();

            this.Closed += this.Disconnect;
        }

        public void MessageButton_Click(object sender, RoutedEventArgs e)
        {
            Button? clickedButton = sender as Button;
            User? clickedUser = clickedButton?.Tag as User;

            if (clickedButton == null || clickedUser == null)
            {
                return;
            }

            int receivedCode = this.service.MessageRequest(this.currentUser.Id, clickedUser.Id);

            if (receivedCode == Service.MESSAGE_REQUEST_FOUND)
            {
                clickedButton.Content = SearchWindow.SEND_MESSAGE_REQUEST_CONTENT;
                return;
            }
            else if (receivedCode == Service.MESSAGE_REQUEST_NOT_FOUND)
            {
                clickedButton.Content = SearchWindow.CANCEL_MESSAGE_REQUEST_CONTENT;
            }
            else
            {
                return;
            }

            switch (this.isHosting)
            {
                case true:
                    break;
                case false:
                    DirectMessages.ChatRoomWindow chatRoomWindow = new ChatRoomWindow(this.currentUser.UserName, DirectMessages.Service.HOST_IP_FINDER);
                    chatRoomWindow.WindowClosed += this.StoppedHosting;
                    chatRoomWindow.Activate();
                    this.isHosting = true;
                    break;
            }
        }

        public void RefreshChatInvitesButton_Click(object sender, RoutedEventArgs e)
        {
            this.FillInvites();
        }

        public void FillInvites()
        {
            List<User> inviteSenders = this.service.GetUsersWhoSentMessageRequest(this.currentUser.Id);

            this.chatInvitesFromUsers.Clear();

            foreach (User user in inviteSenders)
            {
                this.chatInvitesFromUsers.Add(user);
            }
        }

        public void DeclineChatInviteButton_Click(object sender, RoutedEventArgs e)
        {
            Button? clickedButton = sender as Button;
            User? clickedUser = clickedButton?.Tag as User;

            if (clickedButton == null || clickedUser == null)
            {
                return;
            }

            this.service.HandleMessageAcceptOrDecline(clickedUser.Id, this.currentUser.Id);
            this.chatInvitesFromUsers.Remove(clickedUser);
        }
        public void AcceptChatInviteButton_Click(object sender, RoutedEventArgs e)
        {
            Button? clickedButton = sender as Button;
            User? clickedUser = clickedButton?.Tag as User;

            if (clickedButton == null || clickedUser == null)
            {
                return;
            }

            DirectMessages.ChatRoomWindow chatRoomWindow = new ChatRoomWindow(this.currentUser.UserName, clickedUser.IpAddress);
            chatRoomWindow.Activate();

            this.service.HandleMessageAcceptOrDecline(clickedUser.Id, this.currentUser.Id);
            this.chatInvitesFromUsers.Remove(clickedUser);
        }

        public void SendFriendRequestButton_Click(object sender, RoutedEventArgs e)
        {

        }

        public void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            String username = this.InputBox.Text;

            List<User> foundUsers = this.service.GetFirst10UsersMatchedSorted(username);

            this.displayedUsers.Clear();

            foreach (User user in foundUsers)
            {
                this.displayedUsers.Add(user);
            }
        }

        private void StoppedHosting(object? sender, bool eventArgs)
        {
            this.isHosting = false;
        }

        public void Disconnect(object sender, WindowEventArgs args)
        {
            this.service.OnCloseWindow(this.currentUser.Id);
        }
    }
}
