using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

// Bare bones garbage 'cause others weren't willing to share their repos
namespace News
{
    public class User
    {
        public int id;
        public string username;
        public byte[] profilePicture;
        public bool isDeveloper;
        const string profilePicturePath = "C:\\Users\\Mark\\Downloads\\UBB-SE-2025-Team_Pepeja-main\\Steam_Community\\";

        // Create User object
        public User(int id, string username, bool isDeveloper)
        {
            LoadProfilePicture(profilePicturePath);
            this.id = id;
            this.username = username;
            this.isDeveloper = isDeveloper;
        }

        // Load Profile Picture for User
        private void LoadProfilePicture(string profilePicturePath)
        {
            string exePath = Path.GetDirectoryName(profilePicturePath);
            string imagePath = Path.Combine(exePath, "Assets", "default_avatar.png");
            profilePicture = File.ReadAllBytes(imagePath);
        }
    }
}
