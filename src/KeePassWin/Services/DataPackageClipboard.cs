using KeePass.Win.Log;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Email;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;

namespace KeePass.Win.Services
{
    public class DataPackageClipboard : IClipboard<string>, IClipboard<ILogView>, IMailClient<ILogView>
    {
        private readonly KeePassSettings _settings;

        private CancellationTokenSource _cts;

        public DataPackageClipboard(KeePassSettings settings)
        {
            _settings = settings;
        }

        public virtual bool Copy(string text)
        {
            if (text == null)
            {
                return false;
            }

            var dp = new DataPackage();

            dp.SetText(text);

            AddToClipboardAsync(dp, autoClear: true);

            return true;
        }

        private async void AddToClipboardAsync(DataPackage package, bool autoClear = false)
        {
            _cts?.Cancel();
            Clipboard.SetContent(package);

            if (!autoClear)
            {
                return;
            }

            _cts = new CancellationTokenSource();

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(15), _cts.Token);

                ClearClipboard();
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void ClearClipboard()
        {
            var toast = new ToastContent
            {
                Launch = "action=view&eventId=1983",
                Scenario = ToastScenario.Default,
                Visual = new ToastVisual
                {
                    BindingGeneric = new ToastBindingGeneric
                    {
                        Attribution = new ToastGenericAttributionText { Text = "KeePassWin" },
                        Children =
                        {
                            new AdaptiveText { Text = "Clipboard has been cleared" }
                        }
                    }
                }
            };

            Clipboard.Clear();
            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(toast.GetXml()));
        }

        public virtual bool Copy(ILogView log)
        {
            var datapackage = new DataPackage();

            datapackage.SetDataProvider(StandardDataFormats.Text, request =>
            {
                request.SetData(log.Log);
            });

            datapackage.SetDataProvider(StandardDataFormats.StorageItems, async request =>
            {
                var deferral = request.GetDeferral();

                request.SetData(new IStorageItem[] { await CreateFileAsync(log) });

                deferral.Complete();
            });

            AddToClipboardAsync(datapackage, autoClear: false);

            return true;
        }

        public async Task SendAsync(ILogView log)
        {
            Copy(log);

            var message = new EmailMessage
            {
                Body = LocalizedStrings.EmailMessageBody,
                Subject = string.Format(LocalizedStrings.EmailMessageSubject, log.Id, File.ReadAllText("version.txt").Trim())
            };

            message.To.Add(new EmailRecipient(LocalizedStrings.EmailMessageTo, LocalizedStrings.EmailMessageToName));

            using (var ms = new InMemoryRandomAccessStream())
            {
                using (var writer = new DataWriter(ms.GetOutputStreamAt(0)))
                {
                    writer.WriteString(log.Log);

                    await writer.StoreAsync();
                    await writer.FlushAsync();

                    var data = RandomAccessStreamReference.CreateFromStream(ms);

                    var attachment = new EmailAttachment($"{log.Id}.log", data);

                    message.Attachments.Add(attachment);

                    await EmailManager.ShowComposeNewEmailAsync(message);
                }
            }
        }

        private async Task<IStorageItem> CreateFileAsync(ILogView view)
        {
            var file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync($"{view.Id}.log", CreationCollisionOption.GenerateUniqueName);

            using (var stream = await file.OpenStreamForWriteAsync())
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteAsync(view.Log);
            }

            return file;
        }
    }
}
