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

        public DatabasePageViewModel(INavigator navigator, IDatabaseUnlockerDialog unlocker, IClipboard clipboard, IDatabaseTracker tracker, Func<IKeePassEntry, IEntryView> entryView)
        {
            _clipboard = clipboard;
            _unlocker = unlocker;
            _tracker = tracker;
            _navigator = navigator;

            ItemClickCommand = new DelegateCommand<IKeePassId>(async item =>
            {
                if (item is IKeePassGroup)
                {
                    GroupClicked(item as IKeePassGroup);
                }
                else if (item is IKeePassEntry)
                {
                    var dialog = entryView(item as IKeePassEntry);
                    if (await dialog.ShowAsync())
                    {
                        _saveCommand.RaiseCanExecuteChanged();
                    }
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

            GoToSearchCommand = new DelegateCommand<string>(text => _navigator.GoToSearch(Database.Id, text));

            _addEntryCommand = new DelegateCommand(async () =>
            {
                var entry = new ReadWriteKeePassEntry
                {
                    Group = _group
                };

                var view = entryView(entry);
                if (await view.ShowAsync())
                {
                    var kdbxEntry = _group.AddEntry(entry);
                    Items.Add(kdbxEntry);

                    NotifyAllCommands();
                }
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

        public void UpdateItems(IKeePassGroup group)
        {
            if (group != null && Group == group)
            {
                return;
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

        public ICommand GoToSearchCommand { get; }

        public ICommand GoToParentCommand { get; }

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
