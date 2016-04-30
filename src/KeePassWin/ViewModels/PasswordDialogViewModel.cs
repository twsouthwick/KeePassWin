using KeePass.IO.Database;
using KeePass.Models;
using Prism.Commands;
using Prism.Windows.Mvvm;
using System;
using System.Windows.Input;
using Windows.ApplicationModel.Core;

namespace KeePassWin.ViewModels
{
    public class PasswordDialogViewModel : ViewModelBase
    {
        private IFile _keyFile;

        public PasswordDialogViewModel(IFile db, DatabaseCache cache, IDatabaseTracker tracker)
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
        public IFile KeyFile
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
