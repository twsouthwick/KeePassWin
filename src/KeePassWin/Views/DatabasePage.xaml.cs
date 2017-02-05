using KeePass.Models;
using KeePass.Win.Mvvm;
using KeePass.Win.ViewModels;
using Windows.UI.Xaml.Navigation;

namespace KeePass.Win.Views
{
    public sealed partial class DatabasePage
    {
        private DatabasePageViewModel _model;

        public DatabasePage()
        {
            InitializeComponent();

            Loaded += (s, e) => ItemsList.Focus();
        }

        [Inject]
        public DatabasePageViewModel Model
        {
            get
            {
                return _model;
            }

            set
            {
                _model = value;
                DataContext = value;
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var key = DatabaseGroupParameter.Decode((string)e.Parameter);

            await Model.SetDatabase(key.Database, key.Group);
        }
    }
}
