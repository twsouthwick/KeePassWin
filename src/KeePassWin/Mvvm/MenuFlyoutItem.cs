using Windows.UI.Xaml;

namespace KeePass.Win.Mvvm
{
    public class MenuFlyoutItem : Windows.UI.Xaml.Controls.MenuFlyoutItem
    {
        public static readonly DependencyProperty ShortcutProperty = DependencyProperty.Register(nameof(Shortcut), typeof(ShortcutName), typeof(MenuFlyoutItem), new PropertyMetadata(default(ShortcutName)));

        public ShortcutName Shortcut
        {
            get { return (ShortcutName)GetValue(ShortcutProperty); }
            set { SetValue(ShortcutProperty, value); }
        }
    }
}
