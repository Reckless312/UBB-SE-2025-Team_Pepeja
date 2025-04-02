using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SteamCommunity.Reviews.ViewModels;
using System;

namespace SteamCommunity.Reviews.Views
{
    public sealed partial class ReviewView : UserControl
    {
        private readonly ReviewViewModel _reviewViewModel;
        public ReviewView()
        {
            _reviewViewModel = new ReviewViewModel();
            //if (DataContext is ReviewViewModel vm)
            //{
            //    vm.OnValidationFailed = ShowValidationMessage;
            //}
            DataContext = _reviewViewModel;
            _reviewViewModel.OnValidationFailed = ShowValidationMessage;

            InitializeComponent();
        }

        private void OnWriteReviewClicked(object sender, RoutedEventArgs e)
        {
            if (ReviewPanel == null) return;

            ReviewPanel.Visibility = ReviewPanel.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void OnSubmitReviewClicked(object sender, RoutedEventArgs e)
        {
            //if (DataContext is ReviewViewModel vm)
            //{
            //    vm.SubmitNewReview();
            //    ReviewPanel.Visibility = Visibility.Collapsed;
            //}
            _reviewViewModel.SubmitNewReview();
            ReviewPanel.Visibility = Visibility.Collapsed;
        }

        private void OnSortChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e?.AddedItems.Count > 0 &&
                e.AddedItems[0] is ComboBoxItem { Content: string sortOption } &&
                !string.IsNullOrWhiteSpace(sortOption))
            {
                _reviewViewModel.ApplySortinOption(sortOption);
            }
        }

        private void OnFilterChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e?.AddedItems.Count > 0 &&
                e.AddedItems[0] is ComboBoxItem { Content: string filter } &&
                !string.IsNullOrWhiteSpace(filter))
            {
                _reviewViewModel.ApplyReccomendationFilter(filter);
            }
        }

        private void OnEditReviewClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn &&
                btn.DataContext is Models.Review review)
            {
                _reviewViewModel.EditAReview(review);

                // Ensure ReviewPanel is visible
                if (ReviewPanel != null)
                    ReviewPanel.Visibility = Visibility.Visible;
            }
        }

        private void OnDeleteReviewClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn &&
                btn.Tag is int reviewId)
            {
                _reviewViewModel.DeleteSelectedReview(reviewId);
            }
        }

        private void OnVoteHelpfulClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn &&
                btn.DataContext is Models.Review review)
            {
                _reviewViewModel.ToggleVoteForReview(review.ReviewIdentifier, "Helpful", review);
            }
        }

        private void OnVoteFunnyClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn &&
                btn.DataContext is Models.Review review)
            {
                _reviewViewModel.ToggleVoteForReview(review.ReviewIdentifier, "Funny", review);
            }
        }

        public string FormatHoursPlayed(int hours)
        {
            return $"Played {hours} hour{(hours == 1 ? "" : "s")}";
        }

        private async void ShowValidationMessage(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Missing Information",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
