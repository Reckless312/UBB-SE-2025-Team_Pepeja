using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Steam_Community.DirectMessages.Models;
using Steam_Community.DirectMessages.Views; 

namespace Search
{
    public partial class SearchControl : UserControl
    {
        private IService service;
        private User currentUser;

        public ObservableCollection<User> DisplayedUsers;
        public ObservableCollection<User> ChatInvitesFromUsers;
        public ObservableCollection<User> FriendRequestsFromUsers;

        private bool isHosting;

        private const int HARDCODED_USER_ID = 1;
        private const string HARDCODED_USER_NAME = "JaneSmith";

        public const string SEND_MESSAGE_REQUEST_CONTENT = "Invite to Chat";
        public const string CANCEL_MESSAGE_REQUEST_CONTENT = "Cancel Invite";

        public event EventHandler<ChatRoomOpenedEventArgs>? ChatRoomOpened;

        public SearchControl()
        {
            this.InitializeComponent();
            // Hardcoded, assumed we should know about the current user

            this.currentUser = new User(SearchControl.HARDCODED_USER_ID, SearchControl.HARDCODED_USER_NAME, ChatConstants.GET_IP_REPLACER);
            this.DisplayedUsers = new ObservableCollection<User>();
            this.ChatInvitesFromUsers = new ObservableCollection<User>();
            this.FriendRequestsFromUsers = new ObservableCollection<User>();

            this.service = new Service();

            this.currentUser.IpAddress = this.service.UpdateCurrentUserIpAddress(this.currentUser.Id);
            this.isHosting = false;

            this.FillInvites();
        }

        // Method that parent can call when closing
        public void OnClosing(object? sender, WindowEventArgs e)
        {
            this.service.OnCloseWindow(this.currentUser.Id);
        }

        public void SortAscendingButton_Click(object sender, RoutedEventArgs routedEvents)
        {
            List<User> sortedUsers = this.service.SortAscending(this.DisplayedUsers.ToList());
            this.DisplayedUsers.Clear();
            foreach (User user in sortedUsers)
            {
                this.DisplayedUsers.Add(user);
            }
            this.CheckForDisplayingNoUsersFound();
        }

        public void SortDescendingButton_Click(object sender, RoutedEventArgs routedEvents)
        {
            List<User> sortedUsers = this.service.SortDescending(this.DisplayedUsers.ToList());
            this.DisplayedUsers.Clear();
            foreach (User user in sortedUsers)
            {
                this.DisplayedUsers.Add(user);
            }
            this.CheckForDisplayingNoUsersFound();
        }

