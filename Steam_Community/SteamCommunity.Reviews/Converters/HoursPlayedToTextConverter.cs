using Microsoft.UI.Xaml.Data;
using System;

namespace SteamCommunity.Reviews.Converters
{
    public class HoursPlayedToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int hours && hours > 0)
                return $"Played {hours} hour{(hours == 1 ? "" : "s")}";
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
