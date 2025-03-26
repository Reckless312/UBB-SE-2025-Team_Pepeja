using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectMessages
{
    internal interface IService
    {
        public event EventHandler<MessageEventArgs> NewMessage;
        public Task SendMessage(String message);
    }
}
