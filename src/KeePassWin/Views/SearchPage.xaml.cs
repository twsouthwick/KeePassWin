using KeePassWin.ViewModels;
using Prism.Windows.Mvvm;

namespace KeePassWin.Views
{
    public sealed partial class SearchPage :  SessionStateAwarePage
    {
        public SearchPageViewModel Model => DataContext as SearchPageViewModel;

        public SearchPage()
        {
            this.InitializeComponent();
        }
    }
}
