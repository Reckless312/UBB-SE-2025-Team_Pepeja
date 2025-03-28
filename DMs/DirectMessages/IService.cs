using System;
using System.Threading.Tasks;

namespace DirectMessages
{
    internal interface IService
    {
        public event EventHandler<MessageEventArgs> NewMessageEvent;
        public event EventHandler<ClientStatusEventArgs> ClientStatusChangedEvent;
        public Task ConnectUserToServer();
        public Task SendMessage(String message);
        public Task DisconnectClient();
        public Task TryChangeMuteStatus(String targetedUser);
        public Task TryChangeAdminStatus(String targetedUser);
        public Task TryKick(String targetedUser);
        public void SendFriendRequest(String targetedUser);
        public void CancelFriendRequest(String targetedUser);
        public bool IsInFriendRequests(String userName);

    }
}
