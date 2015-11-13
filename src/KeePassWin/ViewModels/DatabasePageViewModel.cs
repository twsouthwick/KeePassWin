using KeePass.Models;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System.Collections.Generic;

namespace KeePassWin.ViewModels
{
    public class DatabasePageViewModel : ViewModelBase
    {
        private IKeePassDatabase _database;

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            Database = (IKeePassDatabase)e.Parameter;
        }

        public IKeePassDatabase Database
        {
            get { return _database; }
            set { SetProperty(ref _database, value); }
        }
    }
}
