using Forum_Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forum_Lib
{
    public interface IForumService
    {
        //at the moment, implementations of IForumService must contain a static field
        //that holds the initialised service
        public static abstract IForumService GetForumServiceInstance();
        public uint GetCurrentUserId();
        public List<ForumPost> GetPagedPosts(uint pageNumber, uint pageSize, bool positiveScoreOnly = false, uint? gameId = null, string? filter = null);
        public List<ForumPost> GetTopPosts(TimeSpanFilter filter);
        public void VoteOnPost(uint postId, int voteValue);
        public void VoteOnComment(uint commentId, int voteValue);
        public List<ForumComment> GetComments(uint postId);
        public void DeleteComment(uint commentId);
        public void CreateComment(string body, uint postId, string date);
        public void DeletePost(uint postId);
        public void CreatePost(string title, string body, string date, uint? gameId);
    }
}
