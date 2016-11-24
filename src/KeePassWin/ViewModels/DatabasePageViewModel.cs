using KeePass.Models;
using Prism.Commands;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.System;
using Windows.UI.Popups;

namespace KeePass.Win.ViewModels
{
    public class DatabasePageViewModel : ViewModelBase
    {
        private readonly IDatabaseUnlockerDialog _unlocker;
        private readonly IDatabaseTracker _tracker;
        private readonly INavigator _navigator;
        private readonly IClipboard _clipboard;
        private readonly DelegateCommand _saveCommand;
        private readonly DelegateCommand _addEntryCommand;

        private IKeePassDatabase _database;
        private IKeePassGroup _group;
        private IList<IKeePassGroup> _parents;

        private bool _saving;
        private bool _activeSearch;


        public DatabasePageViewModel(INavigator navigator, IDatabaseUnlockerDialog unlocker, IClipboard clipboard, IDatabaseTracker tracker)
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
            });

            GoToParentCommand = new DelegateCommand<IKeePassGroup>(group =>
            {
                if (_database != null)
                {
                    _navigator.GoToDatabaseView(_database.Id, group.Id);
                }
            });

            CopyCommand = new DelegateCommand<string>(_clipboard.SetText);

            OpenUrlCommand = new DelegateCommand<IKeePassEntry>(async entry =>
            {
                Uri uri;

                if (Uri.TryCreate(entry.Url, UriKind.Absolute, out uri))
                {
                    if (!(await Launcher.LaunchUriAsync(uri)))
                    {
                        var dialog = new MessageDialog($"Could not launch {entry.Url}");
                    }
                }
            });

            _addEntryCommand = new DelegateCommand(() =>
            {
                var kdbxEntry = _group.CreateEntry();
                Items.Add(kdbxEntry);

                NotifyAllCommands();
            }, () => !_saving);

            RemoveEntryCommand = new DelegateCommand<IKeePassEntry>(entry =>
            {
                entry.Remove();
                Items.Remove(entry);

                NotifyAllCommands();
            });

            AddGroupCommand = new DelegateCommand(() =>
            {
            }, () => false);

            _saveCommand = new DelegateCommand(async () =>
            {
                _saving = true;
                NotifyAllCommands();

                try
                {
                    await Database.SaveAsync();
                }
                catch (Exception e)
                {
                    var dialog = new MessageDialog("Error saving file");
                    Debug.WriteLine(e);
                    await dialog.ShowAsync();
                }
                finally
                {
                    _saving = false;
                    NotifyAllCommands();
                }
            }, () => Database?.Modified == true && !_saving);
        }

        private void NotifyAllCommands()
        {
            _addEntryCommand.RaiseCanExecuteChanged();
            _saveCommand.RaiseCanExecuteChanged();
        }

        private void GroupClicked(IKeePassGroup group)
        {
            _navigator.GoToDatabaseView(Database.Id, group.Id);
        }

        public void Search(string query)
        {
            _activeSearch = true;
            var items = _database.Root.EnumerateAllEntriesWithParent()
                .Where(item => FilterEntry(item, query))
                .Select(item => item.Entry)
                .OrderBy(o => o.Title, StringComparer.CurrentCultureIgnoreCase);

            Items.Clear();

            foreach (var item in items)
            {
                Items.Add(item);
            }
        }

        public bool TryClearSearch()
        {
            if (!_activeSearch)
            {
                return false;
            }

            _activeSearch = false;
            UpdateItems(Group, true);

            return true;
        }

        private bool FilterEntry(KeePassEntryWithParent item, string text)
        {
            return Contains(item.Entry.Title, text) || Contains(item.Entry.Notes, text) || Contains(item.Parent.Name, text);
        }

        private bool Contains(string source, string searchText)
        {
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(source, searchText, CompareOptions.IgnoreCase) >= 0;
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
            UpdateItems(db.GetGroup(key.Group));
        }

        public void UpdateItems(IKeePassGroup group, bool force = false)
        {
            if (!force)
            {
                if (group != null && Group == group)
                {
                    return;
                }
            }

            Items.Clear();

            // Set group and entries into the Items container
            Group = group ?? Database.Root;

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

        public ICommand OpenUrlCommand { get; }

        public ICommand CopyCommand { get; }

        public ICommand ItemClickCommand { get; }

        public ICommand RemoveEntryCommand { get; }

        public ICommand AddEntryCommand => _addEntryCommand;

        public ICommand AddGroupCommand { get; }

        public ICommand SaveCommand => _saveCommand;

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
