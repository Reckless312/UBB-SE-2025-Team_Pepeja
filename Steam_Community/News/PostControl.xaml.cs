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
        public event RoutedEventHandler? PanelClosed;
        public event RoutedEventHandler? PostDeleted;
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
            LoadComments();

            if (PostData != null)
            {
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

            bool isDeveloper = m_service.ActiveUser.isDeveloper;
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

            CommentsPanel.Children.Clear();

            List<Comment> comments = m_service.LoadNextComments(PostData.Id, 1);

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
            switch (PostData.ActiveUserRating)
            {
                case PostRatingType.LIKE:
                    m_service.RemoveRatingFromPost(PostData.Id);
                    PostData.NrLikes -= 1;
                    LikesCount.Text = PostData.NrLikes.ToString();
                    PostData.ActiveUserRating = null;
                    LikeButton.Background = new SolidColorBrush(Color.FromArgb(255, 255, 194, 217));
                    break;

                case PostRatingType.DISLIKE:
                    m_service.RemoveRatingFromPost(PostData.Id);
                    m_service.LikePost(PostData.Id);
                    PostData.NrLikes += 1;
                    PostData.NrDislikes -= 1;
                    PostData.ActiveUserRating = PostRatingType.LIKE;
                    LikesCount.Text = PostData.NrLikes.ToString();
                    DislikesCount.Text = PostData.NrDislikes.ToString();
                    LikeButton.Background = new SolidColorBrush(Color.FromArgb(150, 100, 7, 41));
                    DislikeButton.Background = new SolidColorBrush(Color.FromArgb(255, 255, 194, 217));
                    break;

                default:
                    m_service.LikePost(PostData.Id);
                    PostData.NrLikes += 1;
                    LikesCount.Text = PostData.NrLikes.ToString();
                    PostData.ActiveUserRating = PostRatingType.LIKE;
                    LikeButton.Background = new SolidColorBrush(Color.FromArgb(150, 100, 7, 41));
                    break;
            }
        }

        private void DislikeButton_Click(object sender, RoutedEventArgs e)
        {
            switch (PostData.ActiveUserRating)
            {
                case PostRatingType.LIKE:
                    m_service.RemoveRatingFromPost(PostData.Id);
                    m_service.DislikePost(PostData.Id);
                    PostData.NrLikes -= 1;
                    PostData.NrDislikes += 1;
                    PostData.ActiveUserRating = PostRatingType.DISLIKE;
                    LikesCount.Text = PostData.NrLikes.ToString();
                    DislikesCount.Text = PostData.NrDislikes.ToString();
                    LikeButton.Background = new SolidColorBrush(Color.FromArgb(255, 255, 194, 217));
                    DislikeButton.Background = new SolidColorBrush(Color.FromArgb(150, 100, 7, 41));
                    break;

                case PostRatingType.DISLIKE:
                    m_service.RemoveRatingFromPost(PostData.Id);
                    PostData.NrDislikes -= 1;
                    DislikesCount.Text = PostData.NrDislikes.ToString();
                    PostData.ActiveUserRating = null;
                    DislikeButton.Background = new SolidColorBrush(Color.FromArgb(255, 255, 194, 217));
                    break;

                default:
                    m_service.DislikePost(PostData.Id);
                    PostData.NrDislikes += 1;
                    DislikesCount.Text = PostData.NrDislikes.ToString();
                    PostData.ActiveUserRating = PostRatingType.DISLIKE;
                    DislikeButton.Background = new SolidColorBrush(Color.FromArgb(150, 100, 7, 41));
                    break;
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