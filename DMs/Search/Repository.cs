using System;
using System.Collections.Generic;
using System.Data;

namespace Search
{
    public class Repository
    {
        private DatabaseConnection databaseConnection;

        public const String USER_ID_ROW = "id";
        public const String USER_USERNAME_ROW = "username";
        public const String USER_IPADDRESS_ROW = "ipAddress";
        public const String USER_TABLE_NAME = "USERS";

        public Repository()
        {
            this.databaseConnection = new DatabaseConnection();
        }

        public List<User> GetUsersByName(string username)
        {
            List<User> foundUsers = new List<User>();

            String selectQuery = $"SELECT * FROM {Repository.USER_TABLE_NAME} WHERE username LIKE '%{username}%'";

            try
            {
                this.databaseConnection.Connect();
                DataSet dataSet = this.databaseConnection.ExecuteQuery(selectQuery, Repository.USER_TABLE_NAME);

                int userTableIndex = 0;
                foreach (DataRow row in dataSet.Tables[userTableIndex].Rows)
                {
                    int foundId = Convert.ToInt32(row[Repository.USER_ID_ROW]);
                    String foundUserName = row[Repository.USER_USERNAME_ROW]?.ToString() ?? "NULL";
                    String foundIpAddress = row[Repository.USER_IPADDRESS_ROW]?.ToString() ?? "NULL";

                    if (foundIpAddress == "NULL " || foundUserName == "NULL")
                    {
                        continue;
                    }

                    User user = new User(foundId, foundUserName, foundIpAddress);
                    foundUsers.Add(user);
                }
            }
            catch(Exception)
            {
                throw;
            }
            finally
            {
                this.databaseConnection.Disconnect();
            }

            return foundUsers;
        }

        public void UpdateUserIpAddress(String userIpAddress, int userId)
        {
            try
            {
                this.databaseConnection.Connect();

                this.databaseConnection.ExecuteUpdate(Repository.USER_TABLE_NAME, Repository.USER_IPADDRESS_ROW, Repository.USER_ID_ROW, userIpAddress, userId);
            }
            catch (Exception)
            {
                throw;
            }
            finally 
            { 
                this.databaseConnection.Disconnect(); 
            }
        }
    }
}
