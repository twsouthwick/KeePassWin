using KeePassWin.ViewModels;
using Prism.Windows.Mvvm;
using Windows.UI.Xaml.Controls;

namespace KeePassWin.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchPage :  SessionStateAwarePage
    {
        
        public SearchPageViewModel Model => DataContext as SearchPageViewModel;

        public SearchPage()
        {
            this.InitializeComponent();
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            Model.Update();
        }
    }
}
