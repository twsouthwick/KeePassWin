using KeePass.IO.Database;
using Prism.Commands;
using Prism.Windows.Mvvm;
using System;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.Storage;

namespace KeePassWin.ViewModels
{
    public class PasswordDialogViewModel : ViewModelBase
    {
        private IStorageItem _keyFile;

        public PasswordDialogViewModel(IStorageFile db, DatabaseCache cache, DatabaseTracker tracker)
        {
            Name = db.Name;

            AddKeyFileCommand = new DelegateCommand(async () =>
            {
                KeyFile = await cache.AddKeyFileAsync(db);
            });

            tracker.GetKeyFileAsync(db).ContinueWith(async r =>
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () => KeyFile = r.Result);
            });
        }

        /// <summary>
        /// The key associated with the database. This is of type IStorageItem due to x:Bind restrictions
        /// </summary>
        public IStorageItem KeyFile
        {
            get { return _keyFile; }
            set { SetProperty(ref _keyFile, value); }
        }

        public string Name { get; }

        public ICommand AddKeyFileCommand { get; }

        public string Password { get; set; }

        public bool? AutoSignIn { get; set; } = true;
    }
}
