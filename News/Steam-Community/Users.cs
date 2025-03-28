using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steam_Community
{
    public class Users
    {
        private static Users? m_instance;
        private static List<User> m_users = new();
        public static Users Instance
        {
            get
            {
                if (m_instance == null)
                    m_instance = new Users();

                return m_instance;
            }
        }

        private Users()
        {
            //             ID  Username IsDeveloper
            m_users.Add(new(1, "GabeN", true));
            m_users.Add(new(2, "Iraphahell", false));
            m_users.Add(new(3, "XSlayder", false));
            m_users.Add(new(4, "Tristopher", true));
            m_users.Add(new(5, "Gumball", false));
        }

        public User? GetUserById(int id) => m_users.Find(user => user.id == id);
    }
}
