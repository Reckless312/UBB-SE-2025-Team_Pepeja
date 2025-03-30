using System.Collections.Generic;
using System.Threading.Tasks;
using App1.Models;

namespace App1.Services
{
    public interface IFriendService
    {
        Task<IEnumerable<Friend>> GetFriendsAsync(string username);
        Task<bool> AddFriendAsync(string user1Username, string user2Username, string friendEmail, string friendProfilePhotoPath);
        Task<bool> RemoveFriendAsync(string user1Username, string user2Username);
    }
} 