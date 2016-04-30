using KeePass;
using KeePassWin.Mvvm;
using KeePassWin.Resources;
using Prism.Commands;
using Prism.Windows.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace KeePassWin.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        private readonly INavigator _navigator;
        private readonly DatabaseCache _cache;

        public MenuViewModel(INavigator navigator, DatabaseCache cache)
        {
            // TODO: Add ability to indicate which page your on by listening for navigation events once the NuGet package has been updated. Change CanNavigate to use whether or not your on that page to return false.
            // As-is, if navigation occurs via the back button, we won't know and can't update the _canNavigate value
            _navigator = navigator;
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

        private async void DataBaseCacheUpdate(object sender, DatabaseCacheEvent arg, IFile database)
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

        private async Task AddDatabaseEntry(IFile dbFile)
        {
            var entry = new MenuItemViewModel
            {
                DisplayName = dbFile.Name,
                FontIcon = Symbol.ProtectedDocument,
                Command = new DelegateCommand(() => _navigator.GoToDatabaseView(dbFile.Path, KeePassId.Empty))
            };

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () => Databases.Add(entry));
        }

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