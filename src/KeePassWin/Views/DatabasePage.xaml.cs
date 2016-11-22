using KeePassWin.ViewModels;
using Prism.Windows.Mvvm;
using Windows.UI.Xaml.Navigation;

namespace KeePassWin.Views
{
    public sealed partial class DatabasePage : SessionStateAwarePage
    {
        public DatabasePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (!e.Cancel && e.NavigationMode == NavigationMode.Back)
            {
                e.Cancel = ItemList.DetailsOpen;
            }

            base.OnNavigatingFrom(e);
        }

        public DatabasePageViewModel Model => DataContext as DatabasePageViewModel;
    }
}
