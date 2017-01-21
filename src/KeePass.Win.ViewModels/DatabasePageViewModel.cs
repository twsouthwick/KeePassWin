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
        private readonly IDatabaseCache _unlocker;
        private readonly INavigator _navigator;
        private readonly IClipboard<string> _clipboard;
        private readonly ICredentialProvider _credentialProvider;
        private readonly INameProvider _nameProvider;
        private readonly ILogger _log;

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
            ILogger log)
        {
            _clipboard = clipboard;
            _unlocker = unlocker;
            _navigator = navigator;
            _credentialProvider = credentialProvider;
            _nameProvider = nameProvider;
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
                Uri uri;

                if (Uri.TryCreate(entry.Url, UriKind.Absolute, out uri))
                {
                    if (!(await Launcher.LaunchUriAsync(uri)))
                    {
                        var dialog = new MessageDialog($"Could not launch {entry.Url}");
                    }
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

            RemoveGroupCommand = new DelegateCommand<IKeePassGroup>(async group =>
            {
                if (await CheckToDeleteAsync("group", group.Name))
                {
                    group.Remove();
                    Items.Remove(group);

                    NotifyAllCommands();
                }
            });

            RemoveEntryCommand = new DelegateCommand<IKeePassEntry>(async entry =>
            {
                if (await CheckToDeleteAsync("entry", entry.Title))
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
                    await Database.SaveAsync();

                    var dialog = new MessageDialog("Database saved");
                    await dialog.ShowAsync();
                }
                catch (Exception e)
                {
                    var dialog = new MessageDialog("Error saving file");
                    Debug.WriteLine(e);
                    await dialog.ShowAsync();
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

        private async Task<bool> CheckToDeleteAsync(string type, string name)
        {
            var dialog = new MessageDialog($"Are you sure you want to remove the {type} '{name}'", $"Remove {type}?");
            dialog.Commands.Add(new UICommand { Label = "No", Id = 0 });
            dialog.Commands.Add(new UICommand { Label = "Yes", Id = 1 });

            var result = await dialog.ShowAsync();

            return (int)result.Id == 1;
        }

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
            ((DelegateCommand)AddGroupCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)AddEntryCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
        }

        private void GroupClicked(IKeePassGroup group)
        {
            _navigator.GoToDatabaseView(Database, group);
        }

        public void Search(string query)
        {
            SetSearch(true);

            var items = _database.Root.EnumerateAllEntries()
                .Where(item => FilterEntry(item, query))
                .OrderBy(o => o.Title, StringComparer.CurrentCultureIgnoreCase);

            Items.Clear();

            foreach (var item in items)
            {
                Items.Add(item);
            }
        }

        private void SetSearch(bool activeSearch)
        {
            _activeSearch = activeSearch;
            NotifyAllCommands();
        }

        public bool TryClearSearch()
        {
            if (!_activeSearch)
            {
                return false;
            }

            UpdateItems(Group, true);
            SetSearch(false);

            return true;
        }

        private bool FilterEntry(IKeePassEntry entry, string text)
        {
            return Contains(entry.Title, text) || Contains(entry.Notes, text) || Contains(entry.Group.Name, text);
        }

        private bool Contains(string source, string searchText)
        {
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(source, searchText, CompareOptions.IgnoreCase) >= 0;
        }

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            var key = DatabaseGroupParameter.Decode(((string)e.Parameter));
            var db = await _unlocker.UnlockAsync(key.Database, _credentialProvider);

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
            if (!force && Equals(Group, group))
            {
                return;
            }

            Items.Clear();

            // Set group and entries into the Items container
            Group = group;

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
