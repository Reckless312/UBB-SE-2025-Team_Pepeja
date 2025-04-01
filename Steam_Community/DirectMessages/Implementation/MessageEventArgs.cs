using System;

namespace DirectMessages
{
    // Used for events (a new message has been received => show it to the user)
    public class MessageEventArgs : EventArgs
    {
        public Message Message { get; }
        public MessageEventArgs(Message message) => Message = message;
    }
}
