using System.Windows.Input;

namespace KeePass.Win.ViewModels
{
    public class LoggingPageViewModel
    {
        public LoggingPageViewModel(ILogView logView, IClipboard<ILogView> clipboard, IMailClient<ILogView> mailClient)
        {
            LogView = logView;

            SendCommand = new DelegateCommand(() => mailClient.SendAsync(LogView));
            CopyCommand = new DelegateCommand(() => clipboard.Copy(LogView));
        }

        public ILogView LogView { get; }

        public ICommand SendCommand { get; }

        public ICommand CopyCommand { get; }
    }
}
