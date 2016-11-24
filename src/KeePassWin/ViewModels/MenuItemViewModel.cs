using Prism.Windows.Mvvm;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace KeePass.Win.ViewModels
{
    public class MenuItemViewModel : ViewModelBase
    {
        public string DisplayName { get; set; }

        public Symbol FontIcon { get; set; }

        public ICommand Command { get; set; }

        public ICommand RemoveCommand { get; set; }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
