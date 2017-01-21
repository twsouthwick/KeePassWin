using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace KeePass.Win.Controls
{
    public class MasterDetailsView : Microsoft.Toolkit.Uwp.UI.Controls2.MasterDetailsView
    {
        private ContentPresenter _detailsPresenter;
        private ListView _list;

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


        public void Focus()
        {
            if (ViewState != Microsoft.Toolkit.Uwp.UI.Controls2.MasterDetailsViewState.Details)
            {
                _list.Focus(FocusState.Keyboard);
            }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _list = (ListView)GetTemplateChild("MasterList");
            _detailsPresenter = (ContentPresenter)GetTemplateChild("DetailsPresenter");
            _list.ItemClick -= ListItemClick;
            _list.ItemClick += ListItemClick;
        }

        private static void ListSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (MasterDetailsView)d;

            if (view.ViewState == Microsoft.Toolkit.Uwp.UI.Controls2.MasterDetailsViewState.Both && view.ItemClickCommand?.CanExecute(e.NewValue) == false)
            {
                view.SelectedItem = e.NewValue;
            }
            else
            {
                view.SelectedItem = null;
                view.Focus();
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
                // Set master item first so it won't be called later; it will set SelectedItem to null, so we set it manually as well
                SelectedMasterItem = e.ClickedItem;
                SelectedItem = newItem;
                FocusSelection();
            }

            UpdateView(true);
        }

        private void FocusSelection()
        {
            var control = VisualTreeHelper.GetChild(_detailsPresenter, 0) as ContentControl;

            // Focus if already loaded
            (control.ContentTemplateRoot as Control)?.Focus(FocusState.Keyboard);

            // Focus when loaded if not
            control.Loaded += (s, _) =>
            {
                var root = (s as ContentControl).ContentTemplateRoot;
                (root as Control)?.Focus(FocusState.Keyboard);
            };
        }
    }
}