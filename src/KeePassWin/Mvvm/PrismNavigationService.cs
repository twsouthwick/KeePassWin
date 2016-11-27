using KeePass.Models;
using Prism.Windows.Navigation;

namespace KeePass.Win.Mvvm
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


        public bool GoToDatabaseView(IKeePassDatabase database, IKeePassGroup group)
        {
            return _navigationService.Navigate("Database", DatabaseGroupParameter.Encode(database.Id, group.Id));
        }

        public bool UnlockDatabase(KeePassId database)
        {
            return _navigationService.Navigate("Database", DatabaseGroupParameter.Encode(database, KeePassId.Empty));
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
