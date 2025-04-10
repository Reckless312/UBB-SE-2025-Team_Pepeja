using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Forum_Lib;
using System.Linq;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;

namespace Forum
{
    public sealed partial class PostsControl : UserControl
    {
        private ObservableCollection<PostDisplay> _posts;
        private uint _currentPage = 0;
        private uint _pageSize = 10;
        private bool _isLoadingMore = false;
        private bool _hasMorePosts = true;
        private bool _positiveScoreOnly = false;
        private uint? _gameId = null;
        private string _filter = null;
        private TimeSpanFilter _timeSpanFilter = TimeSpanFilter.AllTime;
        private bool _isTopPostsMode = false;
        
        // Event for when a post is selected
        public event EventHandler<ForumPost> PostSelected;
        
        public PostsControl()
        {
            this.InitializeComponent();
            _posts = new ObservableCollection<PostDisplay>();
            PostsListView.ItemsSource = _posts;
            
            // Add handler for when container is generated to set delete button visibility
            PostsListView.ContainerContentChanging += PostsListView_ContainerContentChanging;
        }

        // Load top posts using time filter
        public void LoadTopPosts(TimeSpanFilter filter)
        {
            try
            {
                // Reset state
                _currentPage = 0;
                _isTopPostsMode = true;
                _timeSpanFilter = filter;
                _hasMorePosts = true;
                
                ShowLoading(true);
                
                // Clear existing posts
                _posts.Clear();
                
                // Get posts from repository
                List<ForumPost> forumPosts = ForumService.GetForumServiceInstance().GetTopPosts(filter);
                
                // Update the UI on the UI thread
                this.DispatcherQueue.TryEnqueue(() => {
                    foreach (var post in forumPosts)
                    {
                        _posts.Add(PostDisplay.FromPost(post));
                    }
                    
                    // If we got fewer posts than the page size, there are no more posts to load
                    if (forumPosts.Count < _pageSize)
                    {
                        _hasMorePosts = false;
                    }
                    
                    // Show the no posts message if no posts were found
                    UpdateVisibility();
                    ShowLoading(false);
                });
            }
            catch (Exception ex)
            {
                // Handle errors
                ShowLoading(false);
                // Could show error message here
            }
        }
        
        // Load paged posts with optional filters
        public void LoadPagedPosts(uint pageNumber, uint pageSize, bool positiveScoreOnly = false, uint? gameId = null, string filter = null)
        {
            try
            {
                // Save filter parameters for loading more later
                _currentPage = pageNumber;
                _pageSize = pageSize;
                _positiveScoreOnly = positiveScoreOnly;
                _gameId = gameId;
                _filter = filter;
                _isTopPostsMode = false;
                _hasMorePosts = true;
                
                ShowLoading(true);
                
                // Clear existing posts if loading first page
                if (pageNumber == 0)
                {
                    _posts.Clear();
                }
                
                // Get posts from repository
                List<ForumPost> forumPosts = ForumService.GetForumServiceInstance().GetPagedPosts(pageNumber, pageSize, positiveScoreOnly, gameId, filter);
                
                // Update the UI on the UI thread
                this.DispatcherQueue.TryEnqueue(() => {
                    foreach (var post in forumPosts)
                    {
                        _posts.Add(PostDisplay.FromPost(post));
                    }
                    
                    // If we got fewer posts than the page size, there are no more posts to load
                    if (forumPosts.Count < pageSize)
                    {
                        _hasMorePosts = false;
                    }
                    
                    // Show the no posts message if no posts were found
                    UpdateVisibility();
                    ShowLoading(false);
                });
            }
            catch (Exception ex)
            {
                // Handle errors
                ShowLoading(false);
                // Could show error message here
            }
        }
        
        // Load more posts when user scrolls to the bottom
        private void LoadMorePosts()
        {
            if (_isLoadingMore || !_hasMorePosts)
            {
                return; // Already loading or no more posts to load
            }
            
            try
            {
                _isLoadingMore = true;
                ShowLoadMoreIndicator(true);
                
                // Increment page number
                _currentPage++;
                
                // Get more posts based on the current mode
                List<ForumPost> morePosts;
                
                if (_isTopPostsMode)
                {
                    // In top posts mode, we can't really load "more" top posts
                    // This is just for demonstration, in a real app you might
                    // implement a different strategy for "more" top posts
                    morePosts = new List<ForumPost>();
                    _hasMorePosts = false;
                }
                else
                {
                    // Get next page of posts
                    morePosts = ForumService.GetForumServiceInstance().GetPagedPosts(_currentPage, _pageSize, _positiveScoreOnly, _gameId, _filter);
                    
                    // If we got fewer posts than the page size, there are no more posts to load
                    if (morePosts.Count < _pageSize)
                    {
                        _hasMorePosts = false;
                    }
                }
                
                // Update the UI on the UI thread
                this.DispatcherQueue.TryEnqueue(() => {
                    foreach (var post in morePosts)
                    {
                        _posts.Add(PostDisplay.FromPost(post));
                    }
                    
                    _isLoadingMore = false;
                    ShowLoadMoreIndicator(false);
                });
            }
            catch (Exception ex)
            {
                _isLoadingMore = false;
                ShowLoadMoreIndicator(false);
                // Could show error message here
            }
        }
        
        // ScrollViewer event handler to detect when the user has scrolled to the bottom
        private void PostsScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // Check if we've scrolled near the bottom
            if (PostsScrollViewer.VerticalOffset >= PostsScrollViewer.ScrollableHeight - 100)
            {
                LoadMorePosts();
            }
        }
        
