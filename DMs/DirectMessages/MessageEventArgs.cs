using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectMessages
{
    public class MessageEventArgs : EventArgs
    {
        public Message Message { get; }
        public MessageEventArgs(Message message) => Message = message;
    }
}
