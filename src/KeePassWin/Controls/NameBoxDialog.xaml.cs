using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KeePass.Win.Controls
{
    public sealed partial class NameBoxDialog : ContentDialog, INameProvider, IContentDialogResult
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(NameBoxDialog), new PropertyMetadata(null));

        public NameBoxDialog()
        {
            this.InitializeComponent();
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public async Task<string> GetNameAsync(string initial = null)
        {
            Text = initial;

            if (initial != null)
            {
                SecondaryButtonText = "Rename";
            }

            await ShowAsync();

            return Text;
        }

        public void Enter()
        {
            Text = TextInputBox.Text;
        }

        public void Escape()
        {
            Text = null;
        }

        private void CancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) => Escape();

        private void CreateButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) => Enter();
    }
}
