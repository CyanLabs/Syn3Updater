using System;
using System.Windows.Input;

namespace Cyanlabs.Syn3Updater.Helper
{
    /// <summary>
    ///     Helper class for ActionCommands used for MVVM button presses
    /// </summary>
    public class ActionCommand : ICommand
    {
        #region Constructors

        public ActionCommand(Action command, Func<bool> canExecute = null)
        {
            _command = command;
            _canExecute = canExecute;
        }

        #endregion

        #region Events

        public event EventHandler CanExecuteChanged;

        #endregion

        #region Properties & Fields

        private readonly Func<bool> _canExecute;
        private readonly Action _command;

        #endregion

        #region Methods

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object parameter)
        {
            _command?.Invoke();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }

    //public class ActionCommand<T> : ICommand
    //{
    //    #region Constructors

    //    public ActionCommand(Action<T> command, Func<T, bool> canExecute = null)
    //    {
    //        _command = command;
    //        _canExecute = canExecute;
    //    }

    //    #endregion

    //    #region Events

    //    public event EventHandler CanExecuteChanged;

    //    #endregion

    //    #region Properties & Fields

    //    private readonly Func<T, bool> _canExecute;
    //    private readonly Action<T> _command;

    //    #endregion

    //    #region Methods

    //    public bool CanExecute(object parameter)
    //    {
    //        return _canExecute?.Invoke((T) parameter) ?? true;
    //    }

    //    public void Execute(object parameter)
    //    {
    //        _command?.Invoke((T) parameter);
    //    }

    //    public void RaiseCanExecuteChanged()
    //    {
    //        CanExecuteChanged?.Invoke(this, new EventArgs());
    //    }

    //    #endregion
    //}
}