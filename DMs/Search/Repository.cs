using Azure.Core;
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
        public const String MESSAGE_INVITES_TABLE_NAME = "CHAT_INVITES";
        public const String MESSAGE_INVITES_SENDER_ROW = "sender";
        public const String MESSAGE_INVITES_RECEIVER_ROW = "receiver";
        public const String FRIEND_REQUESTS_TABLE_NAME = "FRIEND_REQUESTS";
        public const String FRIEND_REQUESTS_SENDER_ROW = "sender";
        public const String FRIEND_REQUESTS_RECEIVER_ROW = "receiver";
        public const String FRIENDS_TABLE_NAME = "FRIENDS";
        public const String FRIENDS_USER1_ROW = "user1";
        public const String FRIENDS_USER2_ROW = "user2";

        public Repository()
        {
            this.databaseConnection = new DatabaseConnection();
        }

        public List<User> GetUsers(string selectQuery)
        {
            List<User> foundUsers = new List<User>();

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
            catch (Exception)
            {
                throw;
            }
            finally
            {
                this.databaseConnection.Disconnect();
            }

            return foundUsers;
        }

        public void SendNewMessageRequest(Dictionary<String, object> invite)
        {
            try
            {
                this.databaseConnection.Connect();

                this.databaseConnection.ExecuteInsert(Repository.MESSAGE_INVITES_TABLE_NAME, invite);
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

        public void RemoveMessageRequest(Dictionary<String, object> request)
        {
            try
            {
                this.databaseConnection.Connect();

                this.databaseConnection.ExecuteDeleteWithAnd(Repository.MESSAGE_INVITES_TABLE_NAME, request);
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

        public bool CheckMessageInviteRequestExistance(int senderUserId, int receiverUserId)
        {
            int foundInvites = 0;

            String selectQuery = $"SELECT * FROM {Repository.MESSAGE_INVITES_TABLE_NAME} WHERE {Repository.MESSAGE_INVITES_SENDER_ROW}={senderUserId} AND {Repository.MESSAGE_INVITES_RECEIVER_ROW}={receiverUserId}";

            try
            {
                this.databaseConnection.Connect();
                DataSet dataSet = this.databaseConnection.ExecuteQuery(selectQuery, Repository.MESSAGE_INVITES_TABLE_NAME);

                int tableIndex = 0;
                foreach (DataRow row in dataSet.Tables[tableIndex].Rows)
                {
                    foundInvites++;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                this.databaseConnection.Disconnect();
            }

            return foundInvites == 1;
        }

        public void CancelAllMessageRequests(int userId)
        {
            try
            {
                this.databaseConnection.Connect();

                this.databaseConnection.ExecuteDelete(Repository.MESSAGE_INVITES_TABLE_NAME, Repository.MESSAGE_INVITES_SENDER_ROW, userId);
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

        public List<int> GetInvites(int receiverId)
        {
            List<int> foundInvites = new List<int>();

            String selectQuery = $"SELECT * FROM {Repository.MESSAGE_INVITES_TABLE_NAME} WHERE {Repository.MESSAGE_INVITES_RECEIVER_ROW} = {receiverId}";

            try
            {
                this.databaseConnection.Connect();
                DataSet dataSet = this.databaseConnection.ExecuteQuery(selectQuery, Repository.MESSAGE_INVITES_TABLE_NAME);

                int inviteTableIndex = 0;
                foreach (DataRow row in dataSet.Tables[inviteTableIndex].Rows)
                {
                    int foundId = Convert.ToInt32(row[Repository.MESSAGE_INVITES_SENDER_ROW]);
                    foundInvites.Add(foundId);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                this.databaseConnection.Disconnect();
            }

            return foundInvites;
        }

        public void SendFriendRequest(int senderUserId, int receiverUserId)
        {
            try
            {
                this.databaseConnection.Connect();

                Dictionary<String, object> newRequest = new Dictionary<String, object>();
                newRequest.Add(FRIEND_REQUESTS_SENDER_ROW, senderUserId);
                newRequest.Add(FRIEND_REQUESTS_RECEIVER_ROW, receiverUserId);

                this.databaseConnection.ExecuteInsert(FRIEND_REQUESTS_TABLE_NAME, newRequest);
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

        public void CancelFriendRequest(int senderUserId, int receiverUserId)
        {
            try
            {
                this.databaseConnection.Connect();

                String deleteQuery = $"DELETE FROM {FRIEND_REQUESTS_TABLE_NAME} WHERE {FRIEND_REQUESTS_SENDER_ROW}={senderUserId} AND {FRIEND_REQUESTS_RECEIVER_ROW}={receiverUserId}";
                this.databaseConnection.ExecuteNonQuery(deleteQuery);
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

        public bool CheckFriendRequestExists(int senderUserId, int receiverUserId)
        {
            try
            {
                this.databaseConnection.Connect();

                String selectQuery = $"SELECT * FROM {FRIEND_REQUESTS_TABLE_NAME} WHERE {FRIEND_REQUESTS_SENDER_ROW}={senderUserId} AND {FRIEND_REQUESTS_RECEIVER_ROW}={receiverUserId}";
                DataSet dataSet = this.databaseConnection.ExecuteQuery(selectQuery, FRIEND_REQUESTS_TABLE_NAME);

                return dataSet.Tables[0].Rows.Count > 0;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                this.databaseConnection.Disconnect();
            }
        }

        public bool CheckFriendshipExists(int userId1, int userId2)
        {
            try
            {
                this.databaseConnection.Connect();

                String selectQuery = $"SELECT * FROM {FRIENDS_TABLE_NAME} WHERE ({FRIENDS_USER1_ROW}={userId1} AND {FRIENDS_USER2_ROW}={userId2}) OR ({FRIENDS_USER1_ROW}={userId2} AND {FRIENDS_USER2_ROW}={userId1})";
                DataSet dataSet = this.databaseConnection.ExecuteQuery(selectQuery, FRIENDS_TABLE_NAME);

                return dataSet.Tables[0].Rows.Count > 0;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                this.databaseConnection.Disconnect();
            }
        }
    }
}
