using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SteamCommunity.Reviews.Models;
using SteamCommunity.Reviews.Repository;
using SteamCommunity.Reviews.Services;
using System;
using System.Collections.Generic;

namespace SteamCommunity.ReviewsTests
{
    [TestClass]
    public class ReviewServiceTests
    {
        private Mock<IReviewRepository> _mockRepo;
        private ReviewService _service;

        [TestInitialize]
        public void SetUp()
        {
            _mockRepo = new Mock<IReviewRepository>();
            _service = new ReviewService(_mockRepo.Object);
        }

        [TestMethod]
        public void SubmitReview_ShouldSetDateAndCallRepository()
        {
            // Arrange
            var review = new Review();
            _mockRepo.Setup(r => r.InsertNewReviewIntoDatabase(review)).Returns(true);

            // Act
            var result = _service.SubmitReview(review);

            // Assert
            Assert.IsTrue(result);
            Assert.AreNotEqual(default, review.DateAndTimeWhenReviewWasCreated);
            _mockRepo.Verify(r => r.InsertNewReviewIntoDatabase(review), Times.Once);
        }

        [TestMethod]
        public void EditReview_ShouldUpdateDateAndCallRepository()
        {
            // Arrange
            var review = new Review();
            _mockRepo.Setup(r => r.UpdateExistingReviewInDatabase(review)).Returns(true);

            // Act
            var result = _service.EditReview(review);

            // Assert
            Assert.IsTrue(result);
            Assert.AreNotEqual(default, review.DateAndTimeWhenReviewWasCreated);
            _mockRepo.Verify(r => r.UpdateExistingReviewInDatabase(review), Times.Once);
        }

        [TestMethod]
        public void DeleteReview_ShouldCallRepository()
        {
            // Arrange
            _mockRepo.Setup(r => r.DeleteReviewFromDatabaseById(5)).Returns(true);

            // Act
            var result = _service.DeleteReview(5);

            // Assert
            Assert.IsTrue(result);
            _mockRepo.Verify(r => r.DeleteReviewFromDatabaseById(5), Times.Once);
        }

        [TestMethod]
        public void GetAllReviewsForAGame_ShouldReturnReviews()
        {
            // Arrange
            var expected = new List<Review> { new Review(), new Review() };
            _mockRepo.Setup(r => r.FetchAllReviewsByGameId(1)).Returns(expected);

            // Act
            var result = _service.GetAllReviewsForAGame(1);

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void GetReviewStatisticsForGame_ShouldReturnRoundedStats()
        {
            // Arrange
            _mockRepo.Setup(r => r.RetrieveReviewStatisticsForGame(1)).Returns((10, 7, 3.678));

            // Act
            var result = _service.GetReviewStatisticsForGame(1);

            // Assert
            Assert.AreEqual(10, result.TotalReviews);
            Assert.AreEqual(70.0, result.PositivePercentage);
            Assert.AreEqual(3.7, result.AverageRating);
        }

        [TestMethod]
        public void SortReviews_ShouldSortByHighestRating()
        {
            // Arrange
            var reviews = new List<Review>
            {
                new Review { NumericRatingGivenByUser = 1 },
                new Review { NumericRatingGivenByUser = 5 }
            };

            // Act
            var result = _service.SortReviews(reviews, "Highest Rating");

            // Assert
            Assert.AreEqual(5, result[0].NumericRatingGivenByUser);
        }

        [TestMethod]
        public void SortReviews_ShouldReturnOriginal_WhenInvalidSort()
        {
            // Arrange
            var reviews = new List<Review> { new Review { ReviewIdentifier = 1 } };

            // Act
            var result = _service.SortReviews(reviews, "Invalid");

            // Assert
            Assert.AreEqual(1, result[0].ReviewIdentifier);
        }

        [TestMethod]
        public void FilterReviewsByRecommendation_ShouldReturnPositiveOnly()
        {
            // Arrange
            var reviews = new List<Review>
            {
                new Review { IsRecommended = true },
                new Review { IsRecommended = false }
            };

            // Act
            var result = _service.FilterReviewsByRecommendation(reviews, "Positive Only");

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0].IsRecommended);
        }

        [TestMethod]
        public void FilterReviewsByRecommendation_ShouldReturnNegativeOnly()
        {
            // Arrange
            var reviews = new List<Review>
            {
                new Review { IsRecommended = true },
                new Review { IsRecommended = false }
            };

            // Act
            var result = _service.FilterReviewsByRecommendation(reviews, "Negative Only");

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.IsFalse(result[0].IsRecommended);
        }

        [TestMethod]
        public void ToggleVote_ShouldCallRepository()
        {
            // Arrange
            _mockRepo.Setup(r => r.ToggleVoteForReview(1, "Helpful", true)).Returns(true);

            // Act
            var result = _service.ToggleVote(1, "Helpful", true);

            // Assert
            Assert.IsTrue(result);
            _mockRepo.Verify(r => r.ToggleVoteForReview(1, "Helpful", true), Times.Once);
        }

        [TestMethod]
        public void UpdateReview_ShouldUpdateIdAndCallEditReview()
        {
            // Arrange
            var review = new Review();
            _mockRepo.Setup(r => r.UpdateExistingReviewInDatabase(review)).Returns(true);

            // Act
            var result = _service.UpdateReview(123, review);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(123, review.ReviewIdentifier);
        }
    }
}
