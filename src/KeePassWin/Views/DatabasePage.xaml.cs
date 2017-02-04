using KeePass.Win.ViewModels;
using Prism.Windows.Mvvm;

namespace KeePass.Win.Views
{
    public sealed partial class DatabasePage : SessionStateAwarePage
    {
        public DatabasePage()
        {
            InitializeComponent();

            Loaded += (_, __) => ItemsList.Focus();
        }

        public DatabasePageViewModel Model => DataContext as DatabasePageViewModel;
    }
}
