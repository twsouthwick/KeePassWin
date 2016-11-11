using KeePassWin.ViewModels;
using System.IO;
using Windows.UI.Xaml.Controls;

namespace KeePassWin.Views
{
    public sealed partial class MenuView : UserControl
    {
        public MenuView()
        {
            InitializeComponent();
        }

        public MenuViewModel Model => DataContext as MenuViewModel;
    }
}
