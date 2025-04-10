using Steam_Community.DirectMessages;
using System;

namespace Steam_Community.DirectMessages.Models
{
    /// <summary>
    /// Event arguments for message-related events.
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the message associated with the event.
        /// </summary>
        public Message Message { get; }

        /// <summary>
        /// Initializes a new instance of the MessageEventArgs class.
        /// </summary>
        /// <param name="message">The message to encapsulate.</param>
        public MessageEventArgs(Message message) =>
           Message = message ?? throw new ArgumentNullException(nameof(message));
    }
}