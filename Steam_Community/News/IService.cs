using System.Collections.Generic;

namespace News
{
    public interface IService
    {
        static abstract Service Instance { get; }

        bool DeleteCommentFromDatabase(int commentId);
        bool DeletePostFromDatabase(int postId);
        bool DislikePost(int postId);
        string FormatAsPost(string text);
        bool LikePost(int postId);
        List<Comment> LoadNextComments(int postId, int pageNumber);
        List<Post> LoadNextPosts(string query, int pageNumber);
        bool RemoveRatingFromPost(int postId);
        bool SaveComment(int postId, string commentContent);
        bool SavePostToDatabase(string postBody);
        bool UpdateComment(int commentId, string commentBody);
        bool UpdatePost(int postId, string postBody);
    }
}