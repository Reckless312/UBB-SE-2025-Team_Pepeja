using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SteamCommunity.Reviews.Views;
using SteamCommunity.Reviews.ViewModels;
using System.Diagnostics;
using System;
using System.IO;
using SteamCommunity.Reviews.Repository;

namespace SteamCommunity.Reviews
{
    public partial class App : Application
    {
        private Window? m_window;

        public App()
        {
            this.InitializeComponent();
        }


        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                m_window = new Window();


                var reviewView = new ReviewView
                {
                    DataContext = new ReviewViewModel()
                };

                m_window.Content = reviewView;
                m_window.Activate();

                if (reviewView.DataContext is ReviewViewModel vm)
                {
                    try
                    {
                        vm.LoadReviewsForGame(1);  // CRASH POINT
                    }
                    catch (Exception ex)
                    {
                        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                        File.WriteAllText(Path.Combine(desktop, "LoadReviews_error.txt"), ex.ToString());
                        throw;
                    }
                }
            }
            catch (Exception outerEx)
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                File.WriteAllText(Path.Combine(desktop, "AppStartup_error.txt"), outerEx.ToString());
                throw;
            }
        }
    }
}