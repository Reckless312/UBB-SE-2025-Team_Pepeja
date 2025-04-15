using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using News;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Search;
using Steam_Community.DirectMessages.Views;
using Steam_Community.DirectMessages.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Steam_Community
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.searchControl.ChatRoomOpened += HandleChatInvite;
            this.Closed += this.searchControl.OnClosing;
        }

        public void HandleChatInvite(object? sender, ChatRoomOpenedEventArgs e)
        {
            ChatRoomWindow chatRoomWindow = new ChatRoomWindow(e.Username, e.IpAddress);
            if (e.IpAddress == ChatConstants.HOST_IP_FINDER)
            {
                chatRoomWindow.Closed += this.searchControl.StoppedHosting;
            }
            chatRoomWindow.Activate();
        }
    }
}
