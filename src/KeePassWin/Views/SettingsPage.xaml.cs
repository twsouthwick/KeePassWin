using KeePassWin.ViewModels;
using Prism.Windows.Mvvm;

namespace KeePassWin.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage :  SessionStateAwarePage
    {

        public SettingsPageViewModel Model => DataContext as SettingsPageViewModel;

        public SettingsPage()
        {
            this.InitializeComponent();
        }
    }
}
