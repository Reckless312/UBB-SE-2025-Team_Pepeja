using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using App1.Models;
using App1.Services;
using App1.ViewModels.Commands;

namespace App1.ViewModels
{
    public class FriendRequestViewModel : INotifyPropertyChanged
    {
        private readonly IFriendRequestService _friendRequestService;
        private readonly IFriendService _friendService;
        private ObservableCollection<FriendRequest> _friendRequests;
        private ObservableCollection<Friend> _friends;
        private bool _isLoading;
        private string _currentUsername;

        public FriendRequestViewModel(IFriendRequestService friendRequestService, string currentUsername)
        {
            _friendRequestService = friendRequestService ?? throw new ArgumentNullException(nameof(friendRequestService));
            _friendService = Steam_Community.App.GetService<IFriendService>(); // Get from service container
            _currentUsername = currentUsername ?? throw new ArgumentNullException(nameof(currentUsername));
            _friendRequests = new ObservableCollection<FriendRequest>();
            _friends = new ObservableCollection<Friend>();

            // Initialize commands
            AcceptRequestCommand = new RelayCommand<FriendRequest>(AcceptRequest);
            RejectRequestCommand = new RelayCommand<FriendRequest>(RejectRequest);
            RemoveFriendCommand = new RelayCommand<Friend>(RemoveFriend);
            
            // Load friend requests and friends
            LoadFriendRequestsAsync();
            LoadFriendsAsync();
        }

        public ObservableCollection<FriendRequest> FriendRequests
        {
            get => _friendRequests;
            set
            {
                if (_friendRequests != value)
                {
                    _friendRequests = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Friend> Friends
        {
            get => _friends;
            set
            {
                if (_friends != value)
                {
                    _friends = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        // Commands
        public ICommand AcceptRequestCommand { get; }
        public ICommand RejectRequestCommand { get; }
        public ICommand RemoveFriendCommand { get; }

        private async void LoadFriendRequestsAsync()
        {
            try
            {
                IsLoading = true;
                var requests = await _friendRequestService.GetFriendRequestsAsync(_currentUsername);
                
                FriendRequests.Clear();
                foreach (var request in requests)
                {
                    FriendRequests.Add(request);
                }
            }
            catch (Exception ex)
            {
                // Handle exception (log or display to user)
                System.Diagnostics.Debug.WriteLine($"Error loading friend requests: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void LoadFriendsAsync()
        {
            try
            {
                IsLoading = true;
                var friends = await _friendService.GetFriendsAsync(_currentUsername);
                
                Friends.Clear();
                foreach (var friend in friends)
                {
                    Friends.Add(friend);
                }
            }
            catch (Exception ex)
            {
                // Handle exception (log or display to user)
                System.Diagnostics.Debug.WriteLine($"Error loading friends: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void AcceptRequest(FriendRequest request)
        {
            if (request == null) return;

            try
            {
                bool success = await _friendRequestService.AcceptFriendRequestAsync(request.Username, _currentUsername);
                if (success)
                {
                    // Remove from the local collection
                    FriendRequests.Remove(request);
                    
                    // Add to friends collection
                    Friends.Add(new Friend
                    {
                        Username = request.Username,
                        Email = request.Email,
                        ProfilePhotoPath = request.ProfilePhotoPath
                    });
                }
            }
            catch (Exception ex)
            {
                // Handle exception (log or display to user)
                System.Diagnostics.Debug.WriteLine($"Error accepting friend request: {ex.Message}");
            }
        }

        private async void RejectRequest(FriendRequest request)
        {
            if (request == null) return;

            try
            {
                bool success = await _friendRequestService.RejectFriendRequestAsync(request.Username, _currentUsername);
                if (success)
                {
                    // Remove from the local collection
                    FriendRequests.Remove(request);
                }
            }
            catch (Exception ex)
            {
                // Handle exception (log or display to user)
                System.Diagnostics.Debug.WriteLine($"Error rejecting friend request: {ex.Message}");
            }
        }

        private async void RemoveFriend(Friend friend)
        {
            if (friend == null) return;

            try
            {
                bool success = await _friendService.RemoveFriendAsync(_currentUsername, friend.Username);
                if (success)
                {
                    // Remove from the local collection
                    Friends.Remove(friend);
                }
            }
            catch (Exception ex)
            {
                // Handle exception (log or display to user)
                System.Diagnostics.Debug.WriteLine($"Error removing friend: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 