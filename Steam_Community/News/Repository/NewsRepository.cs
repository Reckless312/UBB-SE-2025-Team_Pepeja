using News;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Security.Authentication.OnlineId;

namespace News
{
    public class NewsRepository : INewsRepository
    {
        private INewsDatabase databaseConnection;
        public const int PAGE_SIZE = 9;

        public NewsRepository(INewsDatabase? db = null)  // Constructor has parameter for injection
        {
            databaseConnection = db ?? new NewsDatabase();
        }

        /// <summary>
        /// Update a specific post's like count in the database
        /// </summary>
        /// <param name="postId">Post that has to be updated given by its id</param>
        /// <returns>The result of the update (successful or not)</returns>
        /// <exception cref="Exception">Throw an error if the connection or the query execution failed</exception>
        public int UpdatePostLikeCount(int postId)
        {
            try
            {
                databaseConnection.Connect();

                string query = $"UPDATE NewsPosts SET nrLikes = nrLikes + 1 WHERE id = {postId}";

                int executionResult = databaseConnection.ExecuteQuery(query);

                return executionResult;
            }
            catch (Exception ex)
            {
                throw new Exception("Error: Cannot update post's like count: " + ex.Message);
            }
            finally
            {
                databaseConnection.Disconnect();
            }
        }

        /// <summary>
        /// Update a specific post's dislike count in the database
        /// </summary>
        /// <param name="postId">Post that has to be updated given by its id</param>
        /// <returns>The result of the update (successful or not)</returns>
        /// <exception cref="Exception">Throw an error if the connection or the query execution failed</exception>
        public int UpdatePostDislikeCount(int postId)
        {
            try
            {
                databaseConnection.Connect();

                string query = $"UPDATE NewsPosts SET nrDislikes = nrDislikes + 1 WHERE id = {postId}";

                int executionResult = databaseConnection.ExecuteQuery(query);

                return executionResult;
            }
            catch (Exception ex)
            {
                throw new Exception("Error: Cannot update post's dislike count: " + ex.Message);
            }
            finally
            {
                databaseConnection.Disconnect();
            }
        }

        /// <summary>
        /// Save a new rating into the database by a user targeting the given post
        /// </summary>
        /// <param name="postId">Target posts id</param>
        /// <param name="userId">User's id that left the rating</param>
        /// <param name="ratingType">Type of the rating (negative/positive)</param>
        /// <returns>The result of the query execution</returns>
        /// <exception cref="Exception">Throw an error if the connection or the execution failed</exception>
        public int AddRatingToPost(int postId, int userId, int ratingType)
        {
            try
            {
                databaseConnection.Connect();

                string query = $"INSERT INTO Ratings VALUES({postId}, {userId}, {ratingType})";

                int executionResult = databaseConnection.ExecuteQuery(query);

                return executionResult;
            }
            catch (Exception ex)
            {
                throw new Exception("Error: Cannot insert rating: " + ex.Message);
            }
            finally
            {
                databaseConnection.Disconnect();
            }
        }

        /// <summary>
        /// Delete the target post's rating from the database
        /// </summary>
        /// <param name="postId">Target post</param>
        /// <param name="userId">Author of the rating</param>
        /// <returns>The result of the execution</returns>
        /// <exception cref="Exception">Throw an error if the connection or the execution failed</exception>
        public int RemoveRatingFromPost(int postId, int userId)
        {
            try
            {
                databaseConnection.Connect();

                string query = $"DELETE FROM Ratings WHERE postId={postId} AND authorId={userId}";

                int executionResult = databaseConnection.ExecuteQuery(query);

                return executionResult;
            }
            catch (Exception ex)
            {
                throw new Exception("Error: Cannot remove post rating: " + ex.Message);
            }
            finally
            {
                databaseConnection.Disconnect();
            }
        }

        /// <summary>
        /// Add a new comment to the target post and insert it in the database
        /// </summary>
        /// <param name="postId">Target post</param>
        /// <param name="commentContent">Contents of the comment</param>
        /// <param name="userId">Author of the comment</param>
        /// <param name="commentDate">Date the comment is published</param>
        /// <returns>The result of the execution</returns>
        /// <exception cref="Exception">Throw an error if the connection or the execution failed</exception>
        public int AddCommentToPost(int postId, string commentContent, int userId, DateTime commentDate)
        {
            try
            {
                databaseConnection.Connect();

                string query = $"INSERT INTO NewsComments VALUES({userId}, {postId}, N'{commentContent}', '{commentDate}')";

                int executionResult = databaseConnection.ExecuteQuery(query);

                return executionResult;
            }
            catch (Exception ex)
            {
                throw new Exception("Error: Cannot insert new comment to post: " + ex.Message);
            }
            finally
            {
                databaseConnection.Disconnect();
            }
        }

        /// <summary>
        /// Update a target comment in the database
        /// </summary>
        /// <param name="commentId">Target comment</param>
        /// <param name="commentContent">New contents of the comment</param>
        /// <returns>The query execution result</returns>
        /// <exception cref="Exception">Throw an error if the connection or execution failed</exception>
        public int UpdateComment(int commentId, string commentContent)
        {
            try
            {
                databaseConnection.Connect();

                string query = $"UPDATE NewsComments SET content=N'{commentContent}' WHERE id={commentId}";

                int executionResult = databaseConnection.ExecuteQuery(query);

                return executionResult;
            }
            catch (Exception ex)
            {
                throw new Exception("Error: Cannot update comment: " + ex.Message);
            }
            finally
            {
                databaseConnection.Disconnect();
            }
        }

