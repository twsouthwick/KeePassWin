using System;
using Windows.Storage;

namespace KeePass.Win.Services
{
    public class WinKeePassSettings : KeePassSettings
    {
        private readonly ApplicationDataContainer _settings;

        public WinKeePassSettings()
        {
            _settings = ApplicationData.Current.LocalSettings;
        }

        public override T Load<T>(string field)
        {
            var value = _settings.Values[field];

            if (value == null)
            {
                return default(T);
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public override void Save<T>(string field, T value)
        {
            _settings.Values[field] = value;
        }
    }
}
