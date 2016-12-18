using Autofac;
using Prism.Autofac.Windows;
using System;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KeePass.Win.Mvvm
{
    public static partial class Keyboard
    {
        private static Lazy<KeyboardShortcuts> s_keyboardHandler = new Lazy<KeyboardShortcuts>(() =>
        {
            var container = ((PrismAutofacApplication)Application.Current).Container;
            return container.Resolve<KeyboardShortcuts>();
        }, true);

        public static readonly DependencyProperty FocusProperty = DependencyProperty.RegisterAttached("Focus", typeof(ShortcutName), typeof(MvvmExtensions), new PropertyMetadata(default(ShortcutName), FocusPropertyChanged));

        public static void SetFocus(Control attached, ShortcutName value)
        {
            attached.SetValue(FocusProperty, value);
        }

        public static ShortcutName GetFocus(Control attached)
        {
            return (ShortcutName)attached.GetValue(FocusProperty);
        }

        private static void FocusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var name = (ShortcutName)e.NewValue;
            var control = d as Control;

            if (name == default(ShortcutName) || control == null)
            {
                return;
            }

            var handler = new TypedEventHandler<CoreWindow, KeyEventArgs>((s, args) =>
            {
                var keypress = s_keyboardHandler.Value.ProcessKeypress((int)args.VirtualKey);

                if (keypress == name)
                {
                    args.Handled = true;
                    control.Focus(FocusState.Keyboard);
                }
            });

            Window.Current.CoreWindow.KeyDown += handler;
            control.Unloaded += (_, __) => Window.Current.CoreWindow.KeyDown -= handler;
        }
    }
}
