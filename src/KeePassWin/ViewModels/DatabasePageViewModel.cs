using KeePass;
using KeePass.Models;
using KeePassWin.Mvvm;
using Prism.Commands;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;

namespace KeePassWin.ViewModels
{
    public class DatabasePageViewModel : ViewModelBase
    {
        private readonly IDatabaseUnlocker _unlocker;
        private readonly IDatabaseTracker _tracker;
        private readonly INavigator _navigator;
        private readonly IClipboard _clipboard;

        private IKeePassDatabase _database;
        private IKeePassGroup _group;
        private IList<IKeePassGroup> _parents;

        public DatabasePageViewModel(INavigator navigator, IDatabaseUnlocker unlocker, IClipboard clipboard, IDatabaseTracker tracker)
        {
            _clipboard = clipboard;
            _unlocker = unlocker;
            _tracker = tracker;
            _navigator = navigator;

            ItemClickCommand = new DelegateCommand<IKeePassId>(item =>
            {
                if (item is IKeePassGroup)
                {
                    GroupClicked(item as IKeePassGroup);
                }
                else if (item is IKeePassEntry)
                {
                }
            });

            GoToParentCommand = new DelegateCommand<IKeePassGroup>(group =>
            {
                if (_database != null)
                {
                    _navigator.GoToDatabaseView(_database.Id, group.Id);
                }
            });

            CopyCommand = new DelegateCommand<string>(_clipboard.SetText);
        }

        private void GroupClicked(IKeePassGroup group)
        {
            _navigator.GoToDatabaseView(Database.Id, group.Id);
        }

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            var key = DatabaseGroupParameter.Parse(((string)e.Parameter));
            var db = await UnlockAsync(key.Database);

            if (db == null)
            {
                _navigator.GoBack();
                return;
            }

            Database = db;
            Items.Clear();

            // Set group and entries into the Items container
            Group = db.GetGroup(key.Group) ?? db.Root;

            foreach (var item in Group.Groups)
            {
                Items.Add(item);
            }

            foreach (var item in Group.Entries)
            {
                Items.Add(item);
            }

            // Add parents to generate breadcrump navigation
            Parents = Group.EnumerateParents().Reverse().ToList();
        }

        public void GoToSearch(string text)
        {
            _navigator.GoToSearch(Database.Id, text);
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

        public ICommand GoToParentCommand { get; }

        public ICommand CopyCommand { get; }

        public ICommand ItemClickCommand { get; }

        public IKeePassDatabase Database
        {
            get { return _database; }
            set { SetProperty(ref _database, value); }
        }

        public IKeePassGroup Group
        {
            get { return _group; }
            set { SetProperty(ref _group, value); }
        }

        public IList<IKeePassGroup> Parents
        {
            get { return _parents; }
            set { SetProperty(ref _parents, value); }
        }

        public ObservableCollection<IKeePassId> Items { get; } = new ObservableCollection<IKeePassId>();
    }
}
