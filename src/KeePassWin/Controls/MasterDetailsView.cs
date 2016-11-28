using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace KeePass.Win.Controls
{
    public class MasterDetailsView : Microsoft.Toolkit.Uwp.UI.Controls.MasterDetailsView
    {
        public static readonly DependencyProperty FooterTemplateProperty =
            DependencyProperty.Register(nameof(FooterTemplate), typeof(DataTemplate), typeof(MasterDetailsView), new PropertyMetadata(null));

        public static readonly DependencyProperty FooterContentProperty =
            DependencyProperty.Register(nameof(FooterContent), typeof(object), typeof(MasterDetailsView), new PropertyMetadata(null));

        public static readonly DependencyProperty ItemClickCommandProperty =
            DependencyProperty.Register(nameof(ItemClickCommand), typeof(ICommand), typeof(MasterDetailsView), new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedMasterItemProperty =
            DependencyProperty.Register(nameof(SelectedMasterItem), typeof(object), typeof(MasterDetailsView), new PropertyMetadata(null, ListSelectedItemPropertyChanged));

        public object SelectedMasterItem
        {
            get { return (object)GetValue(SelectedMasterItemProperty); }
            set { SetValue(SelectedMasterItemProperty, value); }
        }

        public DataTemplate FooterTemplate
        {
            get { return (DataTemplate)GetValue(FooterTemplateProperty); }
            set { SetValue(FooterTemplateProperty, value); }
        }

        public object FooterContent
        {
            get { return GetValue(FooterContentProperty); }
            set { SetValue(FooterContentProperty, value); }
        }

        public ICommand ItemClickCommand
        {
            get { return (ICommand)GetValue(ItemClickCommandProperty); }
            set { SetValue(ItemClickCommandProperty, value); }
        }

        public bool IsDetailsOpen
        {
            get
            {
                var groups = VisualStateManager.GetVisualStateGroups(VisualTreeHelper.GetChild(this, 0) as FrameworkElement);

                return IsInState(groups, "WidthStates", "NarrowState") && IsInState(groups, "SelectionStates", "HasSelection");
            }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var list = (ListView)GetTemplateChild("MasterList");
            list.ItemClick -= ListItemClick;
            list.ItemClick += ListItemClick;
        }

        private bool IsInState(IEnumerable<VisualStateGroup> groups, string groupName, string stateName)
        {
            var currentState = groups.First(g => string.Equals(g.Name, groupName, StringComparison.Ordinal)).CurrentState;

            return string.Equals(currentState?.Name, stateName, StringComparison.Ordinal);
        }

        private static void ListSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (MasterDetailsView)d;

            if (view.ItemClickCommand?.CanExecute(e.NewValue) == false)
            {
                view.SelectedItem = e.NewValue;
            }
            else
            {
                view.SelectedItem = null;
            }
        }

        private void ListItemClick(object sender, ItemClickEventArgs e)
        {
            var command = ItemClickCommand;
            var newItem = e.ClickedItem;

            if (command?.CanExecute(newItem) == true)
            {
                command.Execute(newItem);
            }
            else
            {
                SelectedItem = newItem;
            }
        }
    }
}