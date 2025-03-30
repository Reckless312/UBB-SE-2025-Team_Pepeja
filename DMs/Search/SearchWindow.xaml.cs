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
        private User user;

        public ObservableCollection<User> displayedUsers;

        public SearchWindow(int id, string userName, string ipAddress)
        {
            this.InitializeComponent();
            
            // Hardcoded, assumed we should know about the current user
            this.user = new User(id, userName, ipAddress);
            this.displayedUsers = new ObservableCollection<User>();
            this.service = new Service();

            this.service.UpdateCurrentUserIpAddress(this.user.Id);
        }

        public void MessageButton_Click(object sender, RoutedEventArgs e)
        {
            DirectMessages.ChatRoomWindow chatRoomWindow = new ChatRoomWindow(this.user.UserName, this.user.IpAddress, DirectMessages.Service.HOST_IP_FINDER);
            chatRoomWindow.Closed += (sender, args) => { chatRoomWindow.DisconnectService(); };
            chatRoomWindow.Activate();
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

    }
}
