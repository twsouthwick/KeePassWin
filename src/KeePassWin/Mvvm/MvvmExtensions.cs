using System.Diagnostics;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KeePassWin.Mvvm
{
    public class MvvmExtensions
    {

        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(MvvmExtensions), new PropertyMetadata(null, CommandPropertyChanged));


        public static void SetCommand(DependencyObject attached, ICommand value)
        {
            attached.SetValue(CommandProperty, value);
        }


        public static ICommand GetCommand(DependencyObject attached)
        {
            return (ICommand)attached.GetValue(CommandProperty);
        }


        private static void CommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Debug.Assert(d is ListViewBase);

            // Attach click handler
            (d as ListViewBase).ItemClick += ItemClick;
        }


        private static void ItemClick(object sender, ItemClickEventArgs e)
        {
            Debug.Assert(sender is DependencyObject);

            var command = GetCommand(sender as DependencyObject);

            command.Execute(e.ClickedItem);
        }
    }
}
