using KeePassWin.ViewModels;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KeePassWin.Views
{
    public sealed partial class MenuView : UserControl, INotifyPropertyChanged
    {
        public MenuView()
        {
            InitializeComponent();
            DataContextChanged += MenuControl_DataContextChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MenuViewModel ConcreteDataContext => DataContext as MenuViewModel;

        private void MenuControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConcreteDataContext)));
        }
    }
}
