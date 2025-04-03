using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App1.Models;
using App1.Repositories;

namespace App1.Services
{
    public class FriendRequestService : IFriendRequestService
    {
        private readonly IFriendRequestRepository _friendRequestRepository;
        private readonly IFriendService _friendService;
        
        public FriendRequestService(IFriendRequestRepository friendRequestRepository, IFriendService friendService)
        {
            _friendRequestRepository = friendRequestRepository ?? throw new ArgumentNullException(nameof(friendRequestRepository));
            _friendService = friendService ?? throw new ArgumentNullException(nameof(friendService));
        }

        public async Task<IEnumerable<FriendRequest>> GetFriendRequestsAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            return await _friendRequestRepository.GetFriendRequestsAsync(username);
        }

        public async Task<bool> SendFriendRequestAsync(FriendRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.ReceiverUsername))
            {
                throw new ArgumentException("Sender and receiver usernames must be provided");
            }

            // Set the request date to now
            request.RequestDate = DateTime.Now;

            return await _friendRequestRepository.AddFriendRequestAsync(request);
        }

        public async Task<bool> AcceptFriendRequestAsync(string senderUsername, string receiverUsername)
        {
            if (string.IsNullOrEmpty(senderUsername) || string.IsNullOrEmpty(receiverUsername))
            {
                throw new ArgumentException("Sender and receiver usernames must be provided");
            }

            // Get the friend request details before deleting it
            var requests = await _friendRequestRepository.GetFriendRequestsAsync(receiverUsername);
            FriendRequest requestToAccept = null;

            foreach (var request in requests)
            {
                if (request.Username == senderUsername)
                {
                    requestToAccept = request;
                    break;
                }
            }

            if (requestToAccept == null)
            {
                // Request not found
                return false;
            }

            // First, add as friend
            bool friendAdded = await _friendService.AddFriendAsync(
                senderUsername, 
                receiverUsername, 
                requestToAccept.Email,
                requestToAccept.ProfilePhotoPath);

            if (!friendAdded)
            {
                return false;
            }

            // Then delete the friend request
            return await _friendRequestRepository.DeleteFriendRequestAsync(senderUsername, receiverUsername);
        }

        public async Task<bool> RejectFriendRequestAsync(string senderUsername, string receiverUsername)
        {
            if (string.IsNullOrEmpty(senderUsername) || string.IsNullOrEmpty(receiverUsername))
            {
                throw new ArgumentException("Sender and receiver usernames must be provided");
            }

            // Simply delete the friend request
            return await _friendRequestRepository.DeleteFriendRequestAsync(senderUsername, receiverUsername);
        }
    }
} 