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

        private static ForumRepository _instance = new ForumRepository(new DatabaseConnection());

        public static ForumRepository Instance {  get { return _instance; } }
        private ForumRepository(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        private string TimeSpanFilterToString(TimeSpanFilter filter)
        {
            switch (filter)
            {
                case TimeSpanFilter.Day:
                    return "DAY";
                case TimeSpanFilter.Week:
                    return "WEEK";
                case TimeSpanFilter.Month:
                    return "MONTH";
                case TimeSpanFilter.Year:
                    return "YEAR";
                default:
                    return "";
            }
        }

        public List<Post> GetTopPosts(TimeSpanFilter filter)
        {
            string query;
            switch (filter)
            {
                case TimeSpanFilter.AllTime:
                    query = "SELECT TOP 20 * FROM Posts ORDER BY score DESC";
                    break;
                default:
                    query = $"SELECT TOP 20 * FROM Posts WHERE creation_date >= DATEADD({TimeSpanFilterToString(filter)}, -1, GETDATE()) ORDER BY score DESC";
                    break;
            }

            DataSet dataSet = _dbConnection.ExecuteQuery(query, "Posts");
            List<Post> posts = new();
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                Post post = new()
                {
                    Id = Convert.ToUInt32(row["post_id"]),
                    Title = Convert.ToString(row["title"]),
                    Body = Convert.ToString(row["body"]),
                    Score = Convert.ToInt32(row["score"]),
                    TimeStamp = Convert.ToString(row["creation_date"]),
                    AuthorId = Convert.ToUInt32(row["author_id"]),
                    GameId = row.IsNull("game_id") ? null : Convert.ToUInt32(row["game_id"]),
                };

                posts.Add(post);
            }
            return posts;
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
            return Convert.ToInt32(score);
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

#nullable enable
        public List<Post> GetPagedPosts(uint pageNumber, uint pageSize, bool positiveScoreOnly = false, uint? gameId = null, string? filter = null)
        {
            string query = $"SELECT * FROM Posts ORDER BY creation_date OFFSET {pageNumber * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY WHERE 1 = 1";

            if (gameId != null)
                query += $" AND game_id = {gameId}";

            if (positiveScoreOnly)
                query += " AND score >= 0";

            if (!string.IsNullOrEmpty(filter))
                query += $" AND title LIKE '%{filter}%'";

            DataSet dataSet = _dbConnection.ExecuteQuery(query, "Posts");
            List<Post> posts = new();
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                Post post = new()
                {
                    Id = Convert.ToUInt32(row["post_id"]),
                    Title = Convert.ToString(row["title"]),
                    Body = Convert.ToString(row["body"]),
                    Score = Convert.ToInt32(row["score"]),
                    TimeStamp = Convert.ToString(row["creation_date"]),
                    AuthorId = Convert.ToUInt32(row["author_id"]),
                    GameId = row.IsNull("game_id") ? null : Convert.ToUInt32(row["game_id"]),
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
                    Id = Convert.ToUInt32(row["comment_id"]),
                    Body = Convert.ToString(row["body"]),
                    Score = Convert.ToInt32(row["score"]),
                    TimeStamp = Convert.ToString(row["creation_date"]),
                    AuthorId = Convert.ToUInt32(row["author_id"])
                };
                comments.Add(comment);
            }

            return comments;
        }
    }
}