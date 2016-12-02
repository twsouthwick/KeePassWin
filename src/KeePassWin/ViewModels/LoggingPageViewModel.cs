using KeePass.Win.Log;
using Prism.Commands;
using Prism.Mvvm;
using System.Windows.Input;

namespace KeePass.Win.ViewModels
{
    public class LoggingPageViewModel : BindableBase
    {
        public LoggingPageViewModel(ILogView logView, IClipboard<ILogView> clipboard, IMailClient<ILogView> mailClient)
        {
            LogView = logView;

            SendCommand = new DelegateCommand(() => mailClient.SendAsync(LogView));
            CopyCommand = new DelegateCommand(() => clipboard.Copy(LogView));

            Statement = LocalizedStrings.SettingsLogDescription;
        }

        public ILogView LogView { get; }

        public string Statement { get; }

        public ICommand SendCommand { get; }

        public ICommand CopyCommand { get; }
    }
}
