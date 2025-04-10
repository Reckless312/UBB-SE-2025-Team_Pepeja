using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News
{
    public class Users : IUsers
    {
        private static readonly Users _instance = new Users();
        private readonly List<User> _users = new List<User>();
        public static Users Instance { get { return _instance; } }

        private Users()
        {
            //             ID  Username IsDeveloper
            _users.Add(new(1, "JaneSmith", true));
            _users.Add(new(2, "Iraphahell", false));
            _users.Add(new(3, "XSlayder", false));
            _users.Add(new(4, "Tristopher", true));
            _users.Add(new(5, "Gumball", false));
        }

        // Returns the User object that has the provided id
        public User? GetUserById(int id)
        {
            return _users.Find(user => user.id == id);
        }
    }
}
