using KeePassWin.ViewModels;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KeePassWin
{
    public sealed partial class AppShell : Page
    {
        public MenuViewModel Model { get; }

        public AppShell(MenuViewModel model)
        {
            InitializeComponent();

            Model = model;
        }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>MVVM is not used here as the style became inconsistent within the hamburger menu with a Button control</remarks>
        private void HamburgerMenuItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (MenuItemViewModel)e.ClickedItem;

            item.Command?.Execute(null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>MVVM is not used here as using a Button control was causing a COMException in the hamburger control</remarks>
        private void HamburgerMenuOptionsItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (HamburgerMenuGlyphCommandItem)e.ClickedItem;

            item.Command?.Execute(null);
        }
    }

    public class HamburgerMenuGlyphCommandItem : HamburgerMenuGlyphItem
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(HamburgerMenuItem), new PropertyMetadata(null));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
    }
}
