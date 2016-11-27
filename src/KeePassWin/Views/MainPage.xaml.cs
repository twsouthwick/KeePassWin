using KeePass.Win.ViewModels;
using Prism.Windows.Mvvm;

namespace KeePass.Win.Views
{
    public sealed partial class MainPage : SessionStateAwarePage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public MainPageViewModel ViewDataContext => DataContext as MainPageViewModel;
    }
}
