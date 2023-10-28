using System;
using System.Windows.Input;

namespace SyncIMEStatus.Helpers
{
    public class DelegateCommand : ICommand
    {
        public Action<object> ExecuteHandler { get; set; }
        public Func<object, bool> CanExecuteHandler { get; set; }
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (CanExecuteHandler == null) { return true; }
            return CanExecuteHandler(parameter);
        }

        public void Execute(object parameter)
        {   
            ExecuteHandler?.Invoke(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}