using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Linq;
using Windows.System;

namespace Steam_Community
{
    public sealed partial class PostEditorControl : UserControl
    {
        public event RoutedEventHandler? PostUploaded;
        private Service m_service = Service.Instance;
        private Post? m_postBeingEdited = null;
        private bool m_bIsEditMode = false;

        public PostEditorControl()
        {
            this.InitializeComponent();
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
            HtmlPreview.CoreWebView2.NavigateToString(m_service.FormatAsPost(RawHtmlEditor.Text));
        }

        public void SetPostToEdit(Post post)
        {
            m_bIsEditMode = true;
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
            m_bIsEditMode = false;
            m_postBeingEdited = null;
            RawHtmlEditor.Text = "";
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            if (RawHtmlEditor.Text == "")
            {
                return;
            }
            if (m_bIsEditMode)
            {
                string html = m_service.FormatAsPost(RawHtmlEditor.Text);
                m_service.UpdatePost(m_postBeingEdited.Id, html);
            }
            else
            {
                string html = m_service.FormatAsPost(RawHtmlEditor.Text);
                m_service.SavePostToDatabase(html);
            }
            ResetEditor();
            RawHtmlEditor.Text = "";
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
            Username.Text = m_service.ActiveUser.username;
            CurrentDate.Text = DateTime.Now.ToString("MMM d, yyyy");

            var image = new BitmapImage();
            image.SetSource(new MemoryStream(m_service.ActiveUser.profilePicture).AsRandomAccessStream());
            ProfilePicture.ImageSource = image;
        }
    }
}