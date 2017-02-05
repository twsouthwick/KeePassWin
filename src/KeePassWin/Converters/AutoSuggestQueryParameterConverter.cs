using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace KeePass.Win.Converters
{
    public class AutoSuggestQueryParameterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var args = (AutoSuggestBoxQuerySubmittedEventArgs)value;

            return args.QueryText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
