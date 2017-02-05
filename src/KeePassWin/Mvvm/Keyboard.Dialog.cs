using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KeePass.Win.Mvvm
{
    public partial class Keyboard
    {
        public static readonly DependencyProperty DialogProperty = DependencyProperty.RegisterAttached("Dialog", typeof(bool), typeof(Keyboard), new PropertyMetadata(default(bool), DialogPropertyChanged));

        public static void SetDialog(ContentDialog attached, bool value)
        {
            attached.SetValue(DialogProperty, value);
        }

        public static bool GetDialog(ContentDialog attached)
        {
            return (bool)attached.GetValue(DialogProperty);
        }

        private static void DialogPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var dialog = d as ContentDialog;

            if (!(bool)args.NewValue)
            {
                return;
            }

            var handler = new TypedEventHandler<CoreWindow, KeyEventArgs>((s, e) =>
            {
                if (e.VirtualKey == VirtualKey.Enter)
                {
                    e.Handled = true;
                    (dialog as IContentDialogResult)?.Enter();
                    dialog.Hide();
                }
                else if (e.VirtualKey == VirtualKey.Escape)
                {
                    e.Handled = true;
                    (dialog as IContentDialogResult)?.Escape();
                    dialog.Hide();
                }
            });

            Window.Current.CoreWindow.KeyDown += handler;
            dialog.Unloaded += (s, e) => Window.Current.CoreWindow.KeyDown -= handler;
        }
    }
}
