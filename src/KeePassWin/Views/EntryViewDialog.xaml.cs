using KeePass;
using Prism.Commands;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace KeePassWin.Views
{
    public sealed partial class EntryViewDialog : ContentDialog, IEntryView
    {
        public EntryViewDialog(IClipboard clipboard, IKeePassEntry entry)
        {
            this.InitializeComponent();

            Entry = entry;
            Copy = new DelegateCommand<string>(clipboard.SetText);
        }

        public ICommand Copy { get; }

        public IKeePassEntry Entry { get; }

        Task IEntryView.ShowAsync() => ShowAsync().AsTask();

        private void PasswordChecked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;

            passwordBox.PasswordRevealMode = checkBox.IsChecked == true ? PasswordRevealMode.Visible : PasswordRevealMode.Hidden;
        }
    }
}




