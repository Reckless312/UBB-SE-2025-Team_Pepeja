using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Linq;
using Windows.System;

namespace News
{
    public sealed partial class PostEditorControl : UserControl
    {
        private const string EMPTY_STRING = "";

        public event RoutedEventHandler? PostUploaded;
        private NewsService service;
        private Post? m_postBeingEdited = null;
        private bool isEditMode = false;

        public PostEditorControl()
        {
            this.InitializeComponent();

            service = new NewsService();

            this.Loaded += PostEditorControl_Loaded;
        }

        private void RawButton_Click(object sender, RoutedEventArgs e)
        {
            if (RawHtmlEditor.Visibility == Visibility.Visible)
            {
                return;
            }
            HtmlPreview.Visibility = Visibility.Collapsed;
            RawHtmlEditor.Visibility = Visibility.Visible;
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (HtmlPreview.Visibility == Visibility.Visible)
            {
                return;
            }
            HtmlPreview.Visibility = Visibility.Visible;
            RawHtmlEditor.Visibility = Visibility.Collapsed;
            HtmlPreview.CoreWebView2.NavigateToString(service.FormatAsPost(RawHtmlEditor.Text));
        }

        public void SetPostToEdit(Post post)
        {
            isEditMode = true;
            m_postBeingEdited = post;

            string htmlContent = post.Content;

            int startIndex = htmlContent.IndexOf("<body>") + "<body>".Length;
            int endIndex = htmlContent.IndexOf("</body>");
            int bodyLength = endIndex - startIndex;
            string bodyContent = htmlContent.Substring(startIndex, bodyLength);
            RawHtmlEditor.Text = bodyContent;
        }

        public void ResetEditor()
        {
            isEditMode = false;
            m_postBeingEdited = null;
            RawHtmlEditor.Text = EMPTY_STRING;
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            if (isEditMode)
            {
                if (RawHtmlEditor.Text == "")
                {
                    return;
                }
                string html = service.FormatAsPost(RawHtmlEditor.Text);
                service.UpdatePost(m_postBeingEdited.Id, html);
            }
            else
            {
                if (RawHtmlEditor.Text != "")
                {
                    string html = service.FormatAsPost(RawHtmlEditor.Text);
                    service.SavePost(html);
                }
            }
            ResetEditor();
            RawHtmlEditor.Text = EMPTY_STRING;
            PostUploaded?.Invoke(this, new RoutedEventArgs());
        }

        private async void PostEditorControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await HtmlPreview.EnsureCoreWebView2Async();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WebView2 initialization error: {ex.Message}");
            }
            Username.Text = service.activeUser.username;
            CurrentDate.Text = DateTime.Now.ToString("MMM d, yyyy");

            var image = new BitmapImage();
            image.SetSource(new MemoryStream(service.activeUser.profilePicture).AsRandomAccessStream());
            ProfilePicture.ImageSource = image;
        }
    }
}