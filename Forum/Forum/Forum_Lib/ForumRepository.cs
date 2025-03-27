using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.AtomPub;

namespace Forum_Lib
{
    internal class ForumRepository
    {
        private DatabaseConnection _dbConnection;

        public ForumRepository(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public void CreatePost(string title, string body, uint authorId, string date, uint? gameId)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("title", title);
            data.Add("body", body);
            data.Add("author_id", authorId);
            data.Add("creation_date", date);
            data.Add("score", 0);
            data.Add("game_id", gameId != null ? gameId : DBNull.Value);
            _dbConnection.ExecuteInsert("Posts", data);
        }

        public void DeletePost(uint postId)
        {
            _dbConnection.ExecuteDelete("Posts", "post_id", postId);
        }

        public void CreateComment(string body, uint postId, string date, uint authorId)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("body", body);
            data.Add("post_id", postId);
            data.Add("creation_date", date);
            data.Add("author_id", authorId);
            data.Add("score", 0);
            _dbConnection.ExecuteInsert("Comments", data);
        }

        public void DeleteComment(uint commentId)
        {
            _dbConnection.ExecuteDelete("Comments", "comment_id", commentId);
        }

        private int getPostScore(uint id)
        {
            string query = $"SELECT score FROM Posts WHERE post_id = {id}";

            DataSet dataSet = _dbConnection.ExecuteQuery(query, "Posts");

            var score = dataSet.Tables[0].Rows[0]["score"];
            return (int)score;
        }

        private int getCommentScore(uint id)
        {
            string query = $"SELECT score FROM Comments WHERE comment_id = {id}";

            DataSet dataSet = _dbConnection.ExecuteQuery(query, "Comments");

            var score = dataSet.Tables[0].Rows[0]["score"];

            return Convert.ToInt32(score);
        }

        public void VoteOnPost(uint postId, int voteValue)
        {
            int newScore = getPostScore(postId) + voteValue;
            _dbConnection.ExecuteUpdate("Posts", "score", "post_id", newScore, postId);
        }

        public void VoteOnComment(uint commentId, int voteValue)
        {
            int newScore = getCommentScore(commentId) + voteValue;
            _dbConnection.ExecuteUpdate("Comments", "score", "comment_id", newScore, commentId);
        }

        public List<Post> GetPagedPosts(uint pageNumber, uint pageSize)
        {
            string query = $"SELECT * FROM Posts ORDER BY creation_date OFFSET {pageNumber * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY";
            DataSet dataSet = _dbConnection.ExecuteQuery(query, "Posts");
            List<Post> posts = new(); // bcz of this
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                Post post = new()
                {
                    Id = (uint)row["post_id"],
                    Title = (string)row["title"],
                    Body = (string)row["body"],
                    Score = (int)row["score"],
                    TimeStamp = (string)row["creation_date"],
                    AuthorId = (uint)row["author_id"],
                    GameId = row.IsNull("game_id") ? null : (uint)row["game_id"],
                };

                posts.Add(post);
            }
            return posts;
        }

        public List<Comment> GetComments(uint postId)
        {
            string query = $"SELECT * FROM Comments ORDER BY creation_date WHERE post_id = {postId}";
            DataSet dataSet = _dbConnection.ExecuteQuery(query, "Comments");
            List<Comment> comments = new();
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                Comment comment = new()
                {
                    Id = (uint)row["comment_id"],
                    Body = (string)row["body"],
                    Score = (int)row["score"],
                    TimeStamp = (string)row["creation_date"],
                    AuthorId = (uint)row["author_id"]
                };
                comments.Add(comment);
            }

            return comments;
        }
    }
}