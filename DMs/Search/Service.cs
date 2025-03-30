using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Search
{
    public class Service
    {
        private Repository repository;

        public const int MAXIMUM_NUMBER_OF_DISPLAYED_USERS = 10;

        public Service()
        {
            this.repository = new Repository();
        }

        public List<User> GetFirst10UsersMatchedSorted(string username)
        {
            try
            {
                List<User> foundUsers = this.repository.GetUsersByName(username);
                foundUsers.Sort((User firstUser, User secondUser) => String.Compare(firstUser.UserName, secondUser.UserName));
                return foundUsers.Take(Service.MAXIMUM_NUMBER_OF_DISPLAYED_USERS).ToList();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                return new List<User>();
            }
        }

        public void UpdateCurrentUserIpAddress(int userId)
        {
            try
            {
                string hostName = Dns.GetHostName();

                // need to further check if the 4 element is always the ip
                string ipAddress = Dns.GetHostEntry(hostName).AddressList[3].ToString();

                this.repository.UpdateUserIpAddress(ipAddress, userId);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }
    }
}
