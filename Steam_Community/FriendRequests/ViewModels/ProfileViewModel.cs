using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using App1.Models;
using App1.Services;
using App1.ViewModels.Commands;
using Windows.System;

namespace App1.ViewModels
{
    public class ProfileViewModel : INotifyPropertyChanged
    {
        private UserProfile _userProfile;
        private readonly FriendRequestViewModel _friendRequestViewModel;

        public ProfileViewModel()
        {
            // Get the FriendRequestViewModel from the service container
            try
            {
                _friendRequestViewModel = Steam_Community.App.GetService<FriendRequestViewModel>();
            }
            catch
            {
                // Fall back to sample data if service container isn't set up yet
                _friendRequestViewModel = null;
            }

            // Initialize with sample data
            _userProfile = new UserProfile
            {
                Username = "JaneSmith",
                Email = "jane.smith.69@fake.email.ai.com",
                ProfilePhotoPath = "ms-appx:///Assets/default_avatar.png"
            };

            // For backward compatibility, if friendRequestViewModel is not available
            if (_friendRequestViewModel == null)
            {
                // Add sample friends with different avatars
                _userProfile.Friends.Add(new Friend
                {
                    Username = "User1",
                    Email = "user1@example.com",
                    ProfilePhotoPath = "ms-appx:///Assets/friend1_avatar.png"
                });

                _userProfile.Friends.Add(new Friend
                {
                    Username = "User2",
                    Email = "user2@example.com",
                    ProfilePhotoPath = "ms-appx:///Assets/friend2_avatar.png"
                });

                _userProfile.Friends.Add(new Friend
                {
                    Username = "User3",
                    Email = "user3@example.com",
                    ProfilePhotoPath = "ms-appx:///Assets/friend3_avatar.png"
                });

                // Add sample friend requests with different avatars
                _userProfile.FriendRequests.Add(new FriendRequest
                {
                    Username = "User4",
                    Email = "user4@example.com",
                    RequestDate = DateTime.Now.AddDays(-1),
                    ProfilePhotoPath = "ms-appx:///Assets/request1_avatar.png"
                });

                _userProfile.FriendRequests.Add(new FriendRequest
                {
                    Username = "User5",
                    Email = "user5@example.com",
                    RequestDate = DateTime.Now.AddDays(-2),
                    ProfilePhotoPath = "ms-appx:///Assets/request2_avatar.png"
                });

                // Initialize commands
                AcceptRequestCommand = new RelayCommand<FriendRequest>(AcceptRequest);
                RejectRequestCommand = new RelayCommand<FriendRequest>(RejectRequest);
                RemoveFriendCommand = new RelayCommand<Friend>(RemoveFriend);
            }
            else
            {
                // Use the commands from FriendRequestViewModel
                AcceptRequestCommand = _friendRequestViewModel.AcceptRequestCommand;
                RejectRequestCommand = _friendRequestViewModel.RejectRequestCommand;
                RemoveFriendCommand = new RelayCommand<Friend>(RemoveFriend);
            }
        }

        public string Username
        {
            get => _userProfile.Username;
            set
            {
                if (_userProfile.Username != value)
                {
                    _userProfile.Username = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Email
        {
            get => _userProfile.Email;
            set
            {
                if (_userProfile.Email != value)
                {
                    _userProfile.Email = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ProfilePhotoPath
        {
            get => _userProfile.ProfilePhotoPath;
            set
            {
                if (_userProfile.ProfilePhotoPath != value)
                {
                    _userProfile.ProfilePhotoPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Friend> Friends => _friendRequestViewModel?.Friends ?? _userProfile.Friends;

        public ObservableCollection<FriendRequest> FriendRequests => _friendRequestViewModel?.FriendRequests ?? _userProfile.FriendRequests;

        // Commands
        public ICommand AcceptRequestCommand { get; }
        public ICommand RejectRequestCommand { get; }
        public ICommand RemoveFriendCommand { get; }

        private void AcceptRequest(FriendRequest request)
        {
            if (request != null && _friendRequestViewModel == null)
            {
                // Add as friend
                Friends.Add(new Friend
                {
                    Username = request.Username,
                    Email = request.Email,
                    ProfilePhotoPath = request.ProfilePhotoPath
                });

                // Remove from requests
                FriendRequests.Remove(request);
            }
        }

        private void RejectRequest(FriendRequest request)
        {
            if (request != null && _friendRequestViewModel == null)
            {
                // Just remove from requests
                FriendRequests.Remove(request);
            }
        }

        private void RemoveFriend(Friend friend)
        {
            if (friend != null)
            {
                Friends.Remove(friend);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}