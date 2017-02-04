using KeePass.Models;
using KeePass.Win.Mvvm;
using KeePass.Win.ViewModels;
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

        [Inject]
        public DatabasePageViewModel Model { get; set; }
    }
}
