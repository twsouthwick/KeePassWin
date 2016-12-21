using KeePass.Win.Controls;
using KeePass.Win.ViewModels;
using Prism.Windows.Mvvm;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace KeePass.Win.Views
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
                if (ItemsList.ViewState == MasterDetailsViewState.Both)
                {
                    e.Cancel = true;
                }
                else if (Model.TryClearSearch())
                {
                    e.Cancel = true;
                    ClearSearch();
                }
            }

            base.OnNavigatingFrom(e);
        }

        public DatabasePageViewModel Model => DataContext as DatabasePageViewModel;

        #region Search functionality
        private AutoSuggestBox _autoSuggestBox;

        public void ClearSearch()
        {
            _autoSuggestBox?.ClearValue(AutoSuggestBox.TextProperty);
        }

        private void SearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Store so we can clear the search if needed
            _autoSuggestBox = sender;

            var text = sender.Text;

            if (text.Length < 3)
            {
                Model.TryClearSearch();
                return;
            }

            Model.Search(text);
        }
        #endregion
    }
}
