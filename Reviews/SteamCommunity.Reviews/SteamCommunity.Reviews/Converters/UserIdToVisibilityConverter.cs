using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace SteamCommunity.Reviews.Converters
{
    public class UserIdToVisibilityConverter : IValueConverter
    {
        private const int CurrentUserId = 1;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int userId && userId == CurrentUserId)
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
