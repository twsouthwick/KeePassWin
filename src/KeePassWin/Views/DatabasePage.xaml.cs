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
    }
}
