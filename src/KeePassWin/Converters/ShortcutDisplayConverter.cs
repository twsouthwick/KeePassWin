using Autofac;
using Prism.Autofac.Windows;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace KeePass.Win.Converters
{
    public class ShortcutDisplayConverter : IValueConverter
    {
        private static Lazy<KeyboardShortcuts> s_keyboardHandler = new Lazy<KeyboardShortcuts>(() =>
        {
            var container = ((PrismAutofacApplication)Application.Current).Container;
            return container.Resolve<KeyboardShortcuts>();
        }, true);

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ShortcutName name)
            {
                return s_keyboardHandler.Value.GetShortcutString(name);
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