        // Handle post selection
        private void PostsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PostsListView.SelectedItem is PostDisplay postDisplay)
            {
                PostSelected?.Invoke(this, postDisplay.Post);
            }
        }
        
        // Handle upvote button click
        private void UpvoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is uint postId)
            {
                // Call the service with a positive vote value (1)
                ForumService.GetForumServiceInstance().VoteOnPost(postId, 1);
                
                // Refresh the specific post in the list to show updated score
                RefreshPost(postId);
            }
        }
        
        // Handle downvote button click
        private void DownvoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is uint postId)
            {
                // Call the service with a negative vote value (-1)
                ForumService.GetForumServiceInstance().VoteOnPost(postId, -1);
                
                // Refresh the specific post in the list to show updated score
                RefreshPost(postId);
            }
        }
        
        // Handle delete button click
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is uint postId)
            {
                try
                {
                    // Skip the confirmation dialog for now due to XamlRoot issues
                    // Just delete the post directly
                    ForumService.GetForumServiceInstance().DeletePost(postId);
                    
                    // Remove the post from the UI
                    RemovePostFromUI(postId);
                }
                catch (Exception ex)
                {
                    // Just fail silently for now
                }
            }
        }
        
        // Helper method to refresh a specific post
        private void RefreshPost(uint postId)
        {
            // Find the post in the collection
            for (int i = 0; i < _posts.Count; i++)
            {
                if (_posts[i].Id == postId)
                {
                    // In a real application, we would fetch the updated post from the service
                    // Instead of updating the score ourselves, let's get the current score from the service
                    
                    try
                    {
                        // Get the actual updated score from the database
                        // First get a reference to the original ForumPost object
                        ForumPost originalPost = _posts[i].Post;
                        
                        // Create a new instance with the latest score from the service
                        // This could be a dedicated GetPostById method in a real application
                        List<ForumPost> posts;
                        
                        if (_isTopPostsMode)
                        {
                            posts = ForumService.GetForumServiceInstance().GetTopPosts(_timeSpanFilter);
                        }
                        else
                        {
                            posts = ForumService.GetForumServiceInstance().GetPagedPosts(_currentPage, _pageSize, _positiveScoreOnly, _gameId, _filter);
                        }
                        
                        // Find the post with matching ID to get updated score
                        ForumPost updatedPost = posts.FirstOrDefault(p => p.Id == postId);
                        
                        if (updatedPost != null)
                        {
                            // Create a new PostDisplay with the updated post
                            PostDisplay updatedPostDisplay = PostDisplay.FromPost(updatedPost);
                            
                            // Replace the old post with the updated one
                            _posts.RemoveAt(i);
                            _posts.Insert(i, updatedPostDisplay);
                        }
                    }
                    catch (Exception)
                    {
                        // If there's an error getting the updated post, just leave as is
                    }
                    
                    break;
                }
            }
        }
        
        // Helper method to update visibility of controls
        private void UpdateVisibility()
        {
            if (_posts.Count == 0)
            {
                PostsListView.Visibility = Visibility.Collapsed;
                NoPostsMessage.Visibility = Visibility.Visible;
            }
            else
            {
                PostsListView.Visibility = Visibility.Visible;
                NoPostsMessage.Visibility = Visibility.Collapsed;
            }
        }
        
        // Helper method to show/hide loading indicator
        private void ShowLoading(bool isLoading)
        {
            LoadingIndicator.IsActive = isLoading;
            
            if (isLoading)
            {
                PostsListView.Visibility = Visibility.Collapsed;
                NoPostsMessage.Visibility = Visibility.Collapsed;
            }
        }
        
        // Helper method to show/hide load more indicator
        private void ShowLoadMoreIndicator(bool isLoading)
        {
            if (isLoading)
            {
                LoadMoreIndicator.IsActive = true;
                LoadMoreIndicator.Visibility = Visibility.Visible;
            }
            else
            {
                LoadMoreIndicator.IsActive = false;
                LoadMoreIndicator.Visibility = Visibility.Collapsed;
            }
        }
        
        // Helper method to remove a post from the UI
        private void RemovePostFromUI(uint postId)
        {
            for (int i = 0; i < _posts.Count; i++)
            {
                if (_posts[i].Id == postId)
                {
                    _posts.RemoveAt(i);
                    break;
                }
            }
            
            // Update visibility in case we removed the last post
            UpdateVisibility();
        }
        
        // Show brief delete confirmation
        private void ShowDeleteConfirmation()
        {
            // In a real application, you might want to show a temporary message or toast notification
            // For this example, we'll just keep it simple
        }

        // Handle container content changing to set delete button visibility
        private void PostsListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.Item is PostDisplay postDisplay)
            {
                // Find the delete button in the container
                var container = args.ItemContainer;
                var deleteButton = FindChildByName(container, "DeleteButton") as Button;
                
                if (deleteButton != null)
                {
                    // Set visibility based on whether this is the current user's post
                    deleteButton.Visibility = postDisplay.IsCurrentUser ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        // Helper method to find child control by name
        private DependencyObject FindChildByName(DependencyObject parent, string name)
        {
            // Check if current element is what we're looking for
            if (parent is FrameworkElement element && element.Name == name)
            {
                return parent;
            }

            // Get number of children
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            
            // Search children
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                DependencyObject result = FindChildByName(child, name);
                
                if (result != null)
                {
                    return result;
                }
            }
            
            return null;
        }
    }
} 