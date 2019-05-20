using System;
using System.Threading.Tasks;
using KeePass.Models;
using KeePass.Win.Mvvm;
using KeePass.Win.ViewModels;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace KeePass.Win.Views
{
    public sealed partial class DatabasePage
    {
        private DatabasePageViewModel _model;
        private string _parameter;

        public DatabasePage()
        {
            InitializeComponent();

            Loaded += (s, e) => ItemsList.Focus();

            Application.Current.Suspending += OnAppSuspending;
            Application.Current.Resuming += OnAppResuming;
        }

        private async void OnAppResuming(object sender, object e)
        {
            await LoadAsync(_parameter);
        }

        private void OnAppSuspending(object sender, SuspendingEventArgs e)
        {
            Model.ClearDatabase();
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
            _parameter = (string)e.Parameter;
            await LoadAsync(_parameter);
        }

        private Task LoadAsync(string parameter)
        {
            var key = DatabaseGroupParameter.Decode(parameter);

            return Model.SetDatabaseAsync(key.Database, key.Group);
        }
    }
}
