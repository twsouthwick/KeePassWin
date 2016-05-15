using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KeePassWin
{
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

        private double WideMinWidth => (double)Application.Current.Resources[nameof(WideMinWidth)];

        public void Dismiss()
        {
            if (ActualWidth < WideMinWidth)
            {
                rootSplitView.IsPaneOpen = false;
            }
        }

        public void Open()
        {
            if (ActualWidth < WideMinWidth)
            {
                rootSplitView.IsPaneOpen = true;
            }
        }
    }
}
