using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Steam_Community.DirectMessages.Models;
using Steam_Community.DirectMessages.Services;

namespace Steam_Community.DirectMessages.Tests.Services
{
    [TestClass]
    public class NetworkServerTests
    {
        private NetworkServer networkServer;
        private Mock<Socket> mockServerSocket;
        private Mock<Socket> mockClientSocket;
        private const string TestHostIp = "192.168.1.1";
        private const string TestHostUsername = "HostUser";

        [TestInitialize]
        public void Initialize()
        {
            // Create mocks for Socket to avoid actual network operations
            mockServerSocket = new Mock<Socket>(SocketType.Stream, ProtocolType.Tcp);
            mockClientSocket = new Mock<Socket>(SocketType.Stream, ProtocolType.Tcp);

            // Setup mock endpoint for client socket
            var mockEndPoint = new Mock<EndPoint>();
            mockEndPoint.Setup(e => e.ToString()).Returns($"{TestHostIp}:{ChatConstants.PORT_NUMBER}");
            mockClientSocket.Setup(s => s.RemoteEndPoint).Returns(mockEndPoint.Object);

            // Create network server
            networkServer = new NetworkServer(TestHostIp, TestHostUsername);

            // Replace private Socket with our mock
            SetPrivateField(networkServer, "_serverSocket", mockServerSocket.Object);

            // Set running state to true
            SetPrivateField(networkServer, "_isRunning", true);
        }

