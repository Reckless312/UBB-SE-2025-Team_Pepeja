using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace SteamCommunity.Reviews.Models
{
    public class User
    {
        public int UserIdentifier { get; set; }
        public string UserName { get; set; } = string.Empty;

        public string ProfilePictureUrlOrPath { get; set; } = string.Empty;

        // Optional methods to retrieve User information
        public int getUserId() => UserIdentifier;
        public string getName() => UserName;

        public string getProfilePicture() => ProfilePictureUrlOrPath;
    }
}