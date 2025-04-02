using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using App1.Database;
using App1.Models;
using Microsoft.Data.SqlClient;

namespace App1.Repositories
{
    public class FriendRepository : IFriendRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public FriendRepository(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        public async Task<IEnumerable<Friend>> GetFriendsAsync(string username)
        {
            var result = new List<Friend>();
            
            // Define the SQL query to retrieve friends
            string query = @"
                -- Get friends where the user is User1
                SELECT u.Username, u.Email, u.ProfilePhotoPath
                FROM Friends f
                JOIN FriendUsers u ON f.User2Username = u.Username
                WHERE f.User1Username = @Username
                
                UNION
                
                -- Get friends where the user is User2
                SELECT u.Username, u.Email, u.ProfilePhotoPath
                FROM Friends f
                JOIN FriendUsers u ON f.User1Username = u.Username
                WHERE f.User2Username = @Username";

            // Create SQL parameters
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Username", SqlDbType.NVarChar) { Value = username }
            };

            // Execute the query and process the results
            var dataTable = await _dbConnection.ExecuteReaderAsync(query, CommandType.Text, parameters);
            foreach (DataRow row in dataTable.Rows)
            {
                result.Add(new Friend
                {
                    Username = row["Username"].ToString(),
                    Email = row["Email"].ToString(),
                    ProfilePhotoPath = row["ProfilePhotoPath"].ToString()
                });
            }

            return result;
        }

        public async Task<bool> AddFriendAsync(string user1Username, string user2Username, string friendEmail, string friendProfilePhotoPath)
        {
            try
            {
                // Define the SQL query to insert a friendship
                string query = @"
                    -- Ensure alphabetical order for consistency in storage
                    INSERT INTO Friends (User1Username, User2Username)
                    VALUES (
                        CASE WHEN @User1Username <= @User2Username THEN @User1Username ELSE @User2Username END,
                        CASE WHEN @User1Username <= @User2Username THEN @User2Username ELSE @User1Username END
                    )";

                // Create SQL parameters
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@User1Username", SqlDbType.NVarChar) { Value = user1Username },
                    new SqlParameter("@User2Username", SqlDbType.NVarChar) { Value = user2Username }
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

        public async Task<bool> RemoveFriendAsync(string user1Username, string user2Username)
        {
            try
            {
                // Define the SQL query to delete a friendship
                string query = @"
                    DELETE FROM Friends
                    WHERE (User1Username = @User1Username AND User2Username = @User2Username)
                        OR (User1Username = @User2Username AND User2Username = @User1Username)";

                // Create SQL parameters
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@User1Username", SqlDbType.NVarChar) { Value = user1Username },
                    new SqlParameter("@User2Username", SqlDbType.NVarChar) { Value = user2Username }
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