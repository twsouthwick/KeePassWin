using KeePass.Win.ViewModels;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KeePass.Win
{
    public sealed partial class AppShell : Page
    {
        public AppShell(MenuViewModel model)
        {
            InitializeComponent();

            Model = model;
        }

        public MenuViewModel Model { get; }

        public void SetContentFrame(Frame frame)
        {
            Menu.Content = frame;
        }

        public void Dismiss()
        {
            Menu.IsPaneOpen = false;
        }

        public void Open()
        {
            Menu.IsPaneOpen = true;
        }

        // MVVM is not used here as the style became inconsistent within the hamburger menu with a Button control
        private void HamburgerMenuItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (MenuItemViewModel)e.ClickedItem;

            item.Command?.Execute(null);
        }

        // MVVM is not used here as using a Button control was causing a COMException in the hamburger control
        private void HamburgerMenuOptionsItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (HamburgerMenuGlyphCommandItem)e.ClickedItem;

            item.Command?.Execute(null);
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class HamburgerMenuGlyphCommandItem : HamburgerMenuGlyphItem
#pragma warning restore SA1402 // File may only contain a single class
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(HamburgerMenuItem), new PropertyMetadata(null));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
    }
}
