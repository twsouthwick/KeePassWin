using KeePass.IO.Database;
using KeePass.Models;
using KeePassWin.Resources;
using Prism.Commands;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace KeePassWin.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly DatabaseCache _cache;

        private bool _canNavigateToMain = false;
        private bool _canNavigateToSecond = true;

        public MenuViewModel(INavigationService navigationService, DatabaseCache cache)
        {
            // TODO: Add ability to indicate which page your on by listening for navigation events once the NuGet package has been updated. Change CanNavigate to use whether or not your on that page to return false.
            // As-is, if navigation occurs via the back button, we won't know and can't update the _canNavigate value
            _navigationService = navigationService;
            _cache = cache;

            var open = new MenuItemViewModel
            {
                DisplayName = LocalizedStrings.MenuItemOpenCommandTitle,
                FontIcon = Symbol.OpenFile,
                Command = new DelegateCommand(async () => await _cache.AddDatabaseAsync())
            };

            Databases = new ObservableCollection<MenuItemViewModel>();
            Commands = new ObservableCollection<MenuItemViewModel> { open };

            _cache.DatabaseUpdated += DataBaseCacheUpdate;
            _cache.GetDatabaseFilesAsync().ContinueWith(async r =>
            {
                if (r.IsFaulted)
                {
                    return;
                }

                foreach (var item in r.Result)
                {
                    await AddDatabaseEntry(item);
                }
            });
        }

        private async void DataBaseCacheUpdate(object sender, DatabaseCacheEvent arg, IStorageFile database)
        {
            if (arg == DatabaseCacheEvent.Added)
            {
                await AddDatabaseEntry(database);
            }
            else if (arg == DatabaseCacheEvent.AlreadyExists)
            {
                var dialog = new MessageDialog(LocalizedStrings.MenuItemOpenSameFileContent, LocalizedStrings.MenuItemOpenSameFileTitle);

                await dialog.ShowAsync();
            }
            else if (arg == DatabaseCacheEvent.Removed)
            {
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(arg), arg, "Unknown cache event");
            }
        }

        private async Task AddDatabaseEntry(IStorageFile dbFile)
        {
            var entry = new MenuItemViewModel
            {
                DisplayName = dbFile.Name,
                FontIcon = Symbol.ProtectedDocument,
                Command = new DelegateCommand(() => _navigationService.Navigate("Database", (string)KeePassId.FromPath(dbFile)))
            };

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () => Databases.Add(entry));
        }

        public ObservableCollection<MenuItemViewModel> Commands { get; set; }

        public ObservableCollection<MenuItemViewModel> Databases { get; set; }

        private void NavigateToMainPage()
        {
            if (CanNavigateToMainPage())
            {
                if (_navigationService.Navigate("Main", null))
                {
                    _canNavigateToMain = false;
                    _canNavigateToSecond = true;
                    RaiseCanExecuteChanged();
                }
            }
        }

        private bool CanNavigateToMainPage()
        {
            return _canNavigateToMain;
        }

        private void NavigateToSecondPage()
        {
            if (CanNavigateToSecondPage())
            {
                if (_navigationService.Navigate("Second", null))
                {
                    _canNavigateToMain = true;
                    _canNavigateToSecond = false;
                    RaiseCanExecuteChanged();
                }
            }
        }

        private bool CanNavigateToSecondPage()
        {
            return _canNavigateToSecond;
        }

        private void RaiseCanExecuteChanged()
        {
            foreach (var item in Commands)
            {
                (item.Command as DelegateCommand).RaiseCanExecuteChanged();
            }
        }
    }
}