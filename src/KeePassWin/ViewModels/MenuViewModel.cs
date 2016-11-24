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
        private readonly DatabaseCache _cache;
        private readonly INavigationPane _navPane;

        public MenuViewModel(INavigator navigator, DatabaseCache cache, INavigationPane navPane)
        {
            // TODO: Add ability to indicate which page your on by listening for navigation events once the NuGet package has been updated. Change CanNavigate to use whether or not your on that page to return false.
            // As-is, if navigation occurs via the back button, we won't know and can't update the _canNavigate value
            _navigator = navigator;
            _cache = cache;
            _navPane = navPane;

            Databases = new ObservableCollection<MenuItemViewModel>();
            SettingsCommand = new DelegateCommand(() => _navigator.GoToSettings());
            OpenCommand = new DelegateCommand(async () => await _cache.AddDatabaseAsync());

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
                Command = new DelegateCommand(() =>
                {
                    _navigator.GoToDatabaseView(dbFile.IdFromPath(), KeePassId.Empty);
                    _navPane.Dismiss();
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