using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using App1.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace App1
{
    public sealed partial class ProfileControl : UserControl
    {
        public ProfileViewModel ProfileViewModel { get; }
        public FriendRequestViewModel FriendRequestViewModel { get; }

        public ProfileControl()
        {
            // Get view models from DI container
            FriendRequestViewModel = Steam_Community.App.GetService<FriendRequestViewModel>();
            
            // Create profile view model (it will use FriendRequestViewModel internally)
            ProfileViewModel = new ProfileViewModel();
            
            InitializeComponent();
        }
    }
} 