using KeePass.Win.Log;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Email;
using Windows.Storage;
using Windows.Storage.Streams;

namespace KeePass.Win.Services
{
    public class DataPackageClipboard : IClipboard<string>, IClipboard<ILogView>, IMailClient<ILogView>
    {
        public virtual bool Copy(string text)
        {
            if (text == null)
            {
                return false;
            }

            var dp = new DataPackage();

            dp.SetText(text);

            Clipboard.SetContent(dp);

            return true;
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

                request.SetData(new IStorageItem[] { await CreateFile(log) });

                deferral.Complete();
            });

            Clipboard.SetContent(datapackage);

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

        private async Task<IStorageItem> CreateFile(ILogView view)
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
