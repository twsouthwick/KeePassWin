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

        public bool GoToSettings()
        {
            return _navigationService.Navigate("Settings", null);
        }
    }
}
