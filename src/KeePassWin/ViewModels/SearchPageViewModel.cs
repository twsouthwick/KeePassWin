using KeePass.IO;
using KeePass.IO.Database;
using KeePass.Models;
using KeePassWin.Mvvm;
using Prism.Commands;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;

namespace KeePassWin.ViewModels
{
    public class SearchPageViewModel : ViewModelBase
    {
        private readonly INavigator _navigator;
        private readonly DatabaseTracker _tracker;
        private readonly IDatabaseUnlocker _unlocker;

        private string _text;
        private IList<IKeePassEntry> _items;
        private IKeePassDatabase _database;

        public SearchPageViewModel(INavigator navigator, IDatabaseUnlocker unlocker, IClipboard clipboard, DatabaseTracker tracker)
        {
            _unlocker = unlocker;
            _tracker = tracker;
            _navigator = navigator;

            CopyCommand = new DelegateCommand<string>(clipboard.SetText);
        }

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            var param = SearchQueryParameter.Parse((string)e.Parameter);
            var db = await UnlockAsync(param.Database);

            if (db == null)
            {
                _navigator.GoBack();
                return;
            }

            _database = db;

            Text = param.Term;
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

        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }

        public IList<IKeePassEntry> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }

        public ICommand ItemClickCommand { get; } = new DelegateCommand(() => { });

        public ICommand CopyCommand { get; }

        public void Update()
        {
            var text = Text.ToLower();

            Items = _database.EnumerateAllEntries().Where(entry => entry.Title.ToLower().Contains(text))
                 .OrderBy(o => o.Title, StringComparer.CurrentCultureIgnoreCase)
                 .ToList();
        }
    }
}
