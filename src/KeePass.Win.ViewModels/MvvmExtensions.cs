using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KeePass.Win.ViewModels
{
    internal static class MvvmExtensions
    {
        public static void RaiseCanExecuteChanged(this ICommand command)
        {
            (command as DelegateCommand)?.RaiseCanExecuteChanged();
        }
    }
}
