using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace KeePass.Win.Converters
{
    internal class UriVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var url = value as string;

            if (string.IsNullOrWhiteSpace(url))
            {
                return Visibility.Collapsed;
            }

            return Uri.IsWellFormedUriString(url, UriKind.Absolute) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
