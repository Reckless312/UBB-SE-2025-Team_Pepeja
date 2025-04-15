using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace News
{
    public sealed partial class NewsControl : UserControl
    {
        private const string EMPTY_STRING = "";
        private const int INITIAL_VALUE_ZERO = 0;
        private const int DIVISOR = 3;
        private const Single FLOAT_DIVISOR = 3f;

        private List<Post> currentPosts = new List<Post>();
        private int currentPage = INITIAL_VALUE_ZERO;
        private NewsService service;
        private bool isLoadingPosts = false;
        private string m_searchedText = EMPTY_STRING;

        public NewsControl()
        {
            this.InitializeComponent();

            service = new NewsService();
          
            News_CreatePostButton.Visibility = service.activeUser.bIsDeveloper ? Visibility.Visible : Visibility.Collapsed;
            
            LoadPosts();

            News_PostControl.PanelClosed += PostDetailPanel_PanelClosed;
            News_PostControl.PostDeleted += PostControl_PostDeleted;
            News_PostControl.PostEditRequested += PostControl_PostEditRequested;
            News_PostEditorPanel.PostUploaded += PostEditorPanel_PostUploaded;
        }

        private void PostControl_PostEditRequested(object sender, Post post)
        {
            News_PostEditorPanel.SetPostToEdit(post);
            News_EditorOverlayContainer.Visibility = Visibility.Visible;
        }

        private void PostControl_PostDeleted(object sender, RoutedEventArgs e)
        {
            LoadPosts(true, m_searchedText);
        }

        private void PostEditorPanel_PostUploaded(object sender, RoutedEventArgs e)
        {
            News_EditorOverlayContainer.Visibility = Visibility.Collapsed;
            LoadPosts(true, m_searchedText);
        }

        private void PostDetailPanel_PanelClosed(object sender, RoutedEventArgs e)
        {
            News_OverlayContainer.Visibility = Visibility.Collapsed;
        }

        private void OverlayBackground_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            PostDetailPanel_PanelClosed(sender, new RoutedEventArgs());
            e.Handled = true;
        }

        private void PostsScroller_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!isLoadingPosts && News_PostsScroller.VerticalOffset >= News_PostsScroller.ScrollableHeight - 50)
            {
                isLoadingPosts = true;
                LoadPosts(false, m_searchedText);
                isLoadingPosts = false;
            }
        }

        private void PostPreview_PostClicked(object sender, RoutedEventArgs e)
        {
            if (sender is PostPreviewControl postPreviewControl && postPreviewControl.PostData != null)
            {
                News_PostControl.SetPostData(postPreviewControl.PostData);
            }
            News_OverlayContainer.Visibility = Visibility.Visible;
        }

        private void CreatePostButton_Click(object sender, RoutedEventArgs e)
        {
            News_EditorOverlayContainer.Visibility = Visibility.Visible;
        }

        private void SearchBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                m_searchedText = News_SearchBox.Text.Trim();
                
                LoadPosts(true, m_searchedText);
            }
        }

        private void LoadPosts(bool resetGrid = false, string searchedText = EMPTY_STRING)
        {
            if (resetGrid)
            {
                News_PostsGrid.Children.Clear();
                currentPosts.Clear();
                currentPage = INITIAL_VALUE_ZERO;
            }

            ++currentPage;
            List<Post> posts = service.LoadNextPosts(currentPage, searchedText);
            currentPosts.AddRange(posts);

            
            int requiredRows = (int)Math.Ceiling(currentPosts.Count / FLOAT_DIVISOR);
            while (News_PostsGrid.RowDefinitions.Count < requiredRows)
            {
                News_PostsGrid.RowDefinitions.Add(new() { Height = GridLength.Auto });
            }

            int startIndex = (currentPage - 1) * NewsService.PAGE_SIZE;
            for (int i = startIndex; i < currentPosts.Count; ++i)
            {
                int row = i / DIVISOR;
                int column = i % DIVISOR;

                var postPreview = new PostPreviewControl();
                postPreview.SetPostData(currentPosts[i]);
                postPreview.PostClicked += PostPreview_PostClicked;

                Grid.SetRow(postPreview, row);
                Grid.SetColumn(postPreview, column);
                News_PostsGrid.Children.Add(postPreview);
            }

            m_searchedText = EMPTY_STRING;
        }
    }
} 