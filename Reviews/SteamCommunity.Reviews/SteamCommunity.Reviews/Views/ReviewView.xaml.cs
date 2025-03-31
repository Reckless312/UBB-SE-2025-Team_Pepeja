using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SteamCommunity.Reviews.ViewModels;
using System;

namespace SteamCommunity.Reviews.Views
{
    public sealed partial class ReviewView : Page
    {
        public ReviewView()

        {

            if (DataContext is ReviewViewModel vm)
            {
                vm.OnValidationFailed = ShowValidationMessage;
            }

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
            if (DataContext is ReviewViewModel vm)
            {
                vm.SubmitNewReview();
                ReviewPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void OnSortChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e?.AddedItems.Count > 0 &&
                e.AddedItems[0] is ComboBoxItem { Content: string sortOption } &&
                !string.IsNullOrWhiteSpace(sortOption) &&
                DataContext is ReviewViewModel vm)
            {
                vm.ApplySortinOption(sortOption);
            }
        }

        private void OnFilterChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e?.AddedItems.Count > 0 &&
                e.AddedItems[0] is ComboBoxItem { Content: string filter } &&
                !string.IsNullOrWhiteSpace(filter) &&
                DataContext is ReviewViewModel vm)
            {
                vm.ApplyReccomendationFilter(filter);
            }
        }
     

        private void OnEditReviewClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn &&
                btn.DataContext is Models.Review review &&
                DataContext is ReviewViewModel vm)
            {
                vm.EditAReview(review);

                // Ensure ReviewPanel is visible
                if (ReviewPanel != null)
                    ReviewPanel.Visibility = Visibility.Visible;
            }
        }


        private void OnDeleteReviewClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn &&
                btn.Tag is int reviewId &&
                DataContext is ReviewViewModel vm)
            {
                vm.DeleteSelectedReview(reviewId);
            }
        }

        private void OnVoteHelpfulClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn &&
                btn.DataContext is Models.Review review &&
                DataContext is ReviewViewModel vm)
            {
                vm.ToggleVoteForReview(review.ReviewIdentifier, "Helpful", review);
            }
        }

        private void OnVoteFunnyClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn &&
                btn.DataContext is Models.Review review &&
                DataContext is ReviewViewModel vm)
            {
                vm.ToggleVoteForReview(review.ReviewIdentifier, "Funny", review);
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
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }








    }


}
