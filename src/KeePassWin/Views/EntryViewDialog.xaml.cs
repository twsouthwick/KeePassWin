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
        private bool _canceled;

        public EntryViewDialog(IClipboard clipboard, IKeePassEntry entry)
        {
            this.InitializeComponent();

            Entry = entry.Copy();
            Copy = new DelegateCommand<string>(clipboard.SetText);
            Save = new DelegateCommand(() =>
            {
                Entry.Copy(entry);
            });
            Cancel = new DelegateCommand(() =>
            {
                _canceled = true;
            });
        }

        public ICommand Save { get; }

        public ICommand Cancel { get; }

        public ICommand Copy { get; }

        public IKeePassEntry Entry { get; }

        async Task<bool> IEntryView.ShowAsync()
        {
            await ShowAsync();

            return !_canceled;
        }

        private void PasswordChecked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;

            passwordBox.PasswordRevealMode = checkBox.IsChecked == true ? PasswordRevealMode.Visible : PasswordRevealMode.Hidden;
        }
    }
}




