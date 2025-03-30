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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App1
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public ProfileViewModel ProfileViewModel { get; }
        public FriendRequestViewModel FriendRequestViewModel { get; }

        public MainWindow()
        {
            // Get view models from DI container
            FriendRequestViewModel = App.GetService<FriendRequestViewModel>();
            
            // Create profile view model (it will use FriendRequestViewModel internally)
            ProfileViewModel = new ProfileViewModel();
            
            InitializeComponent();

            // Set window size
            SetWindowSize(1000, 700);
        }

        private void SetWindowSize(int width, int height)
        {
            // Set window size - note that in WinUI 3, we need to use AppWindow
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            // Resize the window
            appWindow.Resize(new Windows.Graphics.SizeInt32(width, height));
        }
    }
}
