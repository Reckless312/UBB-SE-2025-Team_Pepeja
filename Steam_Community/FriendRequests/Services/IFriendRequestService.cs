using System.Collections.Generic;
using System.Threading.Tasks;
using App1.Models;

namespace App1.Services
{
    public interface IFriendRequestService
    {
        Task<IEnumerable<FriendRequest>> GetFriendRequestsAsync(string username);
        Task<bool> SendFriendRequestAsync(FriendRequest request);
        Task<bool> AcceptFriendRequestAsync(string senderUsername, string receiverUsername);
        Task<bool> RejectFriendRequestAsync(string senderUsername, string receiverUsername);
    }
} 