using System;

namespace DirectMessages
{
    // Used for events (a new status has been provided to the user => update UI)
    public class ClientStatusEventArgs: EventArgs
    {
        public ClientStatus ClientStatus { get; }
        public ClientStatusEventArgs(ClientStatus clientStatus) => ClientStatus = clientStatus;
    }
}
