using Microsoft.UI.Xaml;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DirectMessages
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new ChatRoomWindow("Cora", "192.168.1.136", "None");
            m_window.Closed += M_window_Closed;
            m_window.Activate();
        }

        private async void M_window_Closed(object sender, WindowEventArgs args)
        {
            if(m_window is ChatRoomWindow chatRoomWindow)
            {
                await chatRoomWindow.DisconnectService();
            }
        }

        private Window? m_window;
    }
}
