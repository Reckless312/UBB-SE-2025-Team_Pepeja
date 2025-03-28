namespace DirectMessages
{
    public class ClientStatusEventArgs
    {
        public ClientStatus ClientStatus { get; }
        public ClientStatusEventArgs(ClientStatus clientStatus) => ClientStatus = clientStatus;
    }
}
