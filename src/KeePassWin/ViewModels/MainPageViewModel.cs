using Prism.Windows.Mvvm;

namespace KeePass.Win.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly INavigationPane _navPane;

        public MainPageViewModel(INavigationPane navPane)
        {
            _navPane = navPane;
        }

        public string DisplayText { get; } = "Select a database to open.";

        public void Open() => _navPane.Open();
    }
}
