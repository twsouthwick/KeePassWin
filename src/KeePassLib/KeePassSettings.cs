using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KeePass
{
    public class KeePassSettings : INotifyPropertyChanged
    {
        private bool _trackTelemetry = false;
        private bool _clearOnSuspend = true;

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

        public virtual T Load<T>(string field)
        {
            return default(T);
        }

        public void Load()
        {
            TrackTelemetry = Load<bool>(nameof(TrackTelemetry));
            ClearOnSuspend = Load<bool>(nameof(ClearOnSuspend));
        }
    }
}
