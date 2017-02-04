using Microsoft.Toolkit.Uwp.UI;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace KeePass.Win.Controls
{
    public sealed partial class SearchBox : UserControl
    {
        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(nameof(Filter), typeof(ICommand), typeof(SearchBox), new PropertyMetadata(null));

        public SearchBox()
        {
            this.InitializeComponent();

            if (DesignMode.DesignModeEnabled == false)
            {
                Loaded += SearchBoxLoaded;
                Unloaded += SearchBoxUnloaded;
            }

            Box.GotFocus += (s, e) => Window.Current.CoreWindow.CharacterReceived += CoreWindowCharacterReceived;
            Box.LostFocus += (s, e) => Window.Current.CoreWindow.CharacterReceived -= CoreWindowCharacterReceived;
        }

        public ICommand Filter
        {
            get { return (ICommand)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        private bool TryClearSearch()
        {
            if (string.IsNullOrEmpty(Box.Text))
            {
                return false;
            }

            Box.ClearValue(AutoSuggestBox.TextProperty);

            return true;
        }

        private void SearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var filter = Filter;
            var text = sender.Text;

            if (filter?.CanExecute(text) == true)
            {
                filter.Execute(text);
            }
        }

        private void SearchBoxUnloaded(object sender, RoutedEventArgs e)
        {
            var frame = this.FindVisualAscendant<Frame>();

            if (frame != null)
            {
                frame.Navigating -= OnFrameNavigating;
            }
        }

        private void SearchBoxLoaded(object sender, RoutedEventArgs e)
        {
            var frame = this.FindVisualAscendant<Frame>();

            if (frame != null)
            {
                frame.Navigating += OnFrameNavigating;
            }
        }

        private void OnFrameNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (TryClearSearch())
            {
                e.Cancel = true;
            }
        }

        private void CoreWindowCharacterReceived(CoreWindow sender, CharacterReceivedEventArgs args)
        {
            if (args.KeyCode == (uint)VirtualKey.Escape)
            {
                args.Handled = TryClearSearch();
            }
        }
    }
}
