using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;

namespace KeePass.Win.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        private readonly INavigator _navigator;
        private readonly IDatabaseCache _cache;
        private readonly ICredentialProvider _credentialProvider;
        private readonly IMessageDialogFactory _messageDialogs;
        private readonly IDisposable _subscription;

        public MenuViewModel(INavigator navigator, IDatabaseCache cache, ICredentialProvider credentialProvider, IMessageDialogFactory messageDialogs, IFilePicker filePicker)
        {
            _navigator = navigator;
            _cache = cache;
            _credentialProvider = credentialProvider;
            _messageDialogs = messageDialogs;

            Databases = new ObservableCollection<MenuItemViewModel>();
            SettingsCommand = new DelegateCommand(() => _navigator.GoToSettings());
            OpenCommand = new DelegateCommand(async () => await _cache.AddDatabaseAsync(filePicker, false));

            _subscription = cache.Databases
                .ObserveOn(SynchContext)
                .Subscribe(action =>
                {
                    switch (action.action)
                    {
                        case DatabaseAction.Add:
                            AddDatabaseEntry(action.file);
                            break;
                        case DatabaseAction.Open:
                            var entry = AddDatabaseEntry(action.file);
                            entry.Command.Execute(null);
                            break;
                        case DatabaseAction.Remove:
                            RemoveDatabaseEntry(action.file);
                            break;
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _subscription.Dispose();
        }

        public ICommand SettingsCommand { get; }

        public ICommand OpenCommand { get; }

        public ObservableCollection<MenuItemViewModel> Commands { get; set; }

        public ObservableCollection<MenuItemViewModel> Databases { get; set; }

        private void RemoveDatabaseEntry(IFile file)
        {
            var entry = Databases.FirstOrDefault(m => file.IdFromPath() == m.Id);

            Databases.Remove(entry);
        }

        private MenuItemViewModel AddDatabaseEntry(IFile dbFile)
        {
            var id = dbFile.IdFromPath();

            var exists = Databases.FirstOrDefault(m => m.Id == dbFile.IdFromPath());

            if (exists != null)
            {
                return exists;
            }

            var entry = new MenuItemViewModel
            {
                DisplayName = dbFile.Name,
                Id = dbFile.IdFromPath(),
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
                await _cache.RemoveDatabaseAsync(dbFile);
            });

            SynchContext.Post(_ => Databases.Add(entry), null);

            return entry;
        }

        private void RaiseCanExecuteChanged()
        {
            foreach (var item in Commands)
            {
                item.Command.RaiseCanExecuteChanged();
            }
        }
    }
}
