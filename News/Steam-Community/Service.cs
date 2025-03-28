using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace Steam_Community
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
            m_connection = new SqlConnection("Data Source=DESKTOP-7DEHML0;Initial Catalog=News;Integrated Security=true;");
        }

        public string FormatAsPost(string text)
        {
            string parsedText = @"<html><head>
                        <style>
                            body { 
                                font-family: 'Segoe UI', sans-serif; 
                                margin: 0; 
                                padding: 0; 
                                color: #333;
                                white-space: pre-wrap;
                            }
                            h2 { 
                                margin-top: 0; 
                                color: #0066cc;
                                font-size: 18px;
                            }
                        </style></head><body>" + text + "</body></html>";
            SanitizeHtml(parsedText);
            ConvertSpecialTagsToHtml(parsedText);
            return parsedText;
        }

        public bool LikePost(int postId)
        {
            try
            {
                m_connection.Open();
                var command = new SqlCommand($"UPDATE Posts SET nrLikes = nrLikes + 1 WHERE id = {postId}", m_connection);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
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
                var command = new SqlCommand($"UPDATE Posts SET nrDislikes = nrDislikes + 1 WHERE id = {postId}", m_connection);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
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

        // TODO: Make this bool and evaluate success status on calls
        public void SavePostToDatabase(string postBody)
        {
            try
            {
                m_connection.Open();
                var command = new SqlCommand($"INSERT INTO Posts VALUES({ActiveUser.id}, @body, '{DateTime.Now}', 0, 0, 0)", m_connection);
                command.Parameters.AddWithValue("@body", postBody);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving post: {ex.Message}");
                throw;
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
                var command = new SqlCommand($"UPDATE Posts SET content=@content WHERE id={postId}", m_connection);
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
                var command = new SqlCommand($"DELETE FROM Posts WHERE id={postId}", m_connection);
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
                    FROM Posts
                ) AS _
                WHERE RowNum > {offset} AND RowNum <= {offset + PAGE_SIZE}
                """, m_connection);

                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(new()
                    {
                        Id = (int)reader["id"],
                        AuthorId = (int)reader["authorId"],
                        Content = (string)reader["content"],
                        UploadDate = (DateTime)reader["uploadDate"],
                        NrLikes = (int)reader["nrLikes"],
                        NrDislikes = (int)reader["nrDislikes"],
                        NrComments = (int)reader["nrComments"]
                    });
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

        // TODO: Implement these 2 bad boys
        private string SanitizeHtml(string htmlCode)
        {
            return htmlCode;
        }

        private string ConvertSpecialTagsToHtml(string htmlCode)
        {
            return htmlCode;
        }
    }
}
