using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KeePass.Win.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(ref T field, T obj, [CallerMemberName]string name = null)
        {
            if (Equals(obj, field))
            {
                return;
            }

            field = obj;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
