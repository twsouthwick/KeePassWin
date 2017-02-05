using System.Windows.Input;

namespace KeePass.Win.ViewModels
{
    public class MenuItemViewModel
    {
        public string DisplayName { get; set; }

        public ICommand Command { get; set; }

        public ICommand RemoveCommand { get; set; }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
