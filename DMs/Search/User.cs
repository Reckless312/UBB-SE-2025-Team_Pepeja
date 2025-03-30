namespace Search
{
    public class User
    {
        private int id;
        private string userName;
        private string ipAddress;

        public int Id { get { return id; } }
        public string UserName { get { return userName; } }
        public string IpAddress { get { return ipAddress; } }

        public User(int id, string userName, string ipAddress)
        {
            this.id = id;
            this.userName = userName;
            this.ipAddress = ipAddress;
        }
    }
}
