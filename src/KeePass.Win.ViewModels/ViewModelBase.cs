using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace KeePass.Win.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ViewModelBase() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
