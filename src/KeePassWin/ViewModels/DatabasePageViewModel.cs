using KeePass.IO.Database;
using KeePass.Models;
using Prism.Commands;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;

namespace KeePassWin.ViewModels
{
    public class DatabasePageViewModel : ViewModelBase
    {
        private readonly IDatabaseUnlocker _unlocker;
        private readonly DatabaseTracker _tracker;
        private readonly INavigationService _navigator;

        private IKeePassDatabase _database;
        private IKeePassGroup _group;

        public DatabasePageViewModel(INavigationService navigator, IDatabaseUnlocker unlocker, DatabaseTracker tracker)
        {
            _unlocker = unlocker;
            _tracker = tracker;
            _navigator = navigator;

            GroupClickCommand = new DelegateCommand<IKeePassGroup>(GroupClicked);
            EntryClickCommand = new DelegateCommand(() => { });
        }

        private void GroupClicked(IKeePassGroup group)
        {
            _navigator.Navigate("Database", DatabaseGroupParameter.Encode(Database.Id, group.Id));
        }

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            var key = DatabaseGroupParameter.Parse(((string)e.Parameter));
            var db = await UnlockAsync(key.Database);

            if (db == null)
            {
                _navigator.GoBack();
            }
            else
            {
                Database = db;
                Group = db.GetGroup(key.Group) ?? db.Root;
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

        public ICommand GroupClickCommand { get; }

        public ICommand EntryClickCommand { get; }

        public IKeePassGroup Group
        {
            get { return _group; }
            set { SetProperty(ref _group, value); }
        }

        public IKeePassDatabase Database
        {
            get { return _database; }
            set { SetProperty(ref _database, value); }
        }
    }
}
