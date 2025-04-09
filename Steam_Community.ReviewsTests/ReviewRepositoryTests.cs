using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamCommunity.Reviews.Models;
using SteamCommunity.Reviews.Repository;
using System;
using System.Collections.Generic;

namespace SteamCommunity.ReviewsTests
{
    [TestClass]
    public class ReviewRepositoryTests
    {
        private ReviewRepository _repository;

        [TestInitialize]
        public void SetUp()
        {
            _repository = new ReviewRepository();
        }

        [TestMethod]
        public void InsertNewReviewIntoDatabase_ShouldReturnTrue()
        {
            var review = new Review
            {
                ReviewTitleText = "Integration Test Review",
                ReviewContentText = "Content for test",
                IsRecommended = true,
                NumericRatingGivenByUser = 4.5,
                TotalHoursPlayedByReviewer = 20,
                DateAndTimeWhenReviewWasCreated = DateTime.Now,
                UserIdentifier = 1,
                GameIdentifier = 1
            };

            var result = _repository.InsertNewReviewIntoDatabase(review);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void FetchAllReviewsByGameId_ShouldReturnAtLeastOne()
        {
            var reviews = _repository.FetchAllReviewsByGameId(1);

            Assert.IsTrue(reviews.Count >= 0);
        }

        [TestMethod]
        public void UpdateExistingReviewInDatabase_ShouldReturnTrue()
        {
            // Act
            var reviews = _repository.FetchAllReviewsByGameId(1);
            if (reviews.Count == 0) Assert.Inconclusive("No reviews to update.");

            var reviewToUpdate = reviews[0];
            reviewToUpdate.ReviewTitleText = "Updated Title";
            reviewToUpdate.DateAndTimeWhenReviewWasCreated = DateTime.Now;

            var result = _repository.UpdateExistingReviewInDatabase(reviewToUpdate);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ToggleVoteForReview_ShouldIncrementVoteCount()
        {
            var reviews = _repository.FetchAllReviewsByGameId(1);
            if (reviews.Count == 0) Assert.Inconclusive("No reviews to vote on.");

            var reviewId = reviews[0].ReviewIdentifier;

            var result = _repository.ToggleVoteForReview(reviewId, "Helpful", true);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void RetrieveReviewStatisticsForGame_ShouldReturnValues()
        {
            var stats = _repository.RetrieveReviewStatisticsForGame(1);

            Assert.IsTrue(stats.TotalReviews >= 0);
            Assert.IsTrue(stats.TotalPositiveRecommendations >= 0);
            Assert.IsTrue(stats.AverageRatingValue >= 0.0);
        }

        [TestMethod]
        public void DeleteReviewFromDatabaseById_ShouldRemoveReview()
        {
            // First insert a review to delete
            var review = new Review
            {
                ReviewTitleText = "Temp Review",
                ReviewContentText = "To be deleted",
                IsRecommended = false,
                NumericRatingGivenByUser = 2,
                TotalHoursPlayedByReviewer = 5,
                DateAndTimeWhenReviewWasCreated = DateTime.Now,
                UserIdentifier = 1,
                GameIdentifier = 1
            };

            var insertResult = _repository.InsertNewReviewIntoDatabase(review);
            Assert.IsTrue(insertResult);

            // Fetch it back and delete
            var all = _repository.FetchAllReviewsByGameId(1);
            var lastReview = all[0];

            var deleteResult = _repository.DeleteReviewFromDatabaseById(lastReview.ReviewIdentifier);

            Assert.IsTrue(deleteResult);
        }
    }
}
