using KeePass;
using KeePassWin.ViewModels;
using Prism.Windows.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace KeePassWin.Views
{
    public sealed partial class DatabasePage : SessionStateAwarePage
    {
        public DatabasePage()
        {
            InitializeComponent();
        }

        private bool IsInState(IEnumerable<VisualStateGroup> groups, string groupName, string stateName)
        {
            var currentState = groups.First(g => string.Equals(g.Name, groupName, StringComparison.Ordinal)).CurrentState;

            return string.Equals(currentState?.Name, stateName, StringComparison.Ordinal);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (!e.Cancel && e.NavigationMode == NavigationMode.Back)
            {
                var groups = VisualStateManager.GetVisualStateGroups(VisualTreeHelper.GetChild(ItemsList, 0) as FrameworkElement);

                // This is a way to determine if the details is open in the narrow state; if so, we should not handle the back event
                if (IsInState(groups, "WidthStates", "NarrowState") && IsInState(groups, "SelectionStates", "HasSelection"))
                {
                    e.Cancel = true;
                }
                else if (Model.TryClearSearch())
                {
                    e.Cancel = true;
                    ClearSearch();
                }
            }

            base.OnNavigatingFrom(e);
        }

        public DatabasePageViewModel Model => DataContext as DatabasePageViewModel;

        private void MasterDetailsViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var groups = e.AddedItems.OfType<IKeePassGroup>();

            foreach (var group in groups)
            {
                if (Model.ItemClickCommand?.CanExecute(group) == true)
                {
                    Model.ItemClickCommand.Execute(group);
                }
            }
        }

        #region Search functionality
        private AutoSuggestBox _autoSuggestBox;

        public void ClearSearch()
        {
            _autoSuggestBox?.ClearValue(AutoSuggestBox.TextProperty);
        }

        private void SearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Store so we can clear the search if needed
            _autoSuggestBox = sender;

            var text = sender.Text;

            if (text.Length < 3)
            {
                Model.TryClearSearch();
                return;
            }

            Model.Search(text);
        }
        #endregion
    }
}
