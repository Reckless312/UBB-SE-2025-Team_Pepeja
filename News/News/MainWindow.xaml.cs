using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace News
{
    public sealed partial class MainWindow : Window
    {
        private List<Post> m_currentPosts = new List<Post>();
        private int m_currentPage = 0;
        private Service m_service = Service.Instance;
        private bool m_IsLoadingPosts = false;
        private string m_searchQuery = "";

        public MainWindow()
        {
            this.InitializeComponent();
          
            CreatePostButton.Visibility = m_service.ActiveUser.bIsDeveloper ? Visibility.Visible : Visibility.Collapsed;
            
            LoadPosts();

            PostControl.PanelClosed += PostDetailPanel_PanelClosed;
            PostControl.PostDeleted += PostControl_PostDeleted;
            PostControl.PostEditRequested += PostControl_PostEditRequested;
            PostEditorPanel.PostUploaded += PostEditorPanel_PostUploaded;

        }

        private void PostControl_PostEditRequested(object sender, Post post)
        {
            PostEditorPanel.SetPostToEdit(post);
            EditorOverlayContainer.Visibility = Visibility.Visible;
        }

        private void PostControl_PostDeleted(object sender, RoutedEventArgs e)
        {
            LoadPosts(true);
            m_searchQuery = "";
        }

        private void PostEditorPanel_PostUploaded(object sender, RoutedEventArgs e)
        {
            EditorOverlayContainer.Visibility = Visibility.Collapsed;
            LoadPosts(true);
            m_searchQuery = "";
        }

        private void PostDetailPanel_PanelClosed(object sender, RoutedEventArgs e)
        {
            OverlayContainer.Visibility = Visibility.Collapsed;
        }

        private void OverlayBackground_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            PostDetailPanel_PanelClosed(sender, new RoutedEventArgs());
            e.Handled = true;
        }

        private void PostsScroller_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!m_IsLoadingPosts && PostsScroller.VerticalOffset >= PostsScroller.ScrollableHeight - 50)
            {
                m_IsLoadingPosts = true;
                LoadPosts(false, m_searchQuery);
                m_IsLoadingPosts = false;
            }
        }

        private void PostPreview_PostClicked(object sender, RoutedEventArgs e)
        {
            if (sender is PostPreviewControl postPreviewControl && postPreviewControl.PostData != null)
            {
                PostControl.SetPostData(postPreviewControl.PostData);
            }
            OverlayContainer.Visibility = Visibility.Visible;
        }

        private void CreatePostButton_Click(object sender, RoutedEventArgs e)
        {
            EditorOverlayContainer.Visibility = Visibility.Visible;
        }

        private void SearchBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                m_searchQuery = SearchBox.Text.Trim();
                
                LoadPosts(true, m_searchQuery);
            }
        }

        private void LoadPosts(bool resetGrid = false, string searchQuery = "")
        {
            if (resetGrid)
            {
                PostsGrid.Children.Clear();
                m_currentPosts.Clear();
                m_currentPage = 0;
            }

            ++m_currentPage;
            List<Post> posts = m_service.LoadNextPosts(searchQuery, m_currentPage);
            m_currentPosts.AddRange(posts);

            
            int requiredRows = (int)Math.Ceiling(m_currentPosts.Count / 3f);
            while (PostsGrid.RowDefinitions.Count < requiredRows)
            {
                PostsGrid.RowDefinitions.Add(new() { Height = GridLength.Auto });
            }

            int startIndex = (m_currentPage - 1) * Service.PAGE_SIZE;
            for (int i = startIndex; i < m_currentPosts.Count; ++i)
            {
                int row = i / 3;
                int column = i % 3;

                var postPreview = new PostPreviewControl();
                postPreview.SetPostData(m_currentPosts[i]);
                postPreview.PostClicked += PostPreview_PostClicked;

                Grid.SetRow(postPreview, row);
                Grid.SetColumn(postPreview, column);
                PostsGrid.Children.Add(postPreview);
            }
        }
    }
}