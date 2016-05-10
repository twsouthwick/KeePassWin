using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KeePassWin
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AppShell : Page
    {
        public AppShell()
        {
            InitializeComponent();
        }

        public void SetContentFrame(Frame frame)
        {
            rootSplitView.Content = frame;
        }

        public void SetMenuPaneContent(UIElement content)
        {
            rootSplitView.Pane = content;
        }

        public void Dismiss()
        {
            // TODO: This should be dependent on the wide view state of the shell
            if (ActualWidth < 1280)
            {
                rootSplitView.IsPaneOpen = false;
            }
        }

        public void Open()
        {
            // TODO: This should be dependent on the wide view state of the shell
            if (ActualWidth < 1280)
            {
                rootSplitView.IsPaneOpen = true;
            }
        }
    }
}
