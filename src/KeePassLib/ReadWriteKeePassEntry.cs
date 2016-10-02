using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KeePass
{
    public class ReadWriteKeePassEntry : IKeePassEntry, INotifyPropertyChanged
    {
        private string _password;
        private string _title;
        private string _url;
        private string _userName;

        public IList<IKeePassAttachment> Attachment { get; } = new List<IKeePassAttachment>();

        public byte[] Icon { get; set; }

        public KeePassId Id { get; set; }

        public string Notes { get; set; }

        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public string Url
        {
            get { return _url; }
            set { SetProperty(ref _url, value); }
        }

        public string UserName
        {
            get { return _userName; }
            set { SetProperty(ref _userName, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName]string name = null)
        {
            if (object.Equals(field, value))
            {
                return;
            }

            field = value;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