        /// <summary>
        /// Delete a specific comment from the database
        /// </summary>
        /// <param name="commentId">Target comment</param>
        /// <returns>The result of the execution</returns>
        /// <exception cref="Exception">Throw an error if the connection or the execution failed</exception>
        public int DeleteCommentFromDatabase(int commentId)
        {
            try
            {
                databaseConnection.Connect();

                string query = $"DELETE FROM NewsComments WHERE id={commentId}";

                int executionResult = databaseConnection.ExecuteQuery(query);

                return executionResult;
            }
            catch (Exception ex)
            {
                throw new Exception("Error: Cannot delete comment from database: " + ex.Message);
            }
            finally
            {
                databaseConnection.Disconnect();
            }
        }

        /// <summary>
        /// Search for all the comments in the database that has the given target post
        /// </summary>
        /// <param name="postId">Target post</param>
        /// <returns>Read comments in a list</returns>
        /// <exception cref="Exception">Throw an error if anything failed</exception>
        public List<Comment> LoadFollowingComments(int postId)
        {
            try
            {
                databaseConnection.Connect();

                List<Comment> followingComments = new List<Comment>();

                string readQuery = $"""
                SELECT * FROM NewsComments WHERE postId={postId}
                """;

                followingComments = databaseConnection.FetchCommentsData(readQuery);

                return followingComments;
            }
            catch (Exception ex)
            {
                throw new Exception("Error: The rest of the comments could not be loaded: " + ex.Message);
            }
            finally
            {
                databaseConnection.Disconnect();
            }
        }

        /// <summary>
        /// Insert a post into the database
        /// </summary>
        /// <param name="userId">Author of the post</param>
        /// <param name="postContent">Contents of the post</param>
        /// <param name="postDate">Date the post is published</param>
        /// <returns>The result of the execution</returns>
        /// <exception cref="Exception">Throw an error if the connection or the execution failed</exception>
        public int AddPostToDatabase(int userId, string postContent, DateTime postDate)
        {
            try
            {
                databaseConnection.Connect();

                string query = $"INSERT INTO NewsPosts VALUES({userId}, N'{postContent}', '{postDate}', 0, 0, 0)";

                int executionResult = databaseConnection.ExecuteQuery(query);

                return executionResult;
            }
            catch (Exception ex)
            {
                throw new Exception("Error: Cannot add post to database: " + ex.Message);
            }
            finally
            {
                databaseConnection.Disconnect();
            }
        }

        /// <summary>
        /// Update a specific post in the database
        /// </summary>
        /// <param name="postId">Target post</param>
        /// <param name="postContent">New contents of the post</param>
        /// <returns>The result of the execution</returns>
        /// <exception cref="Exception">Throw an error if the connection or execution failed</exception>
        public int UpdatePost(int postId, string postContent)
        {
            try
            {
                databaseConnection.Connect();

                string query = $"UPDATE NewsPosts SET content=N'{postContent}' WHERE id={postId}";

                int executionResult = databaseConnection.ExecuteQuery(query);

                return executionResult;
            }
            catch (Exception ex)
            {
                throw new Exception("Error: Cannot update post: " + ex.Message);
            }
            finally
            {
                databaseConnection.Disconnect();
            }
        }

        /// <summary>
        /// Delete a post from the database
        /// </summary>
        /// <param name="postId">Target post</param>
        /// <returns>The result of the query execution</returns>
        /// <exception cref="Exception">Throw an error if the connection or execution failed</exception>
        public int DeletePostFromDatabase(int postId)
        {
            try
            {
                databaseConnection.Connect();

                string query = $"DELETE FROM NewsPosts WHERE id={postId}";

                int executionResult = databaseConnection.ExecuteQuery(query);

                return executionResult;
            }
            catch (Exception ex)
            {
                throw new Exception("Error: Cannot delete post from database: " + ex.Message);
            }
            finally
            {
                databaseConnection.Disconnect();
            }
        }

        /// <summary>
        /// Search for all the posts in the database matching the searched text
        /// Load all the ratings the current user has on any of the found posts
        /// </summary>
        /// <param name="pageNumber">Page number to calculate page offset</param>
        /// <param name="userId">User id to select ratings related to user</param>
        /// <param name="searchedText">Searched text</param>
        /// <returns>Posts matching the searched text as a list</returns>
        /// <exception cref="Exception">Throw an error if anything failed</exception>
        public List<Post> LoadFollowingPosts(int pageNumber, int userId, string searchedText)
        {
            try
            {
                databaseConnection.Connect();

                List<Post> followingPosts = new List<Post>();

                int offset = (pageNumber - 1) * PAGE_SIZE;

                string readQuery = $"""
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
                        FROM NewsPosts WHERE content LIKE @search
                    ) AS _
                    WHERE RowNum > {offset} AND RowNum <= {offset + PAGE_SIZE}
                    """;

                followingPosts = databaseConnection.FetchPostsData(readQuery, searchedText);

                foreach (var post in followingPosts)
                {
                    string scalarQuery = $"SELECT ratingType FROM Ratings WHERE postId={post.Id} AND authorId={userId}";
                    post.ActiveUserRating = databaseConnection.ExecuteScalar(scalarQuery);
                }

                return followingPosts;
            }
            catch (Exception ex)
            {
                throw new Exception("Error: The rest of the posts could not be loaded: " + ex.Message);
            }
            finally
            {
                databaseConnection.Disconnect();
            }
        }
    }
}
