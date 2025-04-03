using System.Collections.Generic;
using System.Threading.Tasks;
using App1.Models;

namespace App1.Repositories
{
    public interface IFriendRequestRepository
    {
        Task<IEnumerable<FriendRequest>> GetFriendRequestsAsync(string username);
        Task<bool> AddFriendRequestAsync(FriendRequest request);
        Task<bool> DeleteFriendRequestAsync(string senderUsername, string receiverUsername);
    }
} 