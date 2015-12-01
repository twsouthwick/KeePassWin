using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;

namespace KeePass.IO
{
    public class TimedClipboard : IClipboard
    {
        private readonly TimeSpan _delay;

        public TimedClipboard(TimeSpan delay)
        {
            _delay = delay;
        }

        public void SetText(string text)
        {
            var dp = new DataPackage();

            dp.SetText(text);

            Clipboard.SetContent(dp);

            ClearTextAsync();
        }

        public void ClearTextAsync()
        {
            Task.Run(async () =>
            {
                await Task.Delay(_delay);

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, Clipboard.Clear);
            });
        }
    }
}
