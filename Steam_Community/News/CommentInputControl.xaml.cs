using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace News
{
    public sealed partial class CommentInputControl : UserControl
    {
        public event RoutedEventHandler? CommentPosted;
        private readonly Service m_service = Service.Instance;
        
        private bool m_isEditMode = false;

        public int PostId { get; set; }

        public int CommentId { get; set; }

        public CommentInputControl()
        {
            this.InitializeComponent();
            this.Loaded += CommentInputControl_Loaded;
        }

        public void SetEditMode(bool isEdit)
        {
            m_isEditMode = isEdit;
            
            if (m_isEditMode)
            {
                PostCommentButton.Content = "Save";
            }
            else
            {
                PostCommentButton.Content = "Post Comment";
            }
        }
        
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
                success = m_service.UpdateComment(CommentId, m_service.FormatAsPost(RawEditor.Text));
            }
            else
            {
                success = m_service.SaveComment(PostId, m_service.FormatAsPost(RawEditor.Text));
            }

            if (success)
            {
                CommentPosted?.Invoke(this, new RoutedEventArgs());

                RawEditor.Text = "";
                m_isEditMode = false;
                PostCommentButton.Content = "Post Comment";
                RawButton_Click(this, new RoutedEventArgs());
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Failed to post comment.");
            }
        }
    }
}
