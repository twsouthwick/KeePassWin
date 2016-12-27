using Windows.UI.Xaml;

namespace KeePass.Win.Mvvm
{
    public class ComboBox
    {
        public static readonly DependencyProperty ValueTypeProperty = DependencyProperty.RegisterAttached("ValueType", typeof(bool), typeof(Keyboard), new PropertyMetadata(default(bool), ValueTypePropertyChanged));

        public static void SetValueType(Windows.UI.Xaml.Controls.ComboBox attached, bool value)
        {
            attached.SetValue(ValueTypeProperty, value);
        }

        public static bool GetValueType(Windows.UI.Xaml.Controls.ComboBox attached)
        {
            return (bool)attached.GetValue(ValueTypeProperty);
        }

        private static void ValueTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                return;
            }

            var control = (Windows.UI.Xaml.Controls.ComboBox)d;

            control.Loaded += (s, _) =>
            {
                var combobox = (Windows.UI.Xaml.Controls.ComboBox)s;
                var value = combobox.SelectedValue;

                foreach (var item in combobox.Items)
                {
                    if (Equals(item, value))
                    {
                        combobox.SelectedItem = item;
                        return;
                    }
                }
            };
        }
    }
}