        public void MessageButton_Click(object sender, RoutedEventArgs routedEvents)
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
                clickedButton.Content = SearchControl.SEND_MESSAGE_REQUEST_CONTENT;
                return;
            }
            else if (receivedCode == Service.MESSAGE_REQUEST_NOT_FOUND)
            {
                clickedButton.Content = SearchControl.CANCEL_MESSAGE_REQUEST_CONTENT;
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
                    ChatRoomOpened?.Invoke(this, new ChatRoomOpenedEventArgs
                    {
                        Username = this.currentUser.UserName,
                        IpAddress = ChatConstants.GET_IP_REPLACER
                    });
                    this.isHosting = true;
                    break;
            }
        }

        public void RefreshChatInvitesButton_Click(object sender, RoutedEventArgs routedEvents)
        {
            this.FillInvites();
        }

        public void FillInvites()
        {
            List<User> inviteSenders = this.service.GetUsersWhoSentMessageRequest(this.currentUser.Id);

            this.ChatInvitesFromUsers.Clear();

            foreach (User user in inviteSenders)
            {
                this.ChatInvitesFromUsers.Add(user);
            }
        }

        public void DeclineChatInviteButton_Click(object sender, RoutedEventArgs routedEvents)
        {
            Button? clickedButton = sender as Button;
            User? clickedUser = clickedButton?.Tag as User;

            if (clickedButton == null || clickedUser == null)
            {
                return;
            }

            this.service.HandleMessageAcceptOrDecline(clickedUser.Id, this.currentUser.Id);
            this.ChatInvitesFromUsers.Remove(clickedUser);
        }
        public void AcceptChatInviteButton_Click(object sender, RoutedEventArgs routedEvents)
        {
            Button? clickedButton = sender as Button;
            User? clickedUser = clickedButton?.Tag as User;

            if (clickedButton == null || clickedUser == null)
            {
                return;
            }

            ChatRoomOpened?.Invoke(this, new ChatRoomOpenedEventArgs
            {
                Username = this.currentUser.UserName,
                IpAddress = clickedUser.IpAddress
            });

            this.service.HandleMessageAcceptOrDecline(clickedUser.Id, this.currentUser.Id);
            this.ChatInvitesFromUsers.Remove(clickedUser);
        }

        public void SendFriendRequestButton_Click(object sender, RoutedEventArgs routedEvents)
        {
            Button? clickedButton = sender as Button;
            if (clickedButton == null)
            {
                return;
            }
            // Get the user associated with this button
            User? clickedUser = null;

            // Try to get the user from Tag
            if (clickedButton.Tag is User)
            {
                clickedUser = clickedButton.Tag as User;
            }
            else if (clickedButton.Tag is string)
            {
                // Find the user by username in our displayed users
                string username = clickedButton.Tag.ToString();
                clickedUser = DisplayedUsers.FirstOrDefault(u => u.UserName == username);
            }

            if (clickedUser == null)
            {
                return;
            }


            // Handle different friend status scenarios
            switch (clickedUser.FriendshipStatus)
            {
                case FriendshipStatus.NotFriends:
                    // Send friend request
                    clickedUser.FriendshipStatus = FriendshipStatus.RequestSent;
                    clickedButton.Content = "Cancel Request";
                    break;

                case FriendshipStatus.RequestSent:
                    // Cancel the friend request
                    clickedUser.FriendshipStatus = FriendshipStatus.NotFriends;
                    clickedButton.Content = "Add Friend";
                    break;

                case FriendshipStatus.RequestReceived:
                    // Accept the friend request
                    // This would require additional functionality to be implemented
                    break;

                case FriendshipStatus.Friends:
                    // Already friends - could implement unfriend functionality here
                    break;
            }
            this.service.ToggleFriendRequest(clickedUser.FriendshipStatus, this.currentUser.Id, clickedUser.Id);
        }

        public void AcceptFriendRequestButton_Click(object sender, RoutedEventArgs routedEvents)
        {
            // Implement friend request acceptance
        }

        public void DeclineFriendRequestButton_Click(object sender, RoutedEventArgs routedEvents)
        {
            // Implement friend request declination
        }

        public void SearchButton_Click(object sender, RoutedEventArgs routedEvents)
        {
            string username = this.InputBox.Text;

            List<User> foundUsers = this.service.GetFirst10UsersMatchedSorted(username);

            this.DisplayedUsers.Clear();

            foreach (User user in foundUsers)
            {
                this.DisplayedUsers.Add(user);
            }

            this.CheckForDisplayingNoUsersFound();
        }

        private void CheckForDisplayingNoUsersFound()
        {
            if (this.DisplayedUsers.Count == 0)
            {
                this.NoUsersFoundMessage.Visibility = Visibility.Visible;
            }
            else
            {
                this.NoUsersFoundMessage.Visibility = Visibility.Collapsed;
            }
        }

        public void StoppedHosting(object? sender, WindowEventArgs windowEventArgs)
        {
            this.isHosting = false;
        }
    }

    // Event arguments for when a chat room is opened
    public class ChatRoomOpenedEventArgs : EventArgs
    {
        public string Username { get; set; }
        public string IpAddress { get; set; }
    }
}
