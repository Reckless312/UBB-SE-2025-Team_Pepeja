using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forum_Lib
{
    internal class ForumService
    {
        private ForumRepository _repository;
        
        private static ForumService _instance = new ForumService(ForumRepository.Instance);

        public static ForumService Instance { get { return _instance; } }

        private ForumService(ForumRepository repository)
        {
            _repository = repository;
        }
#nullable enable
        public List<Post> GetPagedPosts(uint pageNumber, uint pageSize, bool positiveScoreOnly = false, uint? gameId = null, string? filter = null)
        {
            return _repository.GetPagedPosts(pageNumber, pageSize, positiveScoreOnly, gameId, filter);
        }

        public List<Post> GetTopPosts(TimeSpanFilter filter)
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

        public List<Comment> GetComments(uint postId)
        {
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

    internal enum TimeSpanFilter
    {
        Day,
        Week,
        Month,
        Year,
        AllTime
    }
}