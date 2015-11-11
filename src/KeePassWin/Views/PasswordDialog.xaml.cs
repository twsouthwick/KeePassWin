using KeePassWin.ViewModels;
using System;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace KeePassWin.Views
{
    public sealed partial class PasswordDialog : ContentDialog
    {
        public enum ResultState { None, Open, Cancel };

        public PasswordDialog(IStorageFile db, Func<IStorageFile, PasswordDialogViewModel> modelCreator)
        {
            this.InitializeComponent();

            Model = modelCreator(db);
        }

        public ResultState Result { get; private set; } = ResultState.None;

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                e.Handled = true;
                Result = ResultState.Open;
                Hide();
            }
            else if (e.Key == VirtualKey.Escape)
            {
                e.Handled = true;
                Result = ResultState.Cancel;
                Hide();
            }
        }

        private void CancelClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Result = ResultState.Cancel;
        }

        private void UnlockDatabaseClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Result = ResultState.Open;
        }

        public PasswordDialogViewModel Model { get; }
    }
}




