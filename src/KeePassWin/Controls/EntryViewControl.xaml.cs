using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KeePass.Win.Controls
{
    public sealed partial class EntryViewControl : UserControl
    {
        public static readonly DependencyProperty EntryProperty =
            DependencyProperty.Register(nameof(Entry), typeof(IKeePassEntry), typeof(EntryViewControl), new PropertyMetadata(null));

        public static readonly DependencyProperty CopyCommandProperty =
            DependencyProperty.Register(nameof(CopyCommand), typeof(ICommand), typeof(EntryViewControl), new PropertyMetadata(null));

        public static readonly DependencyProperty SaveCommandProperty =
            DependencyProperty.Register(nameof(SaveCommand), typeof(ICommand), typeof(EntryViewControl), new PropertyMetadata(null));

        public EntryViewControl()
        {
            this.InitializeComponent();

            Loaded += EntryViewControlLoaded;
        }

        private void EntryViewControlLoaded(object sender, RoutedEventArgs e)
        {
            Focus(FocusState.Keyboard);
        }

        public IKeePassEntry Entry
        {
            get { return (IKeePassEntry)GetValue(EntryProperty); }
            set { SetValue(EntryProperty, value); }
        }

        public ICommand CopyCommand
        {
            get { return (ICommand)GetValue(CopyCommandProperty); }
            set { SetValue(CopyCommandProperty, value); }
        }

        public ICommand SaveCommand
        {
            get { return (ICommand)GetValue(SaveCommandProperty); }
            set { SetValue(SaveCommandProperty, value); }
        }

        private void PasswordChecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;

            passwordBox.PasswordRevealMode = checkBox.IsChecked == true ? PasswordRevealMode.Visible : PasswordRevealMode.Hidden;
        }
    }
}




