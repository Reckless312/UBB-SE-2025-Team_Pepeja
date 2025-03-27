using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;

namespace DirectMessages
{
    internal class Service: IService
    {
        private Server? server;
        private Client client;
        private DispatcherQueue uiThread;

        public event EventHandler<MessageEventArgs> NewMessageEvent;

        private String userName;
        private String userIpAddress;
        private String serverInviteIp;

        private const String HOST_IP_FINDER = "None";

        public Service(String userName, String userIpAddress, String serverInviteIp, DispatcherQueue uiThread)
        {
            this.userName = userName;
            this.userIpAddress = userIpAddress;
            this.serverInviteIp = serverInviteIp;
            this.uiThread = uiThread;
        }

        public async Task ConnectUserToServer()
        {
            if(serverInviteIp == HOST_IP_FINDER)
            {
                this.server = new Server(this.userIpAddress, this.userName);
                this.server.Start();
                this.serverInviteIp = this.userIpAddress;
            }

            this.client = new Client(serverInviteIp, userName, this.uiThread);
            await this.client.ConnectToServer();
            this.client.NewMessageReceivedEvent += UpdateNewMessage;
        }

        private void UpdateNewMessage(object? sender, MessageEventArgs messageEventArgs)
        {
            Message newMessage = messageEventArgs.Message;
            newMessage.MessageAligment = newMessage.MessageSenderName == this.userName ? "Right" : "Left";
            this.uiThread.TryEnqueue(() => this.NewMessageEvent?.Invoke(this, new MessageEventArgs(newMessage)));
        }

        public async Task SendMessage(String message)
        {
            if(message.Length == 0)
            {
                throw new Exception("Message content can't be empty");
            }
            await this.client.SendMessageToServer(message);
        }
    }
}
