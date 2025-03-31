using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Forum_Lib;

namespace Forum
{
    public sealed partial class PostDetailDialog : ContentDialog
    {
        // Hard-coded current user ID for demo
        private readonly uint _currentUserId = ForumService.Instance.GetCurrentUserId(); // Using JaneSmith as the current user
        
        // The forum post to display
        private ForumPost _post;
        private PostDisplay _postDisplay;
        
        // Comments list
        private List<CommentControl> _commentControls = new List<CommentControl>();
        
        // Event that's fired when the post is deleted
        public event EventHandler PostDeleted;
        
        // Indicates if any changes were made (comments added, post voted on, etc.)
        public bool ChangesWereMade { get; private set; }
        
        public PostDetailDialog(ForumPost post)
        {
            this.InitializeComponent();
            _post = post;
            _postDisplay = PostDisplay.FromPost(post);
            
            // Set post information
            TitleTextBlock.Text = _post.Title;
            PostBodyTextBlock.Text = _post.Body;
            ScoreTextBlock.Text = _post.Score.ToString();
            
            // Set user information
            User author = User.GetUserById(_post.AuthorId);
            UsernameTextBlock.Text = author.Username;
            PostDateTextBlock.Text = $"Posted on {_post.TimeStamp}";
            ProfileImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(author.ProfilePicturePath));
            
            // Show delete button if this is the current user's post
            DeleteButton.Visibility = (_post.AuthorId == _currentUserId) ? Visibility.Visible : Visibility.Collapsed;
            
            // Register event handlers
            this.Loaded += PostDetailDialog_Loaded;
        }
        
        private void PostDetailDialog_Loaded(object sender, RoutedEventArgs e)
        {
            // Load comments when the dialog is loaded
            LoadComments();
        }
        
        private void LoadComments()
        {
            try
            {
                // Get comments for this post
                List<ForumComment> postComments = ForumService.Instance.GetComments(_post.Id);
                
                // Clear existing comments
                CommentsPanel.Children.Clear();
                _commentControls.Clear();
                
                // If no comments, show the "no comments" message
                if (postComments.Count == 0)
                {
                    NoCommentsText.Visibility = Visibility.Visible;
                    return;
                }
                else
                {
                    NoCommentsText.Visibility = Visibility.Collapsed;
                }
                
                // Create and add comment controls for each comment
                foreach (var comment in postComments)
                {
                    // Create the comment display object
                    var commentDisplay = CommentDisplay.FromComment(comment);
                    
                    // Create a new comment control
                    var commentControl = new CommentControl();
                    commentControl.SetComment(commentDisplay);
                    
                    // Subscribe to events
                    commentControl.DeleteRequested += CommentControl_DeleteRequested;
                    commentControl.CommentVoted += CommentControl_CommentVoted;
                    
                    // Add to the panel
                    CommentsPanel.Children.Add(commentControl);
                    
                    // Keep track of the control
                    _commentControls.Add(commentControl);
                }
            }
            catch (Exception ex)
            {
                // Handle errors loading comments
                System.Diagnostics.Debug.WriteLine($"Error loading comments: {ex.Message}");
            }
        }
        
        private void CommentControl_DeleteRequested(object sender, uint commentId)
        {
            try
            {
                // Delete the comment
                ForumService.Instance.DeleteComment(commentId);
                
                // Reload comments
                LoadComments();
                
                // Indicate changes were made
                ChangesWereMade = true;
            }
            catch (Exception ex)
            {
                // Handle delete error
                System.Diagnostics.Debug.WriteLine($"Error deleting comment: {ex.Message}");
            }
        }
        
        private void CommentControl_CommentVoted(object sender, uint commentId)
        {
            // Mark that changes were made
            ChangesWereMade = true;
        }
        
        private void UpvoteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Call the service with a positive vote value (1)
                ForumService.Instance.VoteOnPost(_post.Id, 1);
                
                // Update score display
                _post.Score += 1;
                ScoreTextBlock.Text = _post.Score.ToString();
                
                // Indicate changes were made
                ChangesWereMade = true;
            }
            catch (Exception ex)
            {
                // Handle voting error
                System.Diagnostics.Debug.WriteLine($"Error upvoting: {ex.Message}");
            }
        }
        
        private void DownvoteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Call the service with a negative vote value (-1)
                ForumService.Instance.VoteOnPost(_post.Id, -1);
                
                // Update score display
                _post.Score -= 1;
                ScoreTextBlock.Text = _post.Score.ToString();
                
                // Indicate changes were made
                ChangesWereMade = true;
            }
            catch (Exception ex)
            {
                // Handle voting error
                System.Diagnostics.Debug.WriteLine($"Error downvoting: {ex.Message}");
            }
        }
        
        private async void AddCommentButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Temporarily hide this dialog to avoid nesting issues
                this.Hide();
                
                // Create and show the add comment dialog
                AddCommentDialog dialog = new AddCommentDialog(_post.Id);
                
                // Set XamlRoot for proper display (using main window XamlRoot)
                if (App.MainWindow != null && App.MainWindow.Content != null)
                {
                    dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
                    
                    // Show the dialog
                    ContentDialogResult result = await dialog.ShowAsync();
                    
                    // If a comment was added, we'll need to reload
                    bool commentAdded = dialog.CommentCreated;
                    
                    // Show this dialog again with the proper XamlRoot
                    this.XamlRoot = App.MainWindow.Content.XamlRoot;
                    await this.ShowAsync();
                    
                    // If a comment was added, reload comments
                    if (commentAdded)
                    {
                        LoadComments();
                        ChangesWereMade = true;
                    }
                }
                else
                {
                    // If we can't get the XamlRoot, just show this dialog again
                    await this.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                // Handle error showing dialog
                System.Diagnostics.Debug.WriteLine($"Error adding comment: {ex.Message}");
                
                // Make sure we show this dialog again in case of error
                try
                {
                    await this.ShowAsync();
                }
                catch { }
            }
        }
        
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Delete the post
                ForumService.Instance.DeletePost(_post.Id);
                
                // Notify that the post was deleted
                PostDeleted?.Invoke(this, EventArgs.Empty);
                
                // Close the dialog
                this.Hide();
            }
            catch (Exception ex)
            {
                // Handle delete error
                System.Diagnostics.Debug.WriteLine($"Error deleting post: {ex.Message}");
            }
        }
    }
} 