        [TestMethod]
        public void Constructor_SetsCorrectValues()
        {
            // Assert
            Assert.AreEqual(TestHostUsername, GetPrivateField<string>(networkServer, "_hostUsername"));
            Assert.IsTrue(GetPrivateField<bool>(networkServer, "_isRunning"));

            // Check server endpoint
            IPEndPoint ipEndPoint = GetPrivateField<IPEndPoint>(networkServer, "_ipEndPoint");
            Assert.AreEqual(IPAddress.Parse(TestHostIp), ipEndPoint.Address);
            Assert.AreEqual(ChatConstants.PORT_NUMBER, ipEndPoint.Port);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Constructor_InvalidIpAddress_ThrowsException()
        {
            // Act - This should throw an exception
            new NetworkServer("invalid_ip", TestHostUsername);
        }

        [TestMethod]
        public void IsRunning_ReturnsRunningStatus()
        {
            // Arrange
            SetPrivateField(networkServer, "_isRunning", true);

            // Act & Assert
            Assert.IsTrue(networkServer.IsRunning());

            // Change running status
            SetPrivateField(networkServer, "_isRunning", false);

            // Act & Assert
            Assert.IsFalse(networkServer.IsRunning());
        }

        [TestMethod]
        public void CreateMessage_ReturnsCorrectMessageObject()
        {
            // Arrange
            string messageContent = "Test message content";
            string senderUsername = "TestSender";

            // Act
            Message message = networkServer.CreateMessage(messageContent, senderUsername);

            // Assert
            Assert.AreEqual(messageContent, message.MessageContent);
            Assert.AreEqual(senderUsername, message.MessageSenderName);
            Assert.IsNotNull(message.MessageDateTime);
            Assert.AreEqual(ChatConstants.ALIGNMENT_LEFT, message.MessageAligment);

            // Message sender status should be regular user since it's not the host
            Assert.AreEqual(ChatConstants.REGULAR_USER_STATUS, message.MessageSenderStatus);
        }

        [TestMethod]
        public void CreateMessage_HostUser_SetsHostStatus()
        {
            // Act
            Message message = networkServer.CreateMessage("Host message", TestHostUsername);

            // Assert
            Assert.AreEqual(ChatConstants.HOST_STATUS, message.MessageSenderStatus);
        }

        [TestMethod]
        public void CreateMessage_AdminUser_SetsAdminStatus()
        {
            // Arrange
            string adminUsername = "AdminUser";
            ConcurrentDictionary<string, bool> adminUsers = GetPrivateField<ConcurrentDictionary<string, bool>>(networkServer, "_adminUsers");
            adminUsers.TryAdd(adminUsername, true);

            // Act
            Message message = networkServer.CreateMessage("Admin message", adminUsername);

            // Assert
            Assert.AreEqual(ChatConstants.ADMIN_STATUS, message.MessageSenderStatus);
        }

        [TestMethod]
        public void IsHostIpAddress_ReturnsCorrectResult()
        {
            // Act & Assert - For host IP
            bool isHostIpResult = InvokePrivateMethod<bool>(networkServer, "IsHostIpAddress", TestHostIp);
            Assert.IsTrue(isHostIpResult);

            // Act & Assert - For non-host IP
            bool isNonHostIpResult = InvokePrivateMethod<bool>(networkServer, "IsHostIpAddress", "192.168.1.2");
            Assert.IsFalse(isNonHostIpResult);
        }

        [TestMethod]
        public void ExtractTargetUsernameFromCommand_ParsesCommandCorrectly()
        {
            // Arrange
            string requesterUsername = "Requester";
            string targetUsername = "Target";
            string command = $"<{requesterUsername}>|{ChatConstants.MUTE_STATUS}|<{targetUsername}>";

            // Act
            string extracted = InvokePrivateMethod<string>(networkServer, "ExtractTargetUsernameFromCommand", command);

            // Assert
            Assert.AreEqual(targetUsername, extracted);
        }

        [TestMethod]
        public void CanChangeUserStatus_ReturnsCorrectResults()
        {
            // Test cases for different combinations of requester and target status

            // Host can change regular user
            bool hostChangeRegular = InvokePrivateMethod<bool>(networkServer, "CanChangeUserStatus",
                ChatConstants.HOST_STATUS, ChatConstants.REGULAR_USER_STATUS);
            Assert.IsTrue(hostChangeRegular);

            // Host can change admin
            bool hostChangeAdmin = InvokePrivateMethod<bool>(networkServer, "CanChangeUserStatus",
                ChatConstants.HOST_STATUS, ChatConstants.ADMIN_STATUS);
            Assert.IsTrue(hostChangeAdmin);

            // Host cannot change host
            bool hostChangeHost = InvokePrivateMethod<bool>(networkServer, "CanChangeUserStatus",
                ChatConstants.HOST_STATUS, ChatConstants.HOST_STATUS);
            Assert.IsFalse(hostChangeHost);

            // Admin can change regular user
            bool adminChangeRegular = InvokePrivateMethod<bool>(networkServer, "CanChangeUserStatus",
                ChatConstants.ADMIN_STATUS, ChatConstants.REGULAR_USER_STATUS);
            Assert.IsTrue(adminChangeRegular);

            // Admin cannot change admin
            bool adminChangeAdmin = InvokePrivateMethod<bool>(networkServer, "CanChangeUserStatus",
                ChatConstants.ADMIN_STATUS, ChatConstants.ADMIN_STATUS);
            Assert.IsFalse(adminChangeAdmin);

            // Admin cannot change host
            bool adminChangeHost = InvokePrivateMethod<bool>(networkServer, "CanChangeUserStatus",
                ChatConstants.ADMIN_STATUS, ChatConstants.HOST_STATUS);
            Assert.IsFalse(adminChangeHost);

            // Regular user cannot change anyone
            bool regularChangeRegular = InvokePrivateMethod<bool>(networkServer, "CanChangeUserStatus",
                ChatConstants.REGULAR_USER_STATUS, ChatConstants.REGULAR_USER_STATUS);
            Assert.IsFalse(regularChangeRegular);
        }

        [TestMethod]
        public void GetHighestUserStatus_ReturnsCorrectStatus()
        {
            // Host status test
            string hostStatus = InvokePrivateMethod<string>(networkServer, "GetHighestUserStatus", TestHostUsername);
            Assert.AreEqual(ChatConstants.HOST_STATUS, hostStatus);

            // Setup admin user
            string adminUsername = "AdminUser";
            ConcurrentDictionary<string, bool> adminUsers = GetPrivateField<ConcurrentDictionary<string, bool>>(networkServer, "_adminUsers");
            adminUsers.TryAdd(adminUsername, true);

            // Admin status test
            string adminStatus = InvokePrivateMethod<string>(networkServer, "GetHighestUserStatus", adminUsername);
            Assert.AreEqual(ChatConstants.ADMIN_STATUS, adminStatus);

            // Regular user test
            string regularUsername = "RegularUser";
            adminUsers.TryAdd(regularUsername, false);

            string regularStatus = InvokePrivateMethod<string>(networkServer, "GetHighestUserStatus", regularUsername);
            Assert.AreEqual(ChatConstants.REGULAR_USER_STATUS, regularStatus);
        }

        [TestMethod]
        public void ShutdownServer_ClosesSocketAndSetsRunningToFalse()
        {
            // Setup the mock to track Close call
            mockServerSocket.Setup(s => s.Close());

            // Act
            InvokePrivateMethod(networkServer, "ShutdownServer");

            // Assert
            Assert.IsFalse(GetPrivateField<bool>(networkServer, "_isRunning"));
            mockServerSocket.Verify(s => s.Close(), Times.Once);
        }

        [TestMethod]
        public void RemoveClientInformation_RemovesUserFromAllDictionaries()
        {
            // Arrange
            string username = "TestUser";
            string ipAddress = "192.168.1.2";

            // Setup dictionaries with test user
            SetupUserInServerDictionaries(username, ipAddress, mockClientSocket.Object);

            // Act
            InvokePrivateMethod(networkServer, "RemoveClientInformation", mockClientSocket.Object, username, ipAddress);

            // Assert - Check each dictionary to ensure the user is removed
            ConcurrentDictionary<string, string> ipToUsername = GetPrivateField<ConcurrentDictionary<string, string>>(networkServer, "_ipAddressesToUsernames");
            ConcurrentDictionary<Socket, string> socketToIp = GetPrivateField<ConcurrentDictionary<Socket, string>>(networkServer, "_socketsToIpAddresses");
            ConcurrentDictionary<string, Socket> usernameToSocket = GetPrivateField<ConcurrentDictionary<string, Socket>>(networkServer, "_usernamesToSockets");
            ConcurrentDictionary<string, bool> adminUsers = GetPrivateField<ConcurrentDictionary<string, bool>>(networkServer, "_adminUsers");
            ConcurrentDictionary<string, bool> mutedUsers = GetPrivateField<ConcurrentDictionary<string, bool>>(networkServer, "_mutedUsers");

            Assert.IsFalse(ipToUsername.ContainsKey(ipAddress));
            Assert.IsFalse(socketToIp.ContainsKey(mockClientSocket.Object));
            Assert.IsFalse(usernameToSocket.ContainsKey(username));
            Assert.IsFalse(adminUsers.ContainsKey(username));
            Assert.IsFalse(mutedUsers.ContainsKey(username));
        }

        // Helper method to setup a user in all server dictionaries
        private void SetupUserInServerDictionaries(string username, string ipAddress, Socket clientSocket)
        {
            ConcurrentDictionary<string, string> ipToUsername = GetPrivateField<ConcurrentDictionary<string, string>>(networkServer, "_ipAddressesToUsernames");
            ConcurrentDictionary<Socket, string> socketToIp = GetPrivateField<ConcurrentDictionary<Socket, string>>(networkServer, "_socketsToIpAddresses");
            ConcurrentDictionary<string, Socket> usernameToSocket = GetPrivateField<ConcurrentDictionary<string, Socket>>(networkServer, "_usernamesToSockets");
            ConcurrentDictionary<string, bool> adminUsers = GetPrivateField<ConcurrentDictionary<string, bool>>(networkServer, "_adminUsers");
            ConcurrentDictionary<string, bool> mutedUsers = GetPrivateField<ConcurrentDictionary<string, bool>>(networkServer, "_mutedUsers");

            ipToUsername.TryAdd(ipAddress, username);
            socketToIp.TryAdd(clientSocket, ipAddress);
            usernameToSocket.TryAdd(username, clientSocket);
            adminUsers.TryAdd(username, false);
            mutedUsers.TryAdd(username, false);
        }

        // Helper methods to access private members for testing
        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            Type type = obj.GetType();
            FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(obj, value);
        }

        private static T GetPrivateField<T>(object obj, string fieldName)
        {
            Type type = obj.GetType();
            FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)field.GetValue(obj);
        }

        private static T InvokePrivateMethod<T>(object obj, string methodName, params object[] parameters)
        {
            Type type = obj.GetType();
            MethodInfo method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)method.Invoke(obj, parameters);
        }

        private static void InvokePrivateMethod(object obj, string methodName, params object[] parameters)
        {
            Type type = obj.GetType();
            MethodInfo method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(obj, parameters);
        }
    }
}