using System;
using System.ComponentModel;
using System.Linq;
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

    internal class ShortcutInfo : INotifyPropertyChanged
    {
        private readonly ShortcutName _name;
        private readonly KeyboardShortcuts _shortcuts;

        public ShortcutInfo(ShortcutName name, KeyboardShortcuts shortcuts)
        {
            _shortcuts = shortcuts;
            _name = name;

            Name = name.ToString();
        }

        public string Name { get; }

        public string Text => _shortcuts.GetShortcutString(_name);

        public event PropertyChangedEventHandler PropertyChanged;

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
