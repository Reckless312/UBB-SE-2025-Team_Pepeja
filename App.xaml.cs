using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using App1.Repositories;
using App1.Services;
using App1.ViewModels;
using App1.Database;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Steam_Community
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static Window MainWindow { get; private set; }
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            ConfigureServices();
        }

        /// <summary>
        /// Configures the services for dependency injection
        /// </summary>
        private void ConfigureServices()
        {
            // Configuration
            string currentUsername = "JaneSmith"; // This would come from authentication


            // Register database connection
            var dbConnection = new DatabaseConnection();
            _services[typeof(DatabaseConnection)] = dbConnection;

            // Register repositories
            var friendRequestRepository = new FriendRequestRepository(dbConnection);
            _services[typeof(IFriendRequestRepository)] = friendRequestRepository;

            var friendRepository = new FriendRepository(dbConnection);
            _services[typeof(IFriendRepository)] = friendRepository;

            // Register services
            var friendService = new FriendService(friendRepository);
            _services[typeof(IFriendService)] = friendService;

            var friendRequestService = new FriendRequestService(friendRequestRepository, friendService);
            _services[typeof(IFriendRequestService)] = friendRequestService;

            // Register view models
            var friendRequestViewModel = new FriendRequestViewModel(friendRequestService, currentUsername);
            _services[typeof(FriendRequestViewModel)] = friendRequestViewModel;
        }

        /// <summary>
        /// Gets a service of the specified type from the container
        /// </summary>
        /// <typeparam name="T">Type of service to get</typeparam>
        /// <returns>The service instance</returns>
        public static T GetService<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }

            throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered");
        }
        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            MainWindow = m_window;
            m_window.Activate();
        }

        private Window? m_window;
    }
}
