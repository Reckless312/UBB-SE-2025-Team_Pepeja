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
        public bool bIsDeveloper;

        public User(int id, string username, bool bIsDeveloper)
        {
            LoadProfilePicture();
            this.id = id;
            this.username = username;
            this.bIsDeveloper = bIsDeveloper;
        }

        /// <summary>
        /// Load the profile picture of the user
        /// </summary>
        private async void LoadProfilePicture()
        {
        #if DEBUG
            try
            {
                string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string imagePath = Path.Combine(exePath, "Assets", "default_avatar.png");
                profilePicture = File.ReadAllBytes(imagePath);
            }
            catch
            {
                profilePicture = new byte[0];
            }
        #endif
        }
    }
}
