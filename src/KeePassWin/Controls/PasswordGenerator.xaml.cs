using KeePass.Win.Mvvm;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KeePass.Win.Controls
{
    public sealed partial class PasswordGenerator : UserControl
    {
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register(nameof(Password), typeof(string), typeof(PasswordGenerator), new PropertyMetadata(null));

        public PasswordGenerator()
        {
            this.InitializeComponent();
        }

        [Inject]
        public KeePass.PasswordGenerator Generator { get; set; }

        [Inject]
        public PasswordGeneratorSettings Settings { get; set; }

        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        private void GenerateClick(object sender, RoutedEventArgs e)
        {
            var generated = Generator.Generate(Settings);

            if (!string.IsNullOrEmpty(generated))
            {
                Password = generated;
            }
        }
    }
}
