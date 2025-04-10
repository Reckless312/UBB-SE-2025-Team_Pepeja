using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Forum_Lib;

namespace Forum
{
    public sealed partial class CreatePostDialog : ContentDialog
    {
        // Hard-coded current user ID for demo
        private readonly uint _currentUserId = ForumService.GetForumServiceInstance().GetCurrentUserId();
        private User _currentUser;
        
        // Result indicating if a post was created
        public bool PostCreated { get; private set; }
        
        public CreatePostDialog()
        {
            this.InitializeComponent();
            
            // Get current user and set up user display
            _currentUser = User.GetUserById(_currentUserId);
            UserNameTextBlock.Text = _currentUser.Username;
            UserProfileImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(_currentUser.ProfilePicturePath));
            
            // Register for button click events
            this.PrimaryButtonClick += CreatePostDialog_PrimaryButtonClick;
        }
        
        private void CreatePostDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Defer the close operation so we can validate inputs
            var deferral = args.GetDeferral();
            
            // Validate inputs
            bool isValid = ValidateInputs();
            
            if (!isValid)
            {
                // Prevent dialog from closing
                args.Cancel = true;
                deferral.Complete();
                return;
            }
            
            try
            {
                // Get values from controls
                string title = TitleTextBox.Text.Trim();
                string body = BodyTextBox.Text.Trim();
                
                // Get game ID (if selected)
                uint? gameId = null;
                if (GameComboBox.SelectedIndex > 0) // First option is "No game"
                {
                    if (GameComboBox.SelectedItem is ComboBoxItem selectedItem &&
                        selectedItem.Tag is string tagValue &&
                        uint.TryParse(tagValue, out uint parsedGameId))
                    {
                        gameId = parsedGameId;
                    }
                }
                
                // Create the post
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                ForumService.GetForumServiceInstance().CreatePost(title, body, currentDate, gameId);
                
                // Indicate that a post was created
                PostCreated = true;
            }
            catch (Exception ex)
            {
                // Handle any errors that might occur
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Error Creating Post",
                    Content = $"There was an error creating your post: {ex.Message}",
                    CloseButtonText = "OK"
                };
                
                // Don't wait for this to complete in this method
                _ = errorDialog.ShowAsync();
                
                // Prevent dialog from closing
                args.Cancel = true;
            }
            
            // Complete the deferral
            deferral.Complete();
        }
        
        private bool ValidateInputs()
        {
            bool isValid = true;
            
            // Check title
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                TitleErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                TitleErrorText.Visibility = Visibility.Collapsed;
            }
            
            // Check body
            if (string.IsNullOrWhiteSpace(BodyTextBox.Text))
            {
                BodyErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                BodyErrorText.Visibility = Visibility.Collapsed;
            }
            
            return isValid;
        }
    }
} 