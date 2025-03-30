using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using Windows.System;
using Windows.UI;

namespace News
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

        private void NewCommentInput_CommentPosted(object sender, RoutedEventArgs e)
        {
            // Reload comments to show the newly added comment
            LoadComments();

            // Update the comment count in the UI
            if (PostData != null)
            {
                // In a real app, you'd reload the PostData from the database to get the updated count
                // For now, we'll just increment it
                PostData.NrComments++;
                CommentsCount.Text = PostData.NrComments.ToString();
            }
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

            bool isDeveloper = m_service.ActiveUser.bIsDeveloper;
            EditButton.Visibility = isDeveloper ? Visibility.Visible : Visibility.Collapsed;
            DeleteButton.Visibility = isDeveloper ? Visibility.Visible : Visibility.Collapsed;

            UpdateWebViewContent();

            InitializeComments();

            LoadComments();
        }

        private void InitializeComments()
        {
            // Ensure NewCommentInput is properly connected
            if (NewCommentInput != null && PostData != null)
            {
                NewCommentInput.PostId = PostData.Id;
                NewCommentInput.CommentPosted += NewCommentInput_CommentPosted;
            }
        }

        private void LoadComments()
        {
            if (PostData == null)
                return;

            // Clear existing comments
            CommentsPanel.Children.Clear();

            // Get comments for this post
            List<Comment> comments = m_service.LoadNextComments(PostData.Id, 1);

            // Display all comments
            foreach (var comment in comments)
            {
                var commentControl = new CommentControl();
                commentControl.SetCommentData(comment);
                commentControl.CommentDeleted += CommentControl_CommentDeleted;
                commentControl.CommentUpdated += CommentControl_CommentUpdated;
                CommentsPanel.Children.Add(commentControl);
            }
        }

        private void CommentControl_CommentDeleted(object sender, RoutedEventArgs e)
        {
            LoadComments();
            PostData.NrComments -= 1;
            CommentsCount.Text = PostData.NrComments.ToString();
        }

        private void CommentControl_CommentUpdated(object sender, RoutedEventArgs e)
        {
            // No need to reload all comments or change the count
            // The comment control has already updated its own UI
            System.Diagnostics.Debug.WriteLine("Comment updated successfully");
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