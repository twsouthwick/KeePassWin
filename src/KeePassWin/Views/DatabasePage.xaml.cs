using KeePass;
using KeePassWin.ViewModels;
using Prism.Windows.Mvvm;
using System.ComponentModel;
using Windows.UI.Xaml;

namespace KeePassWin.Views
{
    public sealed partial class DatabasePage : SessionStateAwarePage, INotifyPropertyChanged
    {
        public DatabasePage()
        {
            InitializeComponent();
            DataContextChanged += SecondPage_DataContextChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DatabasePageViewModel Model => DataContext as DatabasePageViewModel;

        private void SecondPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Model)));
        }

        private void TreeViewSelectedItemChanged(object sender, WinRTXamlToolkit.Controls.RoutedPropertyChangedEventArgs<object> e)
        {
            Model.UpdateItems(e.NewValue as IKeePassGroup);
        }
    }
}
