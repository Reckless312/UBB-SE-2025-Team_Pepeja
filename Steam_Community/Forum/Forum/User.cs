using System;
using System.Collections.Generic;

namespace Forum
{
    public class User
    {
        public uint Id { get; set; }
        public string Username { get; set; }
        public string ProfilePicturePath { get; set; }
        
        private static Dictionary<uint, User> _users = new Dictionary<uint, User>
        {
            { 1, new User { Id = 1, Username = "JaneSmith", ProfilePicturePath = "ms-appx:///Assets/friend1_avatar.png" } },
            { 2, new User { Id = 2, Username = "JohnDoe", ProfilePicturePath = "ms-appx:///Assets/default_avatar.png" } },
            { 3, new User { Id = 3, Username = "AlexJohnson", ProfilePicturePath = "ms-appx:///Assets/friend2_avatar.png" } }
        };
        

        public static User GetUserById(uint userId)
        {
            if (_users.TryGetValue(userId, out User user))
            {
                return user;
            }
            
            return new User
            {
                Id = userId,
                Username = $"User_{userId}",
                ProfilePicturePath = "ms-appx:///Assets/DefaultUser.png"
            };
        }
    }
} 