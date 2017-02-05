using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace KeePass.Win.Controls
{
    public sealed partial class ShortcutEditor : UserControl
    {
        public static readonly DependencyProperty ShortcutsProperty =
            DependencyProperty.Register(nameof(Shortcuts), typeof(KeyboardShortcuts), typeof(ShortcutEditor), new PropertyMetadata(null, ShortcutPropertyChanged));

        public ShortcutEditor()
        {
            this.InitializeComponent();
        }

        public KeyboardShortcuts Shortcuts
        {
            get { return (KeyboardShortcuts)GetValue(ShortcutsProperty); }
            set { SetValue(ShortcutsProperty, value); }
        }

        private void UpdateShortcuts()
        {
            var shortcuts = Shortcuts;

            MainView.ItemsSource = shortcuts.ShortcutNames
                .Select(n => new ShortcutInfo(n, shortcuts))
                .OrderBy(n => n.Name, StringComparer.CurrentCulture)
                .ToList();
        }

        private static void ShortcutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ShortcutEditor)d).UpdateShortcuts();
        }

        private void TextBoxKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Handled || e.Key == VirtualKey.Tab)
            {
                return;
            }

            var info = (ShortcutInfo)((ContentControl)e.OriginalSource).Content;

            if (info.Update((int)e.Key))
            {
                e.Handled = true;
            }
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    internal class ShortcutInfo : INotifyPropertyChanged
#pragma warning restore SA1402 // File may only contain a single class
    {
        private static readonly ResourceLoader s_loader = ResourceLoader.GetForCurrentView("ShortcutNames");

        private readonly ShortcutName _name;
        private readonly KeyboardShortcuts _shortcuts;

        public ShortcutInfo(ShortcutName name, KeyboardShortcuts shortcuts)
        {
            _shortcuts = shortcuts;
            _name = name;

            Name = s_loader.GetString(name.ToString());

            Debug.Assert(!string.IsNullOrWhiteSpace(Name), "A resource string must be available for the shortcut");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; }

        public string Text => _shortcuts.GetShortcutString(_name);

        public bool Update(int key)
        {
            if (_shortcuts.UpdateKey(_name, key))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));

                return true;
            }

            return false;
        }
    }
}
