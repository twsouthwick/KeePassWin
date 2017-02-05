using KeePass.Win.Mvvm;
using KeePass.Win.ViewModels;
using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KeePass.Win.Views
{
    public sealed partial class SettingsPage
    {
        private static readonly KeyboardCapabilities s_capabilities = new KeyboardCapabilities();

        public SettingsPage()
        {
            this.InitializeComponent();

            LogText.RegisterPropertyChangedCallback(TextBlock.TextProperty, LogUpdated);

            Loaded += SettingsPageLoaded;
        }

        [Inject]
        public SettingsPageViewModel Model { get; set; }

        private void SettingsPageLoaded(object sender, RoutedEventArgs e)
        {
            if (s_capabilities.KeyboardPresent == 0)
            {
                Pivots.Items.Remove(KeyboardPivot);
            }
        }

        private void LogUpdated(DependencyObject sender, DependencyProperty dp)
        {
            LogScroll.ChangeView(null, LogScroll.ScrollableHeight, null);
        }
    }
}
