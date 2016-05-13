using System;
using System.Threading.Tasks;
using KeePass;
using Windows.UI.Xaml.Controls;

namespace KeePassWin.Views
{
    public sealed partial class EntryViewDialog : ContentDialog, IEntryView
    {
        public EntryViewDialog(IKeePassEntry entry)
        {
            this.InitializeComponent();

            Entry = entry;
        }

        public IKeePassEntry Entry { get; }

        Task IEntryView.ShowAsync() => ShowAsync().AsTask();
    }
}




