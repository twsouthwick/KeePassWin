using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace KeePass.Win.Controls
{
    public class MasterDetailsView : Microsoft.Toolkit.Uwp.UI.Controls.MasterDetailsView
    {
        public static readonly DependencyProperty FooterProperty =
            DependencyProperty.Register(nameof(FooterTemplate), typeof(DataTemplate), typeof(MasterDetailsView), new PropertyMetadata(null));

        public static readonly DependencyProperty FooterContentProperty =
            DependencyProperty.Register(nameof(FooterContent), typeof(object), typeof(MasterDetailsView), new PropertyMetadata(null));

        public DataTemplate FooterTemplate
        {
            get { return (DataTemplate)GetValue(FooterProperty); }
            set { SetValue(FooterProperty, value); }
        }

        public object FooterContent
        {
            get { return GetValue(FooterContentProperty); }
            set { SetValue(FooterContentProperty, value); }
        }

        public bool IsDetailsOpen
        {
            get
            {
                var groups = VisualStateManager.GetVisualStateGroups(VisualTreeHelper.GetChild(this, 0) as FrameworkElement);

                return IsInState(groups, "WidthStates", "NarrowState") && IsInState(groups, "SelectionStates", "HasSelection");
            }
        }

        private bool IsInState(IEnumerable<VisualStateGroup> groups, string groupName, string stateName)
        {
            var currentState = groups.First(g => string.Equals(g.Name, groupName, StringComparison.Ordinal)).CurrentState;

            return string.Equals(currentState?.Name, stateName, StringComparison.Ordinal);
        }

    }
}