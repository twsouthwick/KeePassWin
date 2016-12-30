using Prism.Commands;
using Prism.Windows.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace KeePass.Win.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        private readonly INavigator _navigator;
        private readonly IDatabaseCache _cache;
        private readonly ICredentialProvider _credentialProvider;

        public MenuViewModel(INavigator navigator, IDatabaseCache cache, ICredentialProvider credentialProvider)
        {
            _navigator = navigator;
            _cache = cache;
            _credentialProvider = credentialProvider;

            Databases = new ObservableCollection<MenuItemViewModel>();
            SettingsCommand = new DelegateCommand(() => _navigator.GoToSettings());
            OpenCommand = new DelegateCommand(async () =>
            {
                try
                {
                    var db = await _cache.AddDatabaseAsync();

                    if (db != null)
                    {
                        await AddDatabaseEntryAsync(db);
                    }
                }
                catch (DatabaseAlreadyExistsException)
                {
                    var dialog = new MessageDialog(LocalizedStrings.MenuItemOpenSameFileContent, LocalizedStrings.MenuItemOpenSameFileTitle);

                    await dialog.ShowAsync();
                }
            });

            _cache.GetDatabaseFilesAsync().ContinueWith(async r =>
            {
                if (r.IsFaulted)
                {
                    return;
                }

                foreach (var item in r.Result)
                {
                    await AddDatabaseEntryAsync(item);
                }
            });
        }

        private async Task AddDatabaseEntryAsync(IFile dbFile)
        {
            var id = dbFile.IdFromPath();

            var entry = new MenuItemViewModel
            {
                DisplayName = dbFile.Name,
                FontIcon = Symbol.ProtectedDocument,
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
                        var dialog = new MessageDialog(LocalizedStrings.InvalidCredentials, LocalizedStrings.MenuItemOpenError);

                        await dialog.ShowAsync();
                    }
                    catch (DatabaseUnlockException e)
                    {
                        var dialog = new MessageDialog(e.Message, LocalizedStrings.MenuItemOpenError);

                        await dialog.ShowAsync();
                    }
                })
            };

            entry.RemoveCommand = new DelegateCommand(async () =>
            {
                await _cache.RemoveDatabaseAsync(dbFile);
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => Databases.Remove(entry));
            });

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => Databases.Add(entry));
        }

        public ICommand SettingsCommand { get; }

        public ICommand OpenCommand { get; }

        public ObservableCollection<MenuItemViewModel> Commands { get; set; }

        public ObservableCollection<MenuItemViewModel> Databases { get; set; }

        private void RaiseCanExecuteChanged()
        {
            foreach (var item in Commands)
            {
                (item.Command as DelegateCommand).RaiseCanExecuteChanged();
            }
        }
    }
}