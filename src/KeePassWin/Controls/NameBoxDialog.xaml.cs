using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace KeePass.Win.Controls
{
    public sealed partial class NameBoxDialog : ContentDialog, INameProvider, IContentDialogResult
    {
        private string _text;

        public NameBoxDialog()
        {
            this.InitializeComponent();
        }

        public async Task<string> GetNameAsync()
        {
            await ShowAsync();

            return _text;
        }

        public void Enter()
        {
            _text = TextInputBox.Text;
        }

        public void Escape()
        {
            _text = null;
        }

        private void CancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) => Escape();

        private void CreateButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) => Enter();
    }
}
