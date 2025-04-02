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
    public class Service
    {
        private static Service? m_instance;

        public static Service Instance
        {
            get
            {
                if (m_instance == null)
                    m_instance = new Service();
                return m_instance;
            }
        }

        public const int PAGE_SIZE = 9;
        private readonly SqlConnection m_connection;
        public readonly User ActiveUser = Users.Instance.GetUserById(1)!;

        private Service()
        {
            m_connection = new SqlConnection("Data Source=DESKTOP-2OA983C;Initial Catalog=Community;Integrated Security=true;");
        }

        public string FormatAsPost(string text)
        {
            SanitizeHtml(ref text);
            ConvertSpecialTagsToHtml(ref text);
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
            try
            {
                m_connection.Open();
                var command1 = new SqlCommand($"UPDATE NewsPosts SET nrLikes = nrLikes + 1 WHERE id = {postId}", m_connection);
                var command2 = new SqlCommand($"INSERT INTO Ratings VALUES({postId}, {ActiveUser.id}, 1)", m_connection);

                int rowsAffected = command1.ExecuteNonQuery() + command2.ExecuteNonQuery();
                return rowsAffected == 2;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error liking post: {ex.Message}");
                return false;
            }
            finally
            {
                m_connection.Close();
            }
        }
        public bool DislikePost(int postId)
        {
            try
            {
                m_connection.Open();
                var command1 = new SqlCommand($"UPDATE NewsPosts SET nrDislikes = nrDislikes + 1 WHERE id = {postId}", m_connection);
                var command2 = new SqlCommand($"INSERT INTO Ratings VALUES({postId}, {ActiveUser.id}, 0)", m_connection);

                int rowsAffected = command1.ExecuteNonQuery() + command2.ExecuteNonQuery();
                return rowsAffected == 2;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disliking post: {ex.Message}");
                return false;
            }
            finally
            {
                m_connection.Close();
            }
        }

        public bool RemoveRatingFromPost(int postId)
        {
            try
            {
                m_connection.Open();
                var command = new SqlCommand($"DELETE FROM Ratings WHERE postId={postId} AND authorId={ActiveUser.id}", m_connection);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing active user's like from post: {ex.Message}");
                return false;
            }
            finally
            {
                m_connection.Close();
            }
        }

        public bool SaveComment(int postId, string commentContent)
        {
            try
            {
                m_connection.Open();

                var command = new SqlCommand($"INSERT INTO NewsComments VALUES({ActiveUser.id}, {postId}, @content, '{DateTime.Now}')", m_connection);
                command.Parameters.AddWithValue("@content", commentContent);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving comment: {ex.Message}");
                return false;
            }
            finally
            {
                m_connection.Close();
            }
        }

        public bool UpdateComment(int commentId, string commentBody)
        {
            try
            {
                m_connection.Open();
                var command = new SqlCommand($"UPDATE NewsComments SET content=@content WHERE id={commentId}", m_connection);
                command.Parameters.AddWithValue("@content", commentBody);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating comment: {ex.Message}");
                return false;
            }
            finally
            {
                m_connection.Close();
            }
        }

        public bool DeleteCommentFromDatabase(int commentId)
        {
            try
            {
                m_connection.Open();
                var command = new SqlCommand($"DELETE FROM NewsComments WHERE id={commentId}", m_connection);
                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting comment: {ex.Message}");
                return false;
            }
            finally
            {
                m_connection.Close();
            }
        }

        public List<Comment> LoadNextComments(int postId, int pageNumber)
        {
            List<Comment> result = new();
            SqlDataReader? reader = null;

            try
            {
                m_connection.Open();
                int offset = (pageNumber - 1) * PAGE_SIZE;
                var command = new SqlCommand($"""
                SELECT * FROM NewsComments WHERE postId={postId}
                """, m_connection);

                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(new()
                    {
                        Id = (int)reader["id"],
                        PostId = (int)reader["postId"],
                        AuthorId = (int)reader["authorId"],
                        Content = (string)reader["content"],
                        CommentDate = (DateTime)reader["uploadDate"]
                    });
                }
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading comments: {ex.Message}");
                return result;
            }
            finally
            {
                reader?.Close();
                m_connection.Close();
            }
        }

        public bool SavePostToDatabase(string postBody)
        {
            try
            {
                m_connection.Open();
                var command = new SqlCommand($"INSERT INTO NewsPosts VALUES({ActiveUser.id}, @body, '{DateTime.Now}', 0, 0, 0)", m_connection);
                command.Parameters.AddWithValue("@body", postBody);
                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving post: {ex.Message}");
                return false;
            }
            finally
            {
                m_connection.Close();
            }
        }

        public bool UpdatePost(int postId, string postBody)
        {
            try
            {
                m_connection.Open();
                var command = new SqlCommand($"UPDATE NewsPosts SET content=@content WHERE id={postId}", m_connection);
                command.Parameters.AddWithValue("@content", postBody);
                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating post: {ex.Message}");
                return false;
            }
            finally
            {
                m_connection.Close();
            }
        }

        public bool DeletePostFromDatabase(int postId) 
        {
            try
            {
                m_connection.Open();
                var command = new SqlCommand($"DELETE FROM NewsPosts WHERE id={postId}", m_connection);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving post: {ex.Message}");
                return false;
            }
            finally
            {
                m_connection.Close();
            }
            return true;
        }

        public List<Post> LoadNextPosts(string query, int pageNumber)
        {
            List<Post> result = new();
            SqlDataReader? reader = null;
            try
            {
                m_connection.Open();

                int offset = (pageNumber - 1) * PAGE_SIZE;
                var command = new SqlCommand($"""
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
                """, m_connection);
                command.Parameters.AddWithValue("@query", query == "" ? "%" : $"%{query}%");

                reader = command.ExecuteReader();
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
                    result.Add(post);
                }
                reader.Close();
                foreach (var post in result)
                {
                    command.CommandText = $"SELECT ratingType FROM Ratings WHERE postId={post.Id} AND authorId={ActiveUser.id}";
                    post.ActiveUserRating = (bool?)command.ExecuteScalar();
                }
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading posts: {ex.Message}");
                return result;
            }
            finally
            {
                reader?.Close();
                m_connection.Close();
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
