using KeePass.Win.ViewModels;
using Prism.Windows.Mvvm;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KeePass.Win.Views
{
    public sealed partial class SettingsPage : SessionStateAwarePage
    {
        public SettingsPageViewModel Model => DataContext as SettingsPageViewModel;

        public SettingsPage()
        {
            this.InitializeComponent();

            LogText.RegisterPropertyChangedCallback(TextBlock.TextProperty, LogUpdated);
        }

        private void LogUpdated(DependencyObject sender, DependencyProperty dp)
        {
            LogScroll.ChangeView(null, LogScroll.ScrollableHeight, null);
        }
    }
}
