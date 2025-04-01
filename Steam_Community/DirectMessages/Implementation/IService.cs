using System;
using System.Threading.Tasks;

namespace DirectMessages
{
    internal interface IService
    {
        public event EventHandler<MessageEventArgs> NewMessageEvent;
        public event EventHandler<ClientStatusEventArgs> ClientStatusChangedEvent;
        public event EventHandler<ExceptionEventArgs> ExceptionEvent;
        public void ConnectUserToServer();
        public void SendMessage(String message);
        public void DisconnectClient();
        public void TryChangeMuteStatus(String targetedUser);
        public void TryChangeAdminStatus(String targetedUser);
        public void TryKick(String targetedUser);
    }
}
