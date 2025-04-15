using Microsoft.UI.Xaml.Data;
using System;

namespace SteamCommunity.Reviews.Converters
{
    public class RelativeTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not DateTime date) return "";

            var span = DateTime.Now - date;

            if (span.TotalSeconds < 60)
                return $"{(int)span.TotalSeconds} seconds ago";
            if (span.TotalMinutes < 60)
                return $"{(int)span.TotalMinutes} minutes ago";
            if (span.TotalHours < 24)
                return $"{(int)span.TotalHours} hours ago";
            if (span.TotalDays < 30)
                return $"{(int)span.TotalDays} days ago";
            if (span.TotalDays < 365)
                return $"{(int)(span.TotalDays / 30)} months ago";
            else
                return $"{(int)(span.TotalDays / 365)} years ago";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
