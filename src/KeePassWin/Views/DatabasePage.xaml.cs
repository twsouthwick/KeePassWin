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
                if (ItemList.DetailsOpen)
                {
                    e.Cancel = true;
                }
                else if(Model.TryClearSearch())
                {
                    e.Cancel = true;
                    ItemList.ClearSearch();
                }
            }

            base.OnNavigatingFrom(e);
        }

        public DatabasePageViewModel Model => DataContext as DatabasePageViewModel;
    }
}
