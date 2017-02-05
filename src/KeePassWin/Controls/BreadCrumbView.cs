using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KeePass.Win.Controls
{
    public class BreadCrumbView : ItemsControl
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(BreadCrumbView), new PropertyMetadata(null));

        public BreadCrumbView()
        {
            DefaultStyleKey = typeof(BreadCrumbView);
            DataContext = this;
            IsTabStop = false;
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
    }
}
