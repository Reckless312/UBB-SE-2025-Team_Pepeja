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
        private static readonly uint _currentUserId = ForumService.GetForumServiceInstance().GetCurrentUserId();
        
        // The comment being displayed
        private CommentDisplay _comment;
        
        // Events for user interactions
        public event EventHandler<uint> DeleteRequested;
        public event EventHandler<uint> CommentVoted;
        
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
            ScoreTextBlock.Text = comment.Score.ToString();
            
            // Set profile image
            ProfileImage.Source = new BitmapImage(new Uri(comment.ProfilePicturePath));
            
            // Show delete button if this is the current user's comment
            DeleteButton.Visibility = (comment.AuthorId == _currentUserId) ? Visibility.Visible : Visibility.Collapsed;
            
            // Set tag for delete button
            DeleteButton.Tag = comment.Id;
        }
        
        // Update the score display
        public void UpdateScore(int newScore)
        {
            ScoreTextBlock.Text = newScore.ToString();
        }
        
        // Handle delete button click
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Trigger the DeleteRequested event
            DeleteRequested?.Invoke(this, _comment.Id);
        }
        
        // Handle upvote button click
        private void UpvoteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Call the service to upvote the comment
                ForumService.GetForumServiceInstance().VoteOnComment(_comment.Id, 1);
                
                // Update the score locally
                _comment.Comment.Score += 1;
                ScoreTextBlock.Text = _comment.Comment.Score.ToString();
                
                // Notify of vote
                CommentVoted?.Invoke(this, _comment.Id);
            }
            catch (Exception ex)
            {
                // Handle error
                System.Diagnostics.Debug.WriteLine($"Error upvoting comment: {ex.Message}");
            }
        }
        
        // Handle downvote button click
        private void DownvoteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Call the service to downvote the comment
                ForumService.GetForumServiceInstance().VoteOnComment(_comment.Id, -1);
                
                // Update the score locally
                _comment.Comment.Score -= 1;
                ScoreTextBlock.Text = _comment.Comment.Score.ToString();
                
                // Notify of vote
                CommentVoted?.Invoke(this, _comment.Id);
            }
            catch (Exception ex)
            {
                // Handle error
                System.Diagnostics.Debug.WriteLine($"Error downvoting comment: {ex.Message}");
            }
        }
    }
} 