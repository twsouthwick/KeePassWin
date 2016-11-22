using KeePass;
using KeePass.Models;
using KeePassWin.Mvvm;
using Prism.Commands;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;

namespace KeePassWin.ViewModels
{
    public class SearchPageViewModel : ViewModelBase
    {
        private readonly INavigator _navigator;
        private readonly IDatabaseTracker _tracker;
        private readonly IDatabaseUnlockerDialog _unlocker;

        private string _text;
        private IList<IKeePassEntry> _items;
        private IKeePassDatabase _database;

        public SearchPageViewModel(INavigator navigator, IDatabaseUnlockerDialog unlocker, IClipboard clipboard, IDatabaseTracker tracker)
        {
            _unlocker = unlocker;
            _tracker = tracker;
            _navigator = navigator;

            CopyCommand = new DelegateCommand<string>(clipboard.SetText);
            TextChangedCommand = new DelegateCommand(Update);
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

        public ICommand CopyCommand { get; }

        public ICommand TextChangedCommand { get; }

        private void Update()
        {
            var text = Text;

            Items = _database.Root.EnumerateAllEntriesWithParent()
                .Where(item => FilterEntry(item, text))
                .Select(item => item.Entry)
                .OrderBy(o => o.Title, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        private bool FilterEntry(KeePassEntryWithParent item, string text)
        {
            return Contains(item.Entry.Title, text) || Contains(item.Entry.Notes, text) || Contains(item.Parent.Name, text);
        }

        private bool Contains(string source, string searchText)
        {
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(source, searchText, CompareOptions.IgnoreCase) >= 0;
        }
    }
}
