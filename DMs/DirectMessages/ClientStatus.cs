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

        /// <summary>
        /// Constructor for the Client Status class
        /// </summary>
        public ClientStatus()
        {
            this.isAdmin = this.IsMuted = this.IsHost = this.IsConnected = false;
        }

        /// <summary>
        /// Check if the client is a regular user
        /// </summary>
        /// <returns>True or False</returns>
        public bool IsRegularUser()
        {
            return !(this.IsHost || this.IsAdmin);
        }
    }
}
