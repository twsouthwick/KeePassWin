using System;
using KeePass;
using KeePass.Models;
using Prism.Windows.Navigation;

namespace KeePassWin.Mvvm
{
    internal class PrismNavigationService : INavigator
    {
        private readonly INavigationService _navigationService;

        public PrismNavigationService(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public void CreateNewDatabase()
        {
            _navigationService.Navigate("NewDatabase", null);
        }

        public void GoBack()
        {
            _navigationService.GoBack();
        }

        public bool GoToDatabaseView(KeePassId database, KeePassId group)
        {
            return _navigationService.Navigate("Database", DatabaseGroupParameter.Encode(database, group));
        }

        public bool GoToMain()
        {
            return _navigationService.Navigate("Main", null);
        }

        public bool GoToSearch(KeePassId database, string text)
        {
            return _navigationService.Navigate("Search", SearchQueryParameter.Encode(database, text));
        }
    }
}
