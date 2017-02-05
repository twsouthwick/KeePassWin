using System.Windows.Input;

namespace KeePass.Win.ViewModels
{
    public class PasswordDialogViewModel : ViewModelBase
    {
        private IFile _keyFile;

        public PasswordDialogViewModel(IFile db, IDatabaseCache cache, IDatabaseFileAccess tracker)
        {
            Name = db.Name;

            AddKeyFileCommand = new DelegateCommand(async () =>
            {
                KeyFile = await cache.AddKeyFileAsync(db);
            });

            tracker.GetKeyFileAsync(db).ContinueWith((r, o) =>
            {
                KeyFile = r.Result;
            }, SynchContext);
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
