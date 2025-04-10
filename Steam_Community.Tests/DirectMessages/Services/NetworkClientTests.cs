using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Moq;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Steam_Community.DirectMessages.Models;
using Steam_Community.DirectMessages.Services;

namespace Steam_Community.DirectMessages.Tests.Services
{
    [TestClass]
    public class NetworkClientTests
    {
        private NetworkClient networkClient;
        private Mock<Socket> mockSocket;
        private DispatcherQueue dispatcherQueue;
        private const string TestUsername = "TestUser";
        private const string TestHostIp = "192.168.1.1";

        [TestInitialize]
        public void Initialize()
        {
            // Create a mock for Socket to avoid actual network operations
            mockSocket = new Mock<Socket>(SocketType.Stream, ProtocolType.Tcp);

            // Get the real dispatcher queue
            //dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            // Create a mock DispatcherQueue
            var mockDispatcherQueue = new Mock<DispatcherQueue>();
            mockDispatcherQueue
                .Setup(x => x.TryEnqueue(It.IsAny<DispatcherQueueHandler>()))
                .Returns(true)
                .Callback<DispatcherQueueHandler>(handler => handler());

            // Create the network client
            networkClient = new NetworkClient(TestHostIp, TestUsername, dispatcherQueue);

            // Replace the internal socket with our mock
            SetPrivateField(networkClient, "_clientSocket", mockSocket.Object);
        }

