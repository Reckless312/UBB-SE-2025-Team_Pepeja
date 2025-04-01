using System;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Media.Imaging;

namespace App1.Models
{
    public class UserProfile
    {
        private string _profilePhotoPath = string.Empty;

        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string ProfilePhotoPath
        {
            get => string.IsNullOrEmpty(_profilePhotoPath) ? "ms-appx:///Assets/default_avatar.png" : _profilePhotoPath;
            set => _profilePhotoPath = value;
        }

        public ObservableCollection<Friend> Friends { get; set; } = new ObservableCollection<Friend>();
        public ObservableCollection<FriendRequest> FriendRequests { get; set; } = new ObservableCollection<FriendRequest>();
    }

    public class Friend
    {
        private string _profilePhotoPath = string.Empty;

        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string ProfilePhotoPath
        {
            get => string.IsNullOrEmpty(_profilePhotoPath) ? "ms-appx:///Assets/default_avatar.png" : _profilePhotoPath;
            set => _profilePhotoPath = value;
        }
    }

    public class FriendRequest
    {
        private string _profilePhotoPath = string.Empty;

        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string ProfilePhotoPath
        {
            get => string.IsNullOrEmpty(_profilePhotoPath) ? "ms-appx:///Assets/default_avatar.png" : _profilePhotoPath;
            set => _profilePhotoPath = value;
        }

        public string ReceiverUsername { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; } = DateTime.Now;
    }
}