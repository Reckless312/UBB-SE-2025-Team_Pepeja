using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forum_Lib
{
    public class ForumService
    {
        private ForumRepository _repository;
        
        private static ForumService _instance = new ForumService(ForumRepository.Instance);

        public static ForumService Instance { get { return _instance; } }

        private ForumService(ForumRepository repository)
        {
            _repository = repository;
        }
#nullable enable
        public List<ForumPost> GetPagedPosts(uint pageNumber, uint pageSize, bool positiveScoreOnly = false, uint? gameId = null, string? filter = null)
        {
            //List<ForumPost> forumPosts = new List<ForumPost>();
            //Random random = new Random();

            //for (uint i = pageNumber * pageSize + 1; i <= pageSize * pageNumber + pageSize; i++)
            //{
            //    forumPosts.Add(new ForumPost
            //    {
            //        Id = i,
            //        Title = $"Sample Title {i}",
            //        Body = $"This is a sample body text for post {i}.",
            //        Score = random.Next(-10, 100),
            //        TimeStamp = DateTime.UtcNow.AddMinutes(-i * 10).ToString("yyyy-MM-dd HH:mm:ss"),
            //        AuthorId = (uint)random.Next(1, 10),
            //        GameId = (random.Next(0, 2) == 1) ? (uint?)random.Next(1, 20) : null
            //    });
            //}

            //return forumPosts;
            return _repository.GetPagedPosts(pageNumber, pageSize, positiveScoreOnly, gameId, filter);
        }

        public List<ForumPost> GetTopPosts(TimeSpanFilter filter)
        {
            return _repository.GetTopPosts(filter);
        }

        public void VoteOnPost(uint postId, int voteValue)
        {
            _repository.VoteOnPost(postId, voteValue);
        }

        public void VoteOnComment(uint commentId, int voteValue)
        {
            _repository.VoteOnComment(commentId, voteValue);
        }

        public List<ForumComment> GetComments(uint postId)
        {
            //List<ForumComment> forumComments = new List<ForumComment>();
            //Random random = new Random();

            //for (uint i = 1; i <= 30; i++)
            //{
            //    forumComments.Add(new ForumComment
            //    {
            //        Id = i,
            //        Body = $"This is a sample comment body {i}.",
            //        Score = random.Next(-5, 50),
            //        TimeStamp = DateTime.UtcNow.AddMinutes(-i * 5).ToString("yyyy-MM-dd HH:mm:ss"),
            //        AuthorId = (uint)random.Next(1, 10)
            //    });
            //}
            //return forumComments;
            return _repository.GetComments(postId);
        }

        public void DeleteComment(uint commentId)
        {
            _repository.DeleteComment(commentId);
        }

        public void CreateComment(string body, uint postId, string date, uint authorId)
        {
            _repository.CreateComment(body, postId, date, authorId);
        }

        public void DeletePost(uint postId)
        {
            _repository.DeletePost(postId);
        }

        public void CreatePost(string title, string body, uint authorId, string date, uint? gameId)
        {
            _repository.CreatePost(title, body, authorId, date, gameId);
        }
    }

    public enum TimeSpanFilter
    {
        Day,
        Week,
        Month,
        Year,
        AllTime
    }
}