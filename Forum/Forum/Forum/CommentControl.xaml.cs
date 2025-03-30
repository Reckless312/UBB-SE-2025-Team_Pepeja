using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Forum_Lib;

namespace Forum
{
    public sealed partial class CommentControl : UserControl
    {
        // The currently logged-in user ID
        private static readonly uint CurrentUserId = 2; // Hardcoded to JaneSmith for demo
        
        // The comment being displayed
        private CommentDisplay _comment;
        
        // Event for when delete button is clicked
        public event EventHandler<uint> DeleteRequested;
        
        public CommentControl()
        {
            this.InitializeComponent();
        }
        
        // Set up the control with comment data
        public void SetComment(CommentDisplay comment)
        {
            _comment = comment;
            
            // Set up UI elements
            BodyTextBlock.Text = comment.Body;
            UsernameTextBlock.Text = comment.Username;
            TimeStampTextBlock.Text = comment.TimeStamp;
            
            // Set profile image
            ProfileImage.Source = new BitmapImage(new Uri(comment.ProfilePicturePath));
            
            // Show delete button if this is the current user's comment
            DeleteButton.Visibility = (comment.AuthorId == CurrentUserId) ? Visibility.Visible : Visibility.Collapsed;
            
            // Set tag for delete button
            DeleteButton.Tag = comment.Id;
        }
        
        // Handle delete button click
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Trigger the DeleteRequested event
            DeleteRequested?.Invoke(this, _comment.Id);
        }
    }
} 