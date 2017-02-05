using System;
using System.Threading;
using System.Windows.Input;

namespace KeePass.Win.ViewModels
{
    public class DelegateCommand<T> : ICommand
        where T : class
    {
        private readonly Action<T> _executeMethod;
        private readonly Func<T, bool> _canExecuteMethod;
        private readonly SynchronizationContext _synchronizationContext;

        public DelegateCommand(Action<T> executeMethod)
            : this(executeMethod, null)
        {
        }

        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
        {
            _executeMethod = executeMethod;
            _canExecuteMethod = canExecuteMethod ?? DefaultCanExecuteMethod;
            _synchronizationContext = SynchronizationContext.Current;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter) => _executeMethod(parameter as T);

        public bool CanExecute(object parameter) => _canExecuteMethod(parameter as T);

        protected virtual bool DefaultCanExecuteMethod(T obj) => true;

        public void RaiseCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                if (_synchronizationContext != null && _synchronizationContext != SynchronizationContext.Current)
                {
                    _synchronizationContext.Post(o => handler.Invoke(this, EventArgs.Empty), null);
                }
                else
                {
                    handler.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
