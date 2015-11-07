using Prism.Windows.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeePassWin.ViewModels
{
    public class SecondPageViewModel : ViewModelBase
    {
        public SecondPageViewModel()
        {
            DisplayText = "This is the second page!";
        }

        public string DisplayText { get; private set; }
    }
}
