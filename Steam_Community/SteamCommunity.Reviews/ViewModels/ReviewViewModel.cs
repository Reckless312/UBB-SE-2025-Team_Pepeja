

using SteamCommunity.Reviews.Models;
using SteamCommunity.Reviews.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.IO;

namespace SteamCommunity.Reviews.ViewModels
{
    public partial class ReviewViewModel : INotifyPropertyChanged
    {
        private readonly ReviewService _reviewService;
        private int _currentGameIdentifier;
        public int CurrentGameId => _currentGameIdentifier;

        public Action<string>? OnValidationFailed;

        private const int CurrentUserId = 1;
        private bool IsEditingReview = false;
        private int? EditingReviewId = null;

        public ObservableCollection<Review> CollectionOfGameReviews { get; set; } = new();
        public Review ReviewCurrentlyBeingWritten { get; set; } = new();

        public string CurrentSortOption { get; set; } = "Newest First";
        public string CurrentRecommendationFilter { get; set; } = "All Reviews";

        private int _totalReviews;
        public int TotalNumberOfReviews
        {
            get => _totalReviews;
            set { _totalReviews = value; OnPropertyChanged(); }
        }

        private double _positiveReviewPercentage;
        public double PercentageOfPositiveReviews
        {
            get => _positiveReviewPercentage;
            set { _positiveReviewPercentage = value; OnPropertyChanged(); }
        }

        private double _averageRatingScore;
        public double AverageRatingAcrossAllReviews
        {
            get => _averageRatingScore;
            set { _averageRatingScore = value; OnPropertyChanged(); }
        }

        public ReviewViewModel()
        {
            _reviewService = new ReviewService();
        }

        public void LoadReviewsForGame(int gameIdentifier)
        {
            try
            {
                _currentGameIdentifier = gameIdentifier;

                var reviews = _reviewService.GetAllReviewsForAGame(gameIdentifier);
                reviews = _reviewService.FilterReviewsByRecommendation(reviews, CurrentRecommendationFilter);
                reviews = _reviewService.SortReviews(reviews, CurrentSortOption);

                CollectionOfGameReviews.Clear();
                foreach (var review in reviews)
                    CollectionOfGameReviews.Add(review);

                UpdateReviewStatistics();
            }
            catch (Exception ex)
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string path = Path.Combine(desktop, "load_reviews_internal_error.txt");
                File.WriteAllText(path, ex.ToString());
                throw;
            }
        }

        public void SubmitNewReview()
        {
            // Validation
            if (string.IsNullOrWhiteSpace(ReviewCurrentlyBeingWritten.ReviewContentText) ||
                ReviewCurrentlyBeingWritten.NumericRatingGivenByUser <= 0)
            {
                // Send signal to UI to show a validation message
                OnValidationFailed?.Invoke("Please fill in the required fields: Review Content and Rating.");
                return;
            }

            ReviewCurrentlyBeingWritten.GameIdentifier = _currentGameIdentifier;
            ReviewCurrentlyBeingWritten.UserIdentifier = CurrentUserId;
            ReviewCurrentlyBeingWritten.DateAndTimeWhenReviewWasCreated = DateTime.Now;

            bool success;
            if (IsEditingReview && EditingReviewId.HasValue)
            {
                ReviewCurrentlyBeingWritten.ReviewIdentifier = EditingReviewId.Value;
                success = _reviewService.EditReview(ReviewCurrentlyBeingWritten);
            }
            else
            {
                success = _reviewService.SubmitReview(ReviewCurrentlyBeingWritten);
            }

            if (success)
            {
                ReviewCurrentlyBeingWritten = new Review();
                OnPropertyChanged(nameof(ReviewCurrentlyBeingWritten));

                IsEditingReview = false;
                EditingReviewId = null;
                LoadReviewsForGame(_currentGameIdentifier);
            }
        }

        public void EditAReview(Review review)
        {
            if (review.UserIdentifier != CurrentUserId) return;

            IsEditingReview = true;
            EditingReviewId = review.ReviewIdentifier;
            ReviewCurrentlyBeingWritten = new Review
            {
                ReviewIdentifier = review.ReviewIdentifier,
                ReviewTitleText = review.ReviewTitleText,
                ReviewContentText = review.ReviewContentText,
                NumericRatingGivenByUser = review.NumericRatingGivenByUser,
                IsRecommended = review.IsRecommended,
                GameIdentifier = review.GameIdentifier,
                UserIdentifier = review.UserIdentifier,
                TitleOfGame = review.ReviewTitleText
            };

            OnPropertyChanged(nameof(ReviewCurrentlyBeingWritten));
        }

        public void DeleteSelectedReview(int reviewIdentifier)
        {
            if (_reviewService.DeleteReview(reviewIdentifier))
                LoadReviewsForGame(_currentGameIdentifier);
        }

        public void ToggleVoteForReview(int reviewId, string voteType, Review review)
        {
            bool shouldIncrement;

            if (voteType == "Helpful")
            {
                shouldIncrement = !review.HasVotedHelpful;
                review.HasVotedHelpful = !review.HasVotedHelpful;
            }
            else if (voteType == "Funny")
            {
                shouldIncrement = !review.HasVotedFunny;
                review.HasVotedFunny = !review.HasVotedFunny;
            }
            else return;

            _reviewService.ToggleVote(reviewId, voteType, shouldIncrement);

            if (voteType == "Helpful")
                review.TotalHelpfulVotesReceived += shouldIncrement ? 1 : -1;
            else if (voteType == "Funny")
                review.TotalFunnyVotesReceived += shouldIncrement ? 1 : -1;

            OnPropertyChanged(nameof(CollectionOfGameReviews));
        }

        public void ApplyReccomendationFilter(string filter)
        {
            CurrentRecommendationFilter = filter;
            LoadReviewsForGame(_currentGameIdentifier);
        }

        public void ApplySortinOption(string sortOption)
        {
            CurrentSortOption = sortOption;
            LoadReviewsForGame(_currentGameIdentifier);
        }

        private void UpdateReviewStatistics()
        {
            var (TotalReviews, PositivePercentage, AverageRating) = _reviewService.GetReviewStatisticsForGame(_currentGameIdentifier);
            TotalNumberOfReviews = TotalReviews;
            PercentageOfPositiveReviews = PositivePercentage;
            AverageRatingAcrossAllReviews = AverageRating;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}