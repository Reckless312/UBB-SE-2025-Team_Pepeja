using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;


namespace DirectMessages
{
    public sealed partial class ChatRoomWindow : Window
    {
        private IService service;
        private ObservableCollection<Message> messages;
        private ObservableCollection<Message> Messages
        {
            get => this.messages;
        }

        public ChatRoomWindow(String userName, String ipAddress, String serverIp)
        {
            this.InitializeComponent();

            this.service = new Service(userName, ipAddress, serverIp, DispatcherQueue);

            this.messages = new ObservableCollection<Message>();

            this.service.NewMessage += (message, eventArgs) =>
            {
                this.messages.Add(eventArgs.Message);
            };
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

        public void Send_Button_Click(object sender, RoutedEventArgs e)
        {
            this.service.SendMessage(this.MessageTextBox.Text);
            this.MessageTextBox.Text = "";
        }
    }
}
