using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App1.Models;
using App1.Repositories;

namespace App1.Services
{
    public class FriendService : IFriendService
    {
        private readonly IFriendRepository _friendRepository;
        
        public FriendService(IFriendRepository friendRepository)
        {
            _friendRepository = friendRepository ?? throw new ArgumentNullException(nameof(friendRepository));
        }

        public async Task<IEnumerable<Friend>> GetFriendsAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            return await _friendRepository.GetFriendsAsync(username);
        }

        public async Task<bool> AddFriendAsync(string user1Username, string user2Username, string friendEmail, string friendProfilePhotoPath)
        {
            if (string.IsNullOrEmpty(user1Username) || string.IsNullOrEmpty(user2Username))
            {
                throw new ArgumentException("Both usernames must be provided");
            }

            return await _friendRepository.AddFriendAsync(user1Username, user2Username, friendEmail, friendProfilePhotoPath);
        }

        public async Task<bool> RemoveFriendAsync(string user1Username, string user2Username)
        {
            if (string.IsNullOrEmpty(user1Username) || string.IsNullOrEmpty(user2Username))
            {
                throw new ArgumentException("Both usernames must be provided");
            }

            return await _friendRepository.RemoveFriendAsync(user1Username, user2Username);
        }
    }
} 