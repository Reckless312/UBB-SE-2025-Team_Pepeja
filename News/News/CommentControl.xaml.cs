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
        
        private Service m_service = Service.Instance;
        public Comment CommentData { get; private set; }
        private Users m_users = Users.Instance;
        
        public CommentControl()
        {
            this.InitializeComponent();
            EditCommentInput.CommentPosted += EditCommentInput_CommentPosted;
        }

        public void SetCommentData(Comment comment)
        {
            CommentData = comment;
            User? user = m_users.GetUserById(CommentData.AuthorId);
        
            UsernameText.Text = user.username;
            CommentDateText.Text = CommentData.CommentDate.ToString("MMM d, yyyy");

            var image = new BitmapImage();
            image.SetSource(new MemoryStream(user.profilePicture).AsRandomAccessStream());
            ProfilePicture.ImageSource = image;

            LoadCommentContent(comment.Content);

            if (user.id == m_service.ActiveUser.id)
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
            // Hide the WebView showing the comment content
            CommentContent.Visibility = Visibility.Collapsed;
            
            // Hide action buttons while editing
            EditButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;
            
            // Configure the edit control
            EditCommentInput.PostId = CommentData.PostId;
            EditCommentInput.CommentId = CommentData.Id;
            EditCommentInput.SetEditMode(true);
            
            // Set the initial text in the editor
            TextBox rawEditor = (TextBox)EditCommentInput.FindName("RawEditor");
            if (rawEditor != null)
            {
                rawEditor.Text = CommentData.Content;
            }
            
            // Show the edit panel
            EditPanel.Visibility = Visibility.Visible;
        }
        
        private void EditCommentInput_CommentPosted(object sender, RoutedEventArgs e)
        {
            // The CommentInputControl has already handled the database update
            
            // Reload the comment content from the database
            // Note: In a real app, you might want to retrieve the updated comment from the database
            
            // Hide the edit panel
            EditPanel.Visibility = Visibility.Collapsed;
            
            // Show the WebView and action buttons again
            CommentContent.Visibility = Visibility.Visible;
            EditButton.Visibility = Visibility.Visible;
            DeleteButton.Visibility = Visibility.Visible;
            
            // Reset the control for next use
            TextBox rawEditor = (TextBox)EditCommentInput.FindName("RawEditor");
            if (rawEditor != null)
            {
                // Update the local comment data with what was saved
                CommentData.Content = m_service.FormatAsPost(rawEditor.Text);
            }
            
            // Reset the CommentInputControl
            EditCommentInput.ResetControl();
            
            // Notify listeners that the comment was updated
            CommentUpdated?.Invoke(this, new RoutedEventArgs());

            LoadCommentContent(CommentData.Content);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            bool success = m_service.DeleteCommentFromDatabase(CommentData.Id);
            if (success)
            {
                CommentDeleted?.Invoke(this, new RoutedEventArgs());
            }
        }
    }
}