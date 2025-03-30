using System;
using System.Collections.Generic;

namespace Forum
{
    public class User
    {
        public uint Id { get; set; }
        public string Username { get; set; }
        public string ProfilePicturePath { get; set; }
        
        // Dummy users database - in a real application, this would come from a database
        private static Dictionary<uint, User> _users = new Dictionary<uint, User>
        {
            { 1, new User { Id = 1, Username = "JohnDoe", ProfilePicturePath = "ms-appx:///Assets/DefaultUser.png" } },
            { 2, new User { Id = 2, Username = "JaneSmith", ProfilePicturePath = "ms-appx:///Assets/DefaultUser.png" } },
            { 3, new User { Id = 3, Username = "AlexJohnson", ProfilePicturePath = "ms-appx:///Assets/DefaultUser.png" } },
            { 4, new User { Id = 4, Username = "SamWilliams", ProfilePicturePath = "ms-appx:///Assets/DefaultUser.png" } },
            { 5, new User { Id = 5, Username = "TaylorBrown", ProfilePicturePath = "ms-appx:///Assets/DefaultUser.png" } }
        };
        
        // Static method to get a user by ID
        public static User GetUserById(uint userId)
        {
            if (_users.TryGetValue(userId, out User user))
            {
                return user;
            }
            
            // Return a default user if not found
            return new User
            {
                Id = userId,
                Username = $"User_{userId}",
                ProfilePicturePath = "ms-appx:///Assets/DefaultUser.png"
            };
        }
    }
} 