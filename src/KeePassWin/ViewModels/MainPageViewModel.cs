using Prism.Windows.Mvvm;

namespace KeePassWin.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public string DisplayText { get; } = "Select a database to open.";
    }
}
