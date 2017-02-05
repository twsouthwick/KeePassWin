using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace KeePass.Win.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public ViewModelBase()
        {
            SynchContext = SynchronizationContext.Current;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected SynchronizationContext SynchContext { get; }

        protected void SetProperty<T>(ref T field, T obj, [CallerMemberName]string name = null)
        {
            if (Equals(obj, field))
            {
                return;
            }

            field = obj;

            if (SynchContext != null && SynchContext != SynchronizationContext.Current)
            {
                SynchContext.Post(_ => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)), null);
            }
            else
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
