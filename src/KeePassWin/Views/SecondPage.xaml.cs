using Prism.Windows.Mvvm;
using KeePassWin.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
namespace KeePassWin.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SecondPage : SessionStateAwarePage, INotifyPropertyChanged
    {
        public SecondPage()
        {
            InitializeComponent();
            DataContextChanged += SecondPage_DataContextChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SecondPageViewModel ViewDataContext => DataContext as SecondPageViewModel;

        private void SecondPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewDataContext)));
        }
    }
}
