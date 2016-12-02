using Windows.ApplicationModel.DataTransfer;

namespace KeePass.Win.Services
{
    public class DataPackageClipboard : IClipboard<string>
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
    }
}
