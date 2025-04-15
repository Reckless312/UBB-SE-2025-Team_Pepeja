﻿using App1.Database;
using App1.Repositories;
using System.Runtime.InteropServices;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using App1.Services;


namespace Friend_Requests_MSTests
{
    [TestClass]
    public sealed class FriendRequestTests
    {
        [TestMethod]
        public void DabaseConnectionDifferentEachTime()
        {
            //Arrange
            SqlConnection DatabaseConnection1 = DatabaseConnection.CreateConnection();
            SqlConnection DatabaseConnection2 = DatabaseConnection.CreateConnection();
            //Act
            //Assert
            Assert.IsFalse(DatabaseConnection1.Equals(DatabaseConnection2));
        }

        [TestMethod]
        public void FriendRepoNeverNull()
        {
            //Arrange
            DatabaseConnection Connection1 = new DatabseConnection;
            IFriendRepository repo1 = FriendRepository(Connection1);
            //Act
            //Assert
            Assert.IsNotNull(repo1._dbConnection);
        }

        [TestMethod]
        public void FriendRepoNeverNull()
        {
            //Arrange
            DatabaseConnection Connection1 = new DatabseConnection;
            IFriendRepository repo1 = FriendRepository(Connection1);
            //Act
            //Assert
            Assert.IsNotNull(repo1._dbConnection);
        }

        [TestMethod]
        public void FriendrequestRepoNeverNull()
        {
            //Arrange
            DatabaseConnection Connection1 = new DatabseConnection;
            IFriendRequestRepository repo1 = FriendRequestRepository(Connection1);
            //Act
            //Assert
            Assert.IsNotNull(repo1._dbConnection);
        }


        [TestMethod]
        public void FriendServiceNeverNull()
        {
            //Arrange
            DatabaseConnection Connection1 = new DatabseConnection;
            IFriendService repo1 = FriendService(Connection1);
            //Act
            //Assert
            Assert.IsNotNull(repo1._dbConnection);
        }

        [TestMethod]
        public void FriendRequestServiceNeverNull()
        {
            //Arrange
            DatabaseConnection Connection1 = new DatabseConnection;
            IFriendRequestService repo1 = FriendRequestService(Connection1);
            //Act
            //Assert
            Assert.IsNotNull(repo1._dbConnection);
        }
    }
}
