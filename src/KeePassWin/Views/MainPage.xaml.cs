using KeePassWin.ViewModels;
using Prism.Windows.Mvvm;
using System.ComponentModel;
using Windows.UI.Xaml;

namespace KeePassWin.Views
{
    public sealed partial class MainPage : SessionStateAwarePage, INotifyPropertyChanged
    {
        public MainPage()
        {
            InitializeComponent();
            DataContextChanged += MainPage_DataContextChanged;

            Loaded += (_, __) => ViewDataContext.Open();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainPageViewModel ViewDataContext => DataContext as MainPageViewModel;

        private void MainPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewDataContext)));
        }
    }
}
