using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace News
{
    public sealed partial class CommentInputControl : UserControl
    {
        public event RoutedEventHandler? CommentPosted;
        private readonly NewsService service;
        
        private bool isEditMode = false;
        private const string EMPTY_STRING = "";
        private const string POST_COMMENT_STRING = "Post Comment";

        public int PostId { get; set; }

        public int CommentId { get; set; }

        public CommentInputControl()
        {
            this.InitializeComponent();
            service = new NewsService();
            this.Loaded += CommentInputControl_Loaded;
        }

        public void SetEditMode(bool isEdit)
        {
            isEditMode = isEdit;

            PostCommentButton.Content = service.SetStringOnEditMode(isEditMode);
        }
        
        public void ResetControl()
        {
            isEditMode = false;
            RawEditor.Text = EMPTY_STRING;
            PostCommentButton.Content = POST_COMMENT_STRING;
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
                HtmlPreview.CoreWebView2.NavigateToString(service.FormatAsPost(RawEditor.Text));
            }
        }

        private void PostCommentButton_Click(object sender, RoutedEventArgs e)
        {
            if (RawEditor.Text == EMPTY_STRING)
            {
                return;
            }

            bool success = service.SetCommentMethodOnEditMode(isEditMode, CommentId, PostId, RawEditor.Text);

            if (success)
            {
                CommentPosted?.Invoke(this, new RoutedEventArgs());

                RawEditor.Text = EMPTY_STRING;
                isEditMode = false;
                PostCommentButton.Content = POST_COMMENT_STRING;
                RawButton_Click(this, new RoutedEventArgs());
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Failed to post comment.");
            }
        }
    }
}
