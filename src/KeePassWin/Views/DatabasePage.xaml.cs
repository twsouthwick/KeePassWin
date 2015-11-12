using KeePass.Models;
using KeePassWin.ViewModels;
using Prism.Windows.Mvvm;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.UI.Xaml;

namespace KeePassWin.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DatabasePage : SessionStateAwarePage, INotifyPropertyChanged
    {
        public DatabasePage()
        {
            InitializeComponent();
            DataContextChanged += SecondPage_DataContextChanged;
        }

        protected override void LoadState(object navigationParameter, Dictionary<string, object> pageState)
        {
            base.LoadState(navigationParameter, pageState);

            var database = (IKeePassDatabase)navigationParameter;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DatabaseViewModel ViewDataContext => DataContext as DatabaseViewModel;

        private void SecondPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewDataContext)));
        }
    }
}
