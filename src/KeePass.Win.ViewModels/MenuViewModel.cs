using System.Collections.ObjectModel;
using System.Windows.Input;

namespace KeePass.Win.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        private readonly INavigator _navigator;
        private readonly IDatabaseCache _cache;
        private readonly ICredentialProvider _credentialProvider;
        private readonly IMessageDialogFactory _messageDialogs;

        public MenuViewModel(INavigator navigator, IDatabaseCache cache, ICredentialProvider credentialProvider, IMessageDialogFactory messageDialogs)
        {
            _navigator = navigator;
            _cache = cache;
            _credentialProvider = credentialProvider;
            _messageDialogs = messageDialogs;

            Databases = new ObservableCollection<MenuItemViewModel>();
            SettingsCommand = new DelegateCommand(() => _navigator.GoToSettings());
            OpenCommand = new DelegateCommand(async () =>
            {
                try
                {
                    var db = await _cache.AddDatabaseAsync();

                    if (db != null)
                    {
                        AddDatabaseEntry(db);
                    }
                }
                catch (DatabaseAlreadyExistsException)
                {
                    await _messageDialogs.DatabaseAlreadyExistsAsync();
                }
            });

            _cache.GetDatabaseFilesAsync().ContinueWith(r =>
            {
                if (r.IsFaulted)
                {
                    return;
                }

                foreach (var item in r.Result)
                {
                    AddDatabaseEntry(item);
                }
            });
        }

        private void AddDatabaseEntry(IFile dbFile)
        {
            var id = dbFile.IdFromPath();

            var entry = new MenuItemViewModel
            {
                DisplayName = dbFile.Name,
                Command = new DelegateCommand(async () =>
                {
                    try
                    {
                        var db = await _cache.UnlockAsync(id, _credentialProvider);

                        if (db != null)
                        {
                            _navigator.GoToDatabaseView(db, db.Root);
                        }
                    }
                    catch (InvalidCredentialsException)
                    {
                        await _messageDialogs.InvalidCredentialsAsync();
                    }
                    catch (DatabaseUnlockException e)
                    {
                        await _messageDialogs.UnlockErrorAsync(e.Message);
                    }
                })
            };

            entry.RemoveCommand = new DelegateCommand(async () =>
            {
                await _cache.RemoveDatabaseAsync(dbFile)
                    .ContinueWith((t, o) => Databases.Remove(entry), SynchContext);
            });

            SynchContext.Post(_ => Databases.Add(entry), null);
        }

        public ICommand SettingsCommand { get; }

        public ICommand OpenCommand { get; }

        public ObservableCollection<MenuItemViewModel> Commands { get; set; }

        public ObservableCollection<MenuItemViewModel> Databases { get; set; }

        private void RaiseCanExecuteChanged()
        {
            foreach (var item in Commands)
            {
                item.Command.RaiseCanExecuteChanged();
            }
        }
    }
}