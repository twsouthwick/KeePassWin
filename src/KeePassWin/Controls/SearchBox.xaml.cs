using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Reactive.Linq;
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

            var changes = Observable.FromEventPattern<AutoSuggestBox, AutoSuggestBoxTextChangedEventArgs>(Box, nameof(Box.TextChanged))
                .Select(t => t.Sender.Text)
                .Where(text => text.Length >= 3)
                .DistinctUntilChanged()
                .Throttle(TimeSpan.FromMilliseconds(300))
                .ObserveOnDispatcher(CoreDispatcherPriority.Low)
                .Subscribe(text =>
                {
                    var filter = Filter;

                    if (filter?.CanExecute(text) == true)
                    {
                        filter.Execute(text);
                    }
                });

            Box.Unloaded += (s, e) =>
            {
                changes.Dispose();
            };

            void CoreWindowCharacterReceived(CoreWindow sender, CharacterReceivedEventArgs args)
            {
                if (args.KeyCode == (uint)VirtualKey.Escape)
                {
                    args.Handled = TryClearSearch();
                }
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

        private void SearchBoxUnloaded(object sender, RoutedEventArgs e)
        {
            var frame = this.FindAscendant<Frame>();

            if (frame != null)
            {
                frame.Navigating -= OnFrameNavigating;
            }
        }

        private void SearchBoxLoaded(object sender, RoutedEventArgs e)
        {
            var frame = this.FindAscendant<Frame>();

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
    }
}
