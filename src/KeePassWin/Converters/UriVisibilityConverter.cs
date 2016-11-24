using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace KeePass.Win.Converters
{
    internal class UriVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var entry = value as IKeePassEntry;

            if (string.IsNullOrWhiteSpace(entry?.Url))
            {
                return Visibility.Collapsed;
            }

            Uri uri;
            return Uri.TryCreate(entry.Url, UriKind.Absolute, out uri) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
