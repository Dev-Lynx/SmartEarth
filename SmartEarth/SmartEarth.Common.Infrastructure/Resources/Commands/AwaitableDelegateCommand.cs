using Prism.Commands;
using SmartEarth.Common.Infrastructure.Resources.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SmartEarth.Common.Infrastructure.Resources.Commands
{
    public class AwaitableDelegateCommand : AwaitableDelegateCommand<object>, IAsyncCommand
    {
        public AwaitableDelegateCommand(Func<Task> executeMethod)
            : base(o => executeMethod())
        {
        }

        public AwaitableDelegateCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod)
            : base(o => executeMethod(), o => canExecuteMethod())
        {
        }
    }

    public class AwaitableDelegateCommand<T> : IAsyncCommand<T>, ICommand
    {
        #region Properties
        public ICommand Command => this;

        #region Events
        public event EventHandler CanExecuteChanged
        {
            add { _underlyingCommand.CanExecuteChanged += value; }
            remove { _underlyingCommand.CanExecuteChanged -= value; }
        }
        #endregion

        #region Internals
        readonly Func<T, Task> _executingMethod;
        readonly DelegateCommand<T> _underlyingCommand;
        bool _isExecuting;
        #endregion

        #endregion

        #region Constructors
        public AwaitableDelegateCommand(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod)
        {
            _executingMethod = executeMethod;
            _underlyingCommand = new DelegateCommand<T>(x => { }, canExecuteMethod);
        }

        public AwaitableDelegateCommand(Func<T, Task> executeMethod) : this(executeMethod, _ => true) { }
        #endregion

        #region Methods

        #region IAsyncCommand Implementation
        public async Task ExecuteAsync(T obj)
        {
            try
            {
                _isExecuting = true;
                RaiseCanExecuteChanged();
                await _executingMethod(obj);
            }
            catch
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged()
        {
            _underlyingCommand.RaiseCanExecuteChanged();
        }
        #endregion

        #region ICommand Implementation
        public bool CanExecute(object obj)
        {
            return !_isExecuting && _underlyingCommand.CanExecute((T)obj);
        }

        public async void Execute(object parameter)
        {
            await ExecuteAsync((T)parameter);
        }
        #endregion

        #endregion
    }
}
