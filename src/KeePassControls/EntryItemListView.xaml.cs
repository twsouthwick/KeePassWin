using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KeePass.Controls
{
    public sealed partial class EntryItemListView : UserControl
    {
        public EntryItemListView()
        {
            this.InitializeComponent();
        }

        public object ItemsSource
        {
            get { return GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(EntryItemListView), new PropertyMetadata(Array.Empty<IKeePassId>()));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(EntryItemListView), new PropertyMetadata(null));

        public ICommand CopyCommand
        {
            get { return (ICommand)GetValue(CopyCommandProperty); }
            set { SetValue(CopyCommandProperty, value); }
        }

        public ICommand RemoveEntryCommand
        {
            get { return (ICommand)GetValue(RemoveEntryCommandProperty); }
            set { SetValue(RemoveEntryCommandProperty, value); }
        }

        public static readonly DependencyProperty CopyCommandProperty =
            DependencyProperty.Register(nameof(CopyCommand), typeof(ICommand), typeof(EntryItemListView), new PropertyMetadata(null));

        public static readonly DependencyProperty RemoveEntryCommandProperty =
            DependencyProperty.Register(nameof(RemoveEntryCommand), typeof(ICommand), typeof(EntryItemListView), new PropertyMetadata(null));

        private void ListViewItemClick(object sender, ItemClickEventArgs e)
        {
            if (Command?.CanExecute(e.ClickedItem) == true)
            {
                Command.Execute(e.ClickedItem);
            }
        }

        private async void GoToWebsite(object sender, RoutedEventArgs e)
        {
            var entry = GetEntryFromFrameworkElement(sender);
            Uri uri;

            if (Uri.TryCreate(entry.Url, UriKind.Absolute, out uri))
            {
                if (!(await Windows.System.Launcher.LaunchUriAsync(uri)))
                {
                    var dialog = new Windows.UI.Popups.MessageDialog($"Could not launch {entry.Url}");
                }
            }
        }

        private void CopyPassword(object sender, RoutedEventArgs e)
        {
            Copy(GetEntryFromFrameworkElement(sender)?.Password);
        }

        private void CopyUserName(object sender, RoutedEventArgs e)
        {
            Copy(GetEntryFromFrameworkElement(sender)?.UserName);
        }

        private static IKeePassEntry GetEntryFromFrameworkElement(object sender)
        {
            return ((FrameworkElement)sender).DataContext as IKeePassEntry;
        }

        private void Copy(string text)
        {
            if (CopyCommand?.CanExecute(text) == true)
            {
                CopyCommand.Execute(text);
            }
        }

        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            var entry = GetEntryFromFrameworkElement(sender);

            if (RemoveEntryCommand?.CanExecute(entry) == true)
            {
                RemoveEntryCommand.Execute(entry);
            }
        }

        private void EditItem(object sender, RoutedEventArgs e)
        {
            var entry = GetEntryFromFrameworkElement(sender);

            if (Command?.CanExecute(entry) == true)
            {
                Command.Execute(entry);
            }
        }
    }
}
