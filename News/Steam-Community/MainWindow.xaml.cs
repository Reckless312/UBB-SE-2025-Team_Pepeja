using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Steam_Community
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            // Add post previews to grid
            CreatePostPreviews();
        }

        private void CreatePostPreviews()
        {
            // Clear existing content
            PostsGrid.Children.Clear();

            // Create 9 post previews (3x3 grid)
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    var postPreview = new PostPreviewControl();

                    Grid.SetRow(postPreview, row);
                    Grid.SetColumn(postPreview, col);

                    PostsGrid.Children.Add(postPreview);
                }
            }
        }
    }
}
