using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace News
{
    public sealed partial class CommentInputControl : UserControl
    {
        // Event for when a new comment is posted
        public event RoutedEventHandler? CommentPosted;
        private readonly Service m_service = Service.Instance;
        
        // Flag to determine if we're in edit mode
        private bool m_isEditMode = false;

        // Post ID this comment will be attached to
        public int PostId { get; set; }

        // Comment ID when in edit mode
        public int CommentId { get; set; }

        public CommentInputControl()
        {
            this.InitializeComponent();
            this.Loaded += CommentInputControl_Loaded;
        }

        // Set edit mode 
        public void SetEditMode(bool isEdit)
        {
            m_isEditMode = isEdit;
            
            // Update button text
            if (m_isEditMode)
            {
                PostCommentButton.Content = "Save";
            }
            else
            {
                PostCommentButton.Content = "Post Comment";
            }
        }
        
        // Reset control state
        public void ResetControl()
        {
            m_isEditMode = false;
            RawEditor.Text = "";
            PostCommentButton.Content = "Post Comment";
        }

        private async void CommentInputControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await HtmlPreview.EnsureCoreWebView2Async();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WebView2 initialization error: {ex.Message}");
            }
        }

        private void RawButton_Click(object sender, RoutedEventArgs e)
        {
            RawEditor.Visibility = Visibility.Visible;
            HtmlPreview.Visibility = Visibility.Collapsed;
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            RawEditor.Visibility = Visibility.Collapsed;
            HtmlPreview.Visibility = Visibility.Visible;

            if (HtmlPreview.CoreWebView2 != null)
            {
                HtmlPreview.CoreWebView2.NavigateToString(m_service.FormatAsPost(RawEditor.Text));
            }
        }

        private void PostCommentButton_Click(object sender, RoutedEventArgs e)
        {
            if (RawEditor.Text == "")
            {
                return;
            }
            
            bool success = false;
            
            if (m_isEditMode)
            {
                // Update existing comment
                success = m_service.UpdateComment(CommentId, m_service.FormatAsPost(RawEditor.Text));
            }
            else
            {
                // Create new comment
                success = m_service.SaveComment(PostId, m_service.FormatAsPost(RawEditor.Text));
            }

            if (success)
            {
                CommentPosted?.Invoke(this, new RoutedEventArgs());

                RawEditor.Text = "";
                m_isEditMode = false; // Reset edit mode
                PostCommentButton.Content = "Post Comment";
                RawButton_Click(this, new RoutedEventArgs());
            }
            else
            {
                // Show error message (in a real app, you might want to use a ContentDialog here)
                System.Diagnostics.Debug.WriteLine("Failed to post comment.");
            }
        }
    }
}
