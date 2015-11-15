using KeePass.IO.Database;
using KeePass.Models;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Popups;
using System;
using System.Diagnostics;

namespace KeePassWin.ViewModels
{
    public class DatabasePageViewModel : ViewModelBase
    {
        private readonly IDatabaseUnlocker _unlocker;
        private readonly DatabaseTracker _tracker;
        private readonly INavigationService _navigator;

        private IKeePassDatabase _database;

        public DatabasePageViewModel(INavigationService navigator, IDatabaseUnlocker unlocker, DatabaseTracker tracker)
        {
            _unlocker = unlocker;
            _tracker = tracker;
            _navigator = navigator;
        }

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            var db = await UnlockAsync((string)e.Parameter);

            if (db == null)
            {
                _navigator.GoBack();
            }
            else
            {
                Database = db;
            }
        }

        private async Task<IKeePassDatabase> UnlockAsync(KeePassId id)
        {
            var dbFile = await _tracker.GetDatabaseAsync(id);

            Debug.Assert(dbFile != null);

            try
            {
                return await _unlocker.UnlockAsync(dbFile);
            }
            catch (DatabaseUnlockException e)
            {
                var message = new MessageDialog(e.Message, "Could not open database");

                await message.ShowAsync();
                return null;
            }
        }

        public IKeePassDatabase Database
        {
            get { return _database; }
            set { SetProperty(ref _database, value); }
        }
    }
}
