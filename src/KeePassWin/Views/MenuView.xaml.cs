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

            Version = File.ReadAllText("version.txt").Trim();
        }

        public MenuViewModel Model => DataContext as MenuViewModel;

        public string Version { get; set; }
    }
}
