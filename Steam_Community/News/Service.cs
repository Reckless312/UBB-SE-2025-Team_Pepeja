using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace News
{
    public class Service : IService
    {
        private static Service? _instance;

        public static Service Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Service(new NewsDatabase());
                return _instance;
            }
        }

        public const int PAGE_SIZE = 9;
        private readonly INewsDatabase _db;
        public readonly User ActiveUser = Users.Instance.GetUserById(1)!;

        private Service(INewsDatabase db)
        {
            _db = db;
        }

        public static Service Initialize(INewsDatabase db)
        {
            if (_instance != null)
                throw new InvalidOperationException("Service is already initialized.");

            _instance = new Service(db);
            return _instance;
        } 

        // Format text to be posted
        // Returns the text embedded into a predefined html code
        public string FormatAsPost(string text)
        {
            // Recognize html
            SanitizeHtml(ref text);
            ConvertSpecialTagsToHtml(ref text);

            // Format the text
            string parsedText = @"<html><head>
                        <style>
                            body { 
                                font-family: 'Segoe UI', sans-serif; 
                                margin: 0; 
                                padding: 0; 
                                color: #333;
                                white-space: pre-wrap;
                                overflow: scroll; 
                            }
                            body::-webkit-scrollbar {
                                display: none;
                            }
                            img {
                                display: block;
                                margin: 0 auto;
                                max-width: 80%;
                                max-height: 500px;
                            }
                            h2 { 
                                margin-top: 0; 
                                color: #0066cc;
                                font-size: 18px;
                            }
                            .spoiler {
                                background-color: black;
                                user-select: none;
                                color: black;
                                cursor: pointer;
                                padding: 2px 5px;
                                border-radius: 3px;
                                transition: color 0.2s ease-in-out;
                            }
                            .spoiler.revealed {
                                color: white;
                            }
                        </style></head><body>" + text + "</body></html>";
            return parsedText;
        }

        public bool LikePost(int postId)
        {
            using var connection = _db.GetConnection();
            try
            {
                _db.Connect(connection);
                var updateNumberOfLikes = new SqlCommand($"UPDATE NewsPosts SET nrLikes = nrLikes + 1 WHERE id = {postId}", connection);
                var rememberUserRating = new SqlCommand($"INSERT INTO Ratings VALUES({postId}, {ActiveUser.id}, 1)", connection);

                int rowsAffected = updateNumberOfLikes.ExecuteNonQuery() + rememberUserRating.ExecuteNonQuery();
                return rowsAffected == 2;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error liking post: {ex.Message}");
                return false;
            }
            finally
            {
                _db.Disconnect(connection);
            }
        }
        public bool DislikePost(int postId)
        {
            using var connection = _db.GetConnection();
            try
            {
                _db.Connect(connection);
                var updateNumberOfDislikes = new SqlCommand($"UPDATE NewsPosts SET nrDislikes = nrDislikes + 1 WHERE id = {postId}", connection);
                var rememberUserRating = new SqlCommand($"INSERT INTO Ratings VALUES({postId}, {ActiveUser.id}, 0)", connection);

                int rowsAffected = updateNumberOfDislikes.ExecuteNonQuery() + rememberUserRating.ExecuteNonQuery();
                return rowsAffected == 2;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disliking post: {ex.Message}");
                return false;
            }
            finally
            {
                _db.Disconnect(connection);
            }
        }

        public bool RemoveRatingFromPost(int postId)
        {
            using var connection = _db.GetConnection();
            try
            {
                _db.Connect(connection);
                var removeUserRating = new SqlCommand($"DELETE FROM Ratings WHERE postId={postId} AND authorId={ActiveUser.id}", connection);

                int rowsAffected = removeUserRating.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing active user's like from post: {ex.Message}");
                return false;
            }
            finally
            {
                _db.Disconnect(connection);
            }
        }

        public bool SaveComment(int postId, string commentContent)
        {
            using var connection = _db.GetConnection();
            try
            {
                _db.Connect(connection);

                var saveComment = new SqlCommand($"INSERT INTO NewsComments VALUES({ActiveUser.id}, {postId}, @content, '{DateTime.Now}')", connection);
                saveComment.Parameters.AddWithValue("@content", commentContent);

                int rowsAffected = saveComment.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving comment: {ex.Message}");
                return false;
            }
            finally
            {
                _db.Disconnect(connection);
            }
        }

        public bool UpdateComment(int commentId, string commentBody)
        {
            using var connection = _db.GetConnection();
            try
            {
                _db.Connect(connection);
                var updateComment = new SqlCommand($"UPDATE NewsComments SET content=@content WHERE id={commentId}", connection);
                updateComment.Parameters.AddWithValue("@content", commentBody);

                int rowsAffected = updateComment.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating comment: {ex.Message}");
                return false;
            }
            finally
            {
                _db.Disconnect(connection);
            }
        }

        public bool DeleteCommentFromDatabase(int commentId)
        {
            using var connection = _db.GetConnection();
            try
            {
                _db.Connect(connection);
                var deleteComment = new SqlCommand($"DELETE FROM NewsComments WHERE id={commentId}", connection);
                int rowsAffected = deleteComment.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting comment: {ex.Message}");
                return false;
            }
            finally
            {
                _db.Disconnect(connection);
            }
        }

        public List<Comment> LoadNextComments(int postId, int pageNumber)
        {
            using var connection = _db.GetConnection();
            List<Comment> comments = new List<Comment>();
            SqlDataReader reader = null;
            try
            {
                _db.Connect(connection);
                int offset = (pageNumber - 1) * PAGE_SIZE;
                var getComments = new SqlCommand($"""
                SELECT * FROM NewsComments WHERE postId={postId}
                """, connection);

                reader = getComments.ExecuteReader();
                while (reader.Read())
                {
                    comments.Add(new()
                    {
                        Id = (int)reader["id"],
                        PostId = (int)reader["postId"],
                        AuthorId = (int)reader["authorId"],
                        Content = (string)reader["content"],
                        CommentDate = (DateTime)reader["uploadDate"]
                    });
                }
                return comments;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading comments: {ex.Message}");
                return comments;
            }
            finally
            {
                reader?.Close();
                _db.Disconnect(connection);
            }
        }

        public bool SavePostToDatabase(string postBody)
        {
            using var connection = _db.GetConnection();
            try
            {
                _db.Connect(connection);
                var savePost = new SqlCommand($"INSERT INTO NewsPosts VALUES({ActiveUser.id}, @body, '{DateTime.Now}', 0, 0, 0)", connection);
                savePost.Parameters.AddWithValue("@body", postBody);
                int rowsAffected = savePost.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving post: {ex.Message}");
                return false;
            }
            finally
            {
                _db.Disconnect(connection);
            }
        }

        public bool UpdatePost(int postId, string postBody)
        {
            using var connection = _db.GetConnection();
            try
            {
                _db.Connect(connection);
                var updatePost = new SqlCommand($"UPDATE NewsPosts SET content=@content WHERE id={postId}", connection);
                updatePost.Parameters.AddWithValue("@content", postBody);
                int rowsAffected = updatePost.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating post: {ex.Message}");
                return false;
            }
            finally
            {
                _db.Disconnect(connection);
            }
        }

        public bool DeletePostFromDatabase(int postId)
        {
            using var connection = _db.GetConnection();
            try
            {
                _db.Connect(connection);
                var deletePost = new SqlCommand($"DELETE FROM NewsPosts WHERE id={postId}", connection);
                deletePost.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving post: {ex.Message}");
                return false;
            }
            finally
            {
                _db.Disconnect(connection);
            }
            return true;
        }

        public List<Post> LoadNextPosts(string query, int pageNumber)
        {
            using var connection = _db.GetConnection();
            List<Post> posts = new();
            SqlDataReader? reader = null;
            try
            {
                _db.Connect(connection);

                int offset = (pageNumber - 1) * PAGE_SIZE;
                var loadPosts = new SqlCommand($"""
                SELECT 
                    id,
                    authorId,
                    content,
                    uploadDate,
                    nrLikes,
                    nrDislikes,
                    nrComments
                FROM (
                    SELECT 
                        *,
                        ROW_NUMBER() OVER (ORDER BY uploadDate DESC) AS RowNum
                    FROM NewsPosts WHERE content LIKE @query
                ) AS _
                WHERE RowNum > {offset} AND RowNum <= {offset + PAGE_SIZE}
                """, connection);
                loadPosts.Parameters.AddWithValue("@query", query == "" ? "%" : $"%{query}%");

                reader = loadPosts.ExecuteReader();
                while (reader.Read())
                {
                    Post post = new()
                    {
                        Id = (int)reader["id"],
                        AuthorId = (int)reader["authorId"],
                        Content = (string)reader["content"],
                        UploadDate = (DateTime)reader["uploadDate"],
                        NrLikes = (int)reader["nrLikes"],
                        NrDislikes = (int)reader["nrDislikes"],
                        NrComments = (int)reader["nrComments"]
                    };
                    posts.Add(post);
                }
                reader.Close();
                foreach (var post in posts)
                {
                    loadPosts.CommandText = $"SELECT ratingType FROM Ratings WHERE postId={post.Id} AND authorId={ActiveUser.id}";
                    post.ActiveUserRating = (bool?)loadPosts.ExecuteScalar();
                }
                return posts;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading posts: {ex.Message}");
                return posts;
            }
            finally
            {
                reader?.Close();
                _db.Disconnect(connection);
            }
        }

        private void SanitizeHtml(ref string htmlCode)
        {
            string[] ALLOWED_TAGS = { "h1", "/h1", "h2", "/h2", "h3", "/h3", "b", "/b", "i", "/i", "s", "/s", "sub", "/sub", "sup", "/sup", "spoiler", "/spoiler", "img.*" };
            htmlCode = Regex.Replace(htmlCode, $@"</?(?!({string.Join('|', ALLOWED_TAGS)})\b)[^>]*>", "");
            htmlCode = Regex.Replace(htmlCode, @"<img\s+(?!src=(['""])[^'""]+\1\s*\/?>)[^>]*>", "");
        }

        private void ConvertSpecialTagsToHtml(ref string htmlCode)
        {
            htmlCode = htmlCode.Replace("<spoiler>", "<span class=\"spoiler\" onclick=\"this.classList.toggle('revealed')\">");
            htmlCode = htmlCode.Replace("</spoiler>", "</span>");
            Stack<int> indicesOfUnclosedSpans = new();

            foreach (Match match in Regex.Matches(htmlCode, @"</?((span|/span)\b)[^>]*>"))
            {
                if (match.Value.StartsWith("</"))
                {
                    if (indicesOfUnclosedSpans.Count == 0)
                    {
                        htmlCode = htmlCode.Remove(match.Index, match.Value.Length);
                    }
                    else
                    {
                        indicesOfUnclosedSpans.Pop();
                    }
                }
                else
                {
                    indicesOfUnclosedSpans.Push(match.Index);
                }
            }

            while (indicesOfUnclosedSpans.Count > 0)
            {
                int matchIndex = indicesOfUnclosedSpans.Pop();
                htmlCode = htmlCode.Remove(matchIndex, "<span class=\"spoiler\" onclick=\"this.classList.toggle('revealed')\">".Length);
            }
        }
    }
}
