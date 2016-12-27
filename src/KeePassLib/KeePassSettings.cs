using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KeePass
{
    public class KeePassSettings : INotifyPropertyChanged
    {
        private bool _trackTelemetry = false;
        private bool _clearOnSuspend = true;
        private int _clipboardTimeout = 15;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool TrackTelemetry
        {
            get { return _trackTelemetry; }
            set { SetProperty(ref _trackTelemetry, value); }
        }

        public bool ClearOnSuspend
        {
            get { return _clearOnSuspend; }
            set { SetProperty(ref _clearOnSuspend, value); }
        }

        public int ClipboardTimeout
        {
            get { return _clipboardTimeout; }
            set { SetProperty(ref _clipboardTimeout, value); }
        }

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName]string name = null)
        {
            if (Equals(field, value))
            {
                return;
            }

            field = value;
            Save(name, value);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public virtual void Save<T>(string field, T value)
        {
        }

        public virtual T Load<T>(string field, T @default = default(T))
        {
            return @default;
        }

        public void Load()
        {
            TrackTelemetry = Load<bool>(nameof(TrackTelemetry), false);
            ClearOnSuspend = Load<bool>(nameof(ClearOnSuspend), true);
            ClipboardTimeout = Load(nameof(ClipboardTimeout), 15);
        }
    }
}
