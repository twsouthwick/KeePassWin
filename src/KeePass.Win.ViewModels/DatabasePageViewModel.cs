using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KeePass.Win.ViewModels
{
    public class DatabasePageViewModel : ViewModelBase
    {
        private readonly IDatabaseCache _unlocker;
        private readonly INavigator _navigator;
        private readonly IClipboard<string> _clipboard;
        private readonly ICredentialProvider _credentialProvider;
        private readonly INameProvider _nameProvider;
        private readonly ILogger _log;
        private readonly ILauncher _launcher;
        private readonly IMessageDialogFactory _dialogs;

        private IKeePassDatabase _database;
        private IKeePassGroup _group;
        private IList<IKeePassGroup> _parents;

#if FEATURE_SAVE
        private bool _saving;
#endif
        private bool _activeSearch;

        public DatabasePageViewModel(
            INavigator navigator,
            IDatabaseCache unlocker,
            IClipboard<string> clipboard,
            ICredentialProvider credentialProvider,
            INameProvider nameProvider,
            ILauncher launcher,
            IMessageDialogFactory dialogs,
            ILogger log)
        {
            _clipboard = clipboard;
            _unlocker = unlocker;
            _navigator = navigator;
            _credentialProvider = credentialProvider;
            _nameProvider = nameProvider;
            _launcher = launcher;
            _dialogs = dialogs;
            _log = log;

            ItemClickCommand = new DelegateCommand<IKeePassId>(item => GroupClicked(item as IKeePassGroup), item => (item as IKeePassGroup) != null);

            GoToParentCommand = new DelegateCommand<IKeePassGroup>(group =>
            {
                if (_database != null)
                {
                    _navigator.GoToDatabaseView(_database, group);
                }
            });

            CopyCommand = new DelegateCommand<string>(text => _clipboard.Copy(text));

            OpenUrlCommand = new DelegateCommand<IKeePassEntry>(async entry =>
            {
                if (Uri.TryCreate(entry.Url, UriKind.Absolute, out Uri uri))
                {
                    await _launcher.LaunchUriAsync(uri);
                }
            });

            AddEntryCommand = new DelegateCommand(async () =>
            {
                var name = await _nameProvider.GetNameAsync();

                if (!string.IsNullOrEmpty(name))
                {
                    Items.Add(_group.CreateEntry(name));
                }

                NotifyAllCommands();

#if FEATURE_SAVE
                // This is disabled currently due to twsouthwick/KeePassWin 15
            }, () => !_saving);
#else
            }, () => !_activeSearch);
#endif

            FilterCommand = new DelegateCommand<string>(Search);

            RemoveGroupCommand = new DelegateCommand<IKeePassGroup>(async group =>
            {
                if (await _dialogs.CheckToDeleteAsync("group", group.Name))
                {
                    group.Remove();
                    Items.Remove(group);

                    NotifyAllCommands();
                }
            });

            RemoveEntryCommand = new DelegateCommand<IKeePassEntry>(async entry =>
            {
                if (await _dialogs.CheckToDeleteAsync("entry", entry.Title))
                {
                    entry.Remove();
                    Items.Remove(entry);

                    NotifyAllCommands();
                }
            });

            AddGroupCommand = new DelegateCommand(async () =>
            {
                var name = await _nameProvider.GetNameAsync();

                if (!string.IsNullOrEmpty(name))
                {
                    Items.Insert(GetGroupIndex(), _group.CreateGroup(name));
                }

                NotifyAllCommands();

#if FEATURE_SAVE
                // This is disabled currently due to twsouthwick/KeePassWin 15
            }, () => !_saving);
#else
            }, () => !_activeSearch);
#endif

            RenameGroupCommand = new DelegateCommand<IKeePassGroup>(async group =>
            {
                if (group == null)
                {
                    return;
                }

                var name = await _nameProvider.GetNameAsync(group.Name);

                if (!string.IsNullOrEmpty(name))
                {
                    group.Name = name;
                }
            });

            SaveCommand = new DelegateCommand(async () =>
            {
#if FEATURE_SAVE
                _saving = true;
#endif
                NotifyAllCommands();

                try
                {
                    await Database.SaveAsync().ConfigureAwait(false);

                    await _dialogs.DatabaseSavedAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    await _dialogs.ErrorSavingDatabaseAsync(e).ConfigureAwait(false);
                }
                finally
                {
#if FEATURE_SAVE
                    _saving = false;
#endif
                    NotifyAllCommands();
                }

#if FEATURE_SAVE
                // This is disabled currently due to twsouthwick/KeePassWin 15
            }, () => Database?.Modified == true && !_saving);
#else
            });
#endif
        }

        public ICommand RenameGroupCommand { get; }

        public ICommand GoToParentCommand { get; }

        public ICommand OpenUrlCommand { get; }

        public ICommand CopyCommand { get; }

        public ICommand ItemClickCommand { get; }

        public ICommand RemoveGroupCommand { get; }

        public ICommand RemoveEntryCommand { get; }

        public ICommand AddEntryCommand { get; }

        public ICommand AddGroupCommand { get; }

        public ICommand SaveCommand { get; }

        public ICommand FilterCommand { get; }

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

        /// <summary>
        /// Get the index of the last group item in Items. This is used to add groups into the right location of the collection
        /// </summary>
        /// <returns></returns>
        private int GetGroupIndex()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (!(Items[i] is IKeePassGroup))
                {
                    return i;
                }
            }

            return Items.Count;
        }

        private void NotifyAllCommands()
        {
            AddGroupCommand.RaiseCanExecuteChanged();
            AddEntryCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
        }

        private void GroupClicked(IKeePassGroup group)
        {
            _navigator.GoToDatabaseView(Database, group);
        }

        private void Search(string query)
        {
            bool FilterEntry(IKeePassEntry entry, string text)
            {
                return Contains(entry.Title, text) || Contains(entry.Notes, text) || Contains(entry.Group.Name, text);
            }

            bool Contains(string source, string searchText)
            {
                return CultureInfo.CurrentCulture.CompareInfo.IndexOf(source, searchText, CompareOptions.IgnoreCase) >= 0;
            }

            if (string.IsNullOrEmpty(query))
            {
                UpdateItems(Group, true);
                _activeSearch = false;
            }
            else
            {
                _activeSearch = true;

                var items = _database.Root.EnumerateAllEntries()
                    .Where(item => FilterEntry(item, query))
                    .OrderBy(o => o.Title, StringComparer.CurrentCultureIgnoreCase);

                Items.Clear();

                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }

            NotifyAllCommands();
        }

        public async Task SetDatabase(KeePassId dbId, KeePassId groupId)
        {
            var db = await _unlocker.UnlockAsync(dbId, _credentialProvider);

            if (db == null)
            {
                _navigator.GoBack();
            }
            else
            {
                Database = db;
                UpdateItems(db.GetGroup(groupId));
            }
        }

        public void UpdateItems(IKeePassGroup group, bool force = false)
        {
            if (!force && Equals(Group, group))
            {
                return;
            }

            // Set group and entries into the Items container
            Group = group;

            Items.Clear();

            foreach (var item in Group.Groups)
            {
                Items.Add(item);
            }

            foreach (var item in Group.Entries)
            {
                Items.Add(item);
            }

            // Add parents to generate breadcrump navigation
            Parents = Group.EnumerateParents(includeSelf: true).Reverse().ToList();
        }
    }
}
