using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using Windows.System;
using Windows.UI;

namespace Steam_Community
{
    public sealed partial class PostControl : UserControl
    {
        // Event for when the panel of this control is closed
        public event RoutedEventHandler? PanelClosed;
        // Event for when a post is deleted
        public event RoutedEventHandler? PostDeleted;
        // Event for when post edit is requested
        public event EventHandler<Post>? PostEditRequested;

        public Post PostData { get; private set; }

        private Users m_users = Users.Instance;
        private Service m_service = Service.Instance;

        public PostControl()
        {
            this.InitializeComponent();
            this.Loaded += PostControl_Loaded;
        }

        public void SetPostData(Post post)
        {
            User? user = m_users.GetUserById(post.AuthorId);
            PostData = post;
            Username.Text = user.username;
            UploadDate.Text = post.UploadDate.ToString("MMM d, yyyy");
            LikesCount.Text = post.NrLikes.ToString();
            DislikesCount.Text = post.NrDislikes.ToString();
            CommentsCount.Text = post.NrComments.ToString();

            var image = new BitmapImage();
            image.SetSource(new MemoryStream(user.profilePicture).AsRandomAccessStream());
            ProfilePicture.ImageSource = image;

            UpdateWebViewContent();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            bool success = m_service.DeletePostFromDatabase(PostData.Id);
            if (success)
            {
                PostDeleted?.Invoke(this, e);
            }
            PanelClosed?.Invoke(this, e);
        }

        private void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            bool success = m_service.LikePost(PostData.Id);
            if (success)
            {
                PostData.NrLikes += 1;
                LikesCount.Text = PostData.NrLikes.ToString();
            }
        }

        private void DislikeButton_Click(object sender, RoutedEventArgs e)
        {
            bool success = m_service.DislikePost(PostData.Id);
            if (success)
            {
                PostData.NrDislikes += 1;
                DislikesCount.Text = PostData.NrDislikes.ToString();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            PostEditRequested?.Invoke(this, PostData);
            PanelClosed?.Invoke(this, new RoutedEventArgs());
        }

        private async void PostControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await ContentWebView.EnsureCoreWebView2Async();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WebView2 initialization error: {ex.Message}");
            }
        }

        private void UpdateWebViewContent()
        {
            if (ContentWebView.CoreWebView2 != null)
            {
                ContentWebView.CoreWebView2.NavigateToString(PostData.Content);
            }
        }
    }
}