        [TestMethod]
        public void Constructor_SetsCorrectValues()
        {
            // Assert
            Assert.AreEqual(TestUsername, GetPrivateField<string>(networkClient, "_username"));
            Assert.IsNotNull(GetPrivateField<DispatcherQueue>(networkClient, "_uiDispatcherQueue"));

            // Check server endpoint
            IPEndPoint serverEndPoint = GetPrivateField<IPEndPoint>(networkClient, "_serverEndPoint");
            Assert.AreEqual(IPAddress.Parse(TestHostIp), serverEndPoint.Address);
            Assert.AreEqual(ChatConstants.PORT_NUMBER, serverEndPoint.Port);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Constructor_InvalidIpAddress_ThrowsException()
        {
            // Act - This should throw an exception
            new NetworkClient("invalid_ip", TestUsername, dispatcherQueue);
        }

        [TestMethod]
        public async Task ConnectToServer_ConnectsSocketAndSendsUsername()
        {
            // Arrange
            byte[] sentData = null;

            // Setup mock for Socket methods
            // Use a simple setup instead of Returns for ConnectAsync 
            mockSocket.Setup(s => s.ConnectAsync(It.IsAny<EndPoint>()));

            mockSocket.Setup(s => s.SendAsync(It.IsAny<byte[]>(), SocketFlags.None))
                      .Callback<byte[], SocketFlags>((data, flags) => sentData = data)
                      .ReturnsAsync(TestUsername.Length);

            // To prevent test from hanging in ReceiveMessages
            mockSocket.Setup(s => s.ReceiveAsync(It.IsAny<byte[]>(), SocketFlags.None))
                      .ReturnsAsync(0); // Simulate disconnection

            // Act
            await networkClient.ConnectToServer();

            // Assert
            mockSocket.Verify(s => s.ConnectAsync(It.IsAny<EndPoint>()), Times.Once);
            mockSocket.Verify(s => s.SendAsync(It.IsAny<byte[]>(), SocketFlags.None), Times.Once);

            Assert.IsNotNull(sentData);
            string sentUsername = Encoding.UTF8.GetString(sentData);
            Assert.AreEqual(TestUsername, sentUsername);

            // Check that user status is updated
            UserStatus userStatus = GetPrivateField<UserStatus>(networkClient, "_userStatus");
            Assert.IsTrue(userStatus.IsConnected);
        }

        [TestMethod]
        public async Task ConnectToServer_SocketException_ThrowsWrappedException()
        {
            // Arrange
            SocketException socketException = new SocketException();
            mockSocket.Setup(s => s.ConnectAsync(It.IsAny<EndPoint>()))
                      .Throws(socketException);

            try
            {
                // Act
                await networkClient.ConnectToServer();
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception ex)
            {
                // Assert
                Assert.IsTrue(ex.Message.Contains("Failed to connect to server"));
            }
        }

        [TestMethod]
        public async Task SendMessageToServer_SendsMessage()
        {
            // Arrange
            string testMessage = "Test message content";
            byte[] sentData = null;

            mockSocket.Setup(s => s.SendAsync(It.IsAny<byte[]>(), SocketFlags.None))
                      .Callback<byte[], SocketFlags>((data, flags) => sentData = data)
                      .ReturnsAsync(testMessage.Length);

            // Act
            await networkClient.SendMessageToServer(testMessage);

            // Assert
            mockSocket.Verify(s => s.SendAsync(It.IsAny<byte[]>(), SocketFlags.None), Times.Once);

            Assert.IsNotNull(sentData);
            string sentMessage = Encoding.UTF8.GetString(sentData);
            Assert.AreEqual(testMessage, sentMessage);
        }

        [TestMethod]
        public async Task SendMessageToServer_SocketException_ThrowsWrappedException()
        {
            // Arrange
            string testMessage = "Test message";
            SocketException socketException = new SocketException();
            mockSocket.Setup(s => s.SendAsync(It.IsAny<byte[]>(), SocketFlags.None))
                      .ThrowsAsync(socketException);

            try
            {
                // Act
                await networkClient.SendMessageToServer(testMessage);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception ex)
            {
                // Assert
                Assert.IsTrue(ex.Message.Contains("Failed to send message"));
            }
        }

        [TestMethod]
        public void IsConnected_ReturnsUserStatusValue()
        {
            // Arrange
            UserStatus userStatus = GetPrivateField<UserStatus>(networkClient, "_userStatus");

            // Act & Assert - Initial value
            Assert.IsFalse(networkClient.IsConnected());

            // Change connected status
            userStatus.IsConnected = true;

            // Act & Assert - After change
            Assert.IsTrue(networkClient.IsConnected());
        }

        [TestMethod]
        public void SetAsHost_SetsHostStatusToTrue()
        {
            // Act
            networkClient.SetAsHost();

            // Assert
            UserStatus userStatus = GetPrivateField<UserStatus>(networkClient, "_userStatus");
            Assert.IsTrue(userStatus.IsHost);
        }

        [TestMethod]
        public void Disconnect_SendsEmptyMessageAndClosesConnection()
        {
            // Arrange
            byte[] sentData = null;

            mockSocket.Setup(s => s.SendAsync(It.IsAny<byte[]>(), SocketFlags.None))
                      .Callback<byte[], SocketFlags>((data, flags) => sentData = data)
                      .ReturnsAsync(0);

            // Act
            networkClient.Disconnect();

            // Assert
            mockSocket.Verify(s => s.SendAsync(It.IsAny<byte[]>(), SocketFlags.None), Times.Once);

            // Should send a zero-length array equal to disconnect code
            Assert.IsNotNull(sentData);
            Assert.AreEqual(ChatConstants.DISCONNECT_CODE, sentData.Length);

            // Connected status should be false
            UserStatus userStatus = GetPrivateField<UserStatus>(networkClient, "_userStatus");
            Assert.IsFalse(userStatus.IsConnected);
        }

        [TestMethod]
        public void UpdateUserStatus_AdminStatus_TogglesIsAdmin()
        {
            // Arrange
            UserStatus userStatus = GetPrivateField<UserStatus>(networkClient, "_userStatus");
            bool initialAdminValue = userStatus.IsAdmin;

            // Act
            InvokePrivateMethod(networkClient, "UpdateUserStatus", ChatConstants.ADMIN_STATUS);

            // Assert
            Assert.AreNotEqual(initialAdminValue, userStatus.IsAdmin);
        }

        [TestMethod]
        public void UpdateUserStatus_MuteStatus_TogglesIsMuted()
        {
            // Arrange
            UserStatus userStatus = GetPrivateField<UserStatus>(networkClient, "_userStatus");
            bool initialMutedValue = userStatus.IsMuted;

            // Act
            InvokePrivateMethod(networkClient, "UpdateUserStatus", ChatConstants.MUTE_STATUS);

            // Assert
            Assert.AreNotEqual(initialMutedValue, userStatus.IsMuted);
        }

        [TestMethod]
        public void UpdateUserStatus_KickStatus_ClosesConnection()
        {
            // Arrange
            UserStatus userStatus = GetPrivateField<UserStatus>(networkClient, "_userStatus");
            userStatus.IsConnected = true;

            // Act
            InvokePrivateMethod(networkClient, "UpdateUserStatus", ChatConstants.KICK_STATUS);

            // Assert
            Assert.IsFalse(userStatus.IsConnected);
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

        private static object InvokePrivateMethod(object obj, string methodName, params object[] parameters)
        {
            Type type = obj.GetType();
            MethodInfo method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return method.Invoke(obj, parameters);
        }
    }
}