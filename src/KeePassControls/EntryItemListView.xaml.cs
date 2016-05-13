using KeePass;
using System;
using System.Collections.Generic;
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

        public static readonly DependencyProperty CopyCommandProperty =
            DependencyProperty.Register(nameof(CopyCommand), typeof(ICommand), typeof(EntryItemListView), new PropertyMetadata(null));

        private void ListViewItemClick(object sender, ItemClickEventArgs e)
        {
            if (Command?.CanExecute(e.ClickedItem) == true)
            {
                Command.Execute(e.ClickedItem);
            }

        }

        private void CopyPassword(object sender, RoutedEventArgs e)
        {
            Copy((((AppBarButton)sender).DataContext as IKeePassEntry)?.Password);
        }

        private void CopyUserName(object sender, RoutedEventArgs e)
        {
            Copy((((AppBarButton)sender).DataContext as IKeePassEntry)?.UserName);
        }

        private void Copy(string text)
        {
            if(CopyCommand?.CanExecute(text) == true)
            {
                CopyCommand.Execute(text);
            }
        }
    }
}
