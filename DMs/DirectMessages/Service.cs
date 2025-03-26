using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace DirectMessages
{
    internal class Service: IService
    {
        private String userName;
        private String userIpAddress;
        private String serverIp;

        private Server? server;
        private Client client;
        private DispatcherQueue dispatch;

        private const String HOST_IP_FINDER = "None";

        public event EventHandler<MessageEventArgs> NewMessage;

        public Service(String userName, String userIpAddress, String serverIp, DispatcherQueue dispatcher)
        {
            this.userName = userName;
            this.userIpAddress = userIpAddress;
            this.dispatch = dispatcher;

            try
            {
                this.serverIp = serverIp;
                ConnectUserToServer();
            }
            catch(Exception exception)
            {
                Console.WriteLine($"Error parsing server IP and port: {exception.Message}");
            }
        }

        private void ConnectUserToServer()
        {
            if(serverIp == HOST_IP_FINDER)
            {
                this.server = new Server(this.userIpAddress, this.userName);
                this.serverIp = this.userIpAddress;
            }
            this.client = new Client(serverIp , userName, this.dispatch);

            this.client.NewMessage += (currentObject, eventArgs) =>
            {
                Message newMessage = eventArgs.Message;
                newMessage.MessageAligment = newMessage.MessageSenderName == this.userName ? "Right" : "Left";
                this.dispatch.TryEnqueue(() => this.NewMessage?.Invoke(this, new MessageEventArgs(newMessage)));
            };
        }

        public async Task SendMessage(String message)
        {
            if(message.Length == 0)
            {
                Console.WriteLine("Message is empty.");
            }
            await this.client.SendMessageToServer(message);
        }
    }
}
