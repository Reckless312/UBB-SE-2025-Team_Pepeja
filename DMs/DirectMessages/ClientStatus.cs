namespace DirectMessages
{
    public class ClientStatus
    {
        private bool isAdmin;
        private bool isMuted;
        private bool isHost;
        private bool isConnected;

        public bool IsAdmin { get => isAdmin; set => isAdmin = value; }
        public bool IsMuted { get => isMuted; set => isMuted = value; }
        public bool IsHost { get => isHost; set => isHost = value; }
        public bool IsConnected { get => isConnected; set => isConnected = value; }



        public ClientStatus(bool isAdmin, bool isMuted, bool isHost, bool isConnected)
        {
            this.isAdmin = isAdmin;
            this.isMuted = isMuted;
            this.isHost = isHost;
            this.isConnected = isConnected;
        }

        public bool IsRegularUser()
        {
            return !(this.IsHost || this.IsAdmin);
        }
    }
}
