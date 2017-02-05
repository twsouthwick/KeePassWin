using KeePass.Models;
using Prism.Windows.Navigation;
using System;

namespace KeePass.Win.Mvvm
{
    internal class PrismNavigationService : INavigator
    {
        private readonly INavigationService _navigationService;
        private readonly Lazy<AppShell> _shell;

        public PrismNavigationService(INavigationService navigationService, Lazy<AppShell> shell)
        {
            _navigationService = navigationService;
            _shell = shell;
        }

        public void GoBack()
        {
            _navigationService.GoBack();
        }

        public bool GoToDatabaseView(IKeePassDatabase database, IKeePassGroup group)
        {
            _shell.Value.Dismiss();
            return _navigationService.Navigate("Database", DatabaseGroupParameter.Encode(database.Id, group.Id));
        }

        public bool GoToMain()
        {
            _shell.Value.Open();
            return _navigationService.Navigate("Main", null);
        }

        public bool GoToSettings()
        {
            _shell.Value.Dismiss();
            return _navigationService.Navigate("Settings", null);
        }
    }
}
