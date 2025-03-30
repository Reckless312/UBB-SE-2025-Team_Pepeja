using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using App1.Database;
using App1.Models;
using Microsoft.Data.SqlClient;

namespace App1.Repositories
{
    public class FriendRequestRepository : IFriendRequestRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public FriendRequestRepository(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        public async Task<IEnumerable<FriendRequest>> GetFriendRequestsAsync(string username)
        {
            var result = new List<FriendRequest>();
            
            // Define the SQL query to retrieve friend requests
            string query = @"
                SELECT SenderUsername, SenderEmail, SenderProfilePhotoPath, RequestDate
                FROM FriendRequests
                WHERE ReceiverUsername = @ReceiverUsername";

            // Create SQL parameters
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@ReceiverUsername", SqlDbType.NVarChar) { Value = username }
            };

            // Execute the query and process the results
            var dataTable = await _dbConnection.ExecuteReaderAsync(query, CommandType.Text, parameters);
            foreach (DataRow row in dataTable.Rows)
            {
                result.Add(new FriendRequest
                {
                    Username = row["SenderUsername"].ToString(),
                    Email = row["SenderEmail"].ToString(),
                    ProfilePhotoPath = row["SenderProfilePhotoPath"].ToString(),
                    RequestDate = Convert.ToDateTime(row["RequestDate"]),
                    ReceiverUsername = username
                });
            }

            return result;
        }

        public async Task<bool> AddFriendRequestAsync(FriendRequest request)
        {
            try
            {
                // Define the SQL query to insert a friend request
                string query = @"
                    INSERT INTO FriendRequests (SenderUsername, SenderEmail, SenderProfilePhotoPath, ReceiverUsername, RequestDate)
                    VALUES (@SenderUsername, @SenderEmail, @SenderProfilePhotoPath, @ReceiverUsername, @RequestDate)";

                // Create SQL parameters
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@SenderUsername", SqlDbType.NVarChar) { Value = request.Username },
                    new SqlParameter("@SenderEmail", SqlDbType.NVarChar) { Value = request.Email },
                    new SqlParameter("@SenderProfilePhotoPath", SqlDbType.NVarChar) { Value = request.ProfilePhotoPath },
                    new SqlParameter("@ReceiverUsername", SqlDbType.NVarChar) { Value = request.ReceiverUsername },
                    new SqlParameter("@RequestDate", SqlDbType.DateTime) { Value = request.RequestDate }
                };

                // Execute the query
                await _dbConnection.ExecuteNonQueryAsync(query, CommandType.Text, parameters);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteFriendRequestAsync(string senderUsername, string receiverUsername)
        {
            try
            {
                // Define the SQL query to delete a friend request
                string query = @"
                    DELETE FROM FriendRequests
                    WHERE SenderUsername = @SenderUsername AND ReceiverUsername = @ReceiverUsername";

                // Create SQL parameters
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@SenderUsername", SqlDbType.NVarChar) { Value = senderUsername },
                    new SqlParameter("@ReceiverUsername", SqlDbType.NVarChar) { Value = receiverUsername }
                };

                // Execute the query
                await _dbConnection.ExecuteNonQueryAsync(query, CommandType.Text, parameters);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
} 