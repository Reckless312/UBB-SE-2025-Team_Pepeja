using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;
using System;

namespace News
{
    public sealed partial class CommentControl : UserControl
    {
        public event RoutedEventHandler? CommentDeleted;
        public event RoutedEventHandler? CommentUpdated;
        
        private NewsService service;
        public Comment CommentData { get; private set; }

        private Users users = Users.Instance;
        
        public CommentControl()
        {
            this.InitializeComponent();

            service = new NewsService();
            EditCommentInput.CommentPosted += EditCommentInput_CommentPosted;
        }

        public void SetCommentData(Comment comment)
        {
            CommentData = comment;
            User? user = users.GetUserById(CommentData.AuthorId);
        
            UsernameText.Text = user.username;
            CommentDateText.Text = CommentData.CommentDate.ToString("MMM d, yyyy");

            var image = new BitmapImage();
            image.SetSource(new MemoryStream(user.profilePicture).AsRandomAccessStream());
            ProfilePicture.ImageSource = image;

            LoadCommentContent(comment.Content);

            if (user.id == service.activeUser.id)
            {
                EditButton.Visibility = Visibility.Visible;
                DeleteButton.Visibility = Visibility.Visible;
            }
            else
            {
                EditButton.Visibility = Visibility.Collapsed;
                DeleteButton.Visibility = Visibility.Collapsed;
            }
        }

        private async void LoadCommentContent(string content)
        {
            try
            {
                if (CommentContent.CoreWebView2 == null)
                {
                    await CommentContent.EnsureCoreWebView2Async();
                }

                CommentContent.CoreWebView2.NavigateToString(content);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading comment content: {ex.Message}");
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            CommentContent.Visibility = Visibility.Collapsed;
            
            EditButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;
            
            EditCommentInput.PostId = CommentData.PostId;
            EditCommentInput.CommentId = CommentData.CommentId;
            EditCommentInput.SetEditMode(true);
            
            TextBox rawEditor = (TextBox)EditCommentInput.FindName("RawEditor");
            if (rawEditor != null)
            {
                rawEditor.Text = CommentData.Content;
            }
            
            EditPanel.Visibility = Visibility.Visible;
        }
        
        private void EditCommentInput_CommentPosted(object sender, RoutedEventArgs e)
        {

            EditPanel.Visibility = Visibility.Collapsed;
            
            CommentContent.Visibility = Visibility.Visible;
            EditButton.Visibility = Visibility.Visible;
            DeleteButton.Visibility = Visibility.Visible;
            
            TextBox rawEditor = (TextBox)EditCommentInput.FindName("RawEditor");
            if (rawEditor != null)
            {
                CommentData.Content = service.FormatAsPost(rawEditor.Text);
            }
            
            EditCommentInput.ResetControl();
            
            CommentUpdated?.Invoke(this, new RoutedEventArgs());

            LoadCommentContent(CommentData.Content);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            bool success = service.DeleteComment(CommentData.CommentId);
            if (success)
            {
                CommentDeleted?.Invoke(this, new RoutedEventArgs());
            }
        }
    }
}