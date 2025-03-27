using System;
using System.Threading.Tasks;

namespace DirectMessages
{
    internal interface IService
    {
        public event EventHandler<MessageEventArgs> NewMessageEvent;
        public Task ConnectUserToServer();
        public Task SendMessage(String message);
        public Task DisconnectClient();

    }
}
