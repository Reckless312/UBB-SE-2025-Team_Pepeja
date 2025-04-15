namespace Search
{
    public enum FriendshipStatus
    {
        NotFriends,
        Friends,
        RequestSent,
        RequestReceived
    }

    public class User
    {
        private int id;
        private string userName;
        private string ipAddress;
        private FriendshipStatus friendshipStatus;

        public int Id
        {
            get { return id; }
        }
        public string UserName
        {
            get { return userName; }
        }
        public string IpAddress
        {
            get
            {
                return ipAddress;
            }
            set
            {
                this.ipAddress = value;
            }
        }

        public FriendshipStatus FriendshipStatus
        {
            get { return friendshipStatus; }
            set { friendshipStatus = value; }
        }

        public string GetFriendButtonText(FriendshipStatus status)
        {
            switch (status)
            {
                case FriendshipStatus.Friends:
                    return "Friends";
                case FriendshipStatus.RequestSent:
                    return "Cancel Request";
                case FriendshipStatus.RequestReceived:
                    return "Accept Request";
                case FriendshipStatus.NotFriends:
                default:
                    return "Add Friend";
            }
        }

        public User(int id, string userName, string ipAddress)
        {
            this.id = id;
            this.userName = userName;
            this.ipAddress = ipAddress;
        }
    }
}