using KeePass.Models;
using KeePass.Win.ViewModels;
using Prism.Windows.Mvvm;
using Windows.UI.Xaml.Navigation;

namespace KeePass.Win.Views
{
    public sealed partial class DatabasePage
    {
        public DatabasePage()
        {
            InitializeComponent();

            Loaded += (_, __) => ItemsList.Focus();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var key = DatabaseGroupParameter.Decode(((string)e.Parameter));

            await Model.SetDatabase(key.Database, key.Group);
        }

        public DatabasePageViewModel Model => DataContext as DatabasePageViewModel;
    }
}
