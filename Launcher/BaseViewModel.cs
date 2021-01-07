using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Launcher
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal bool Set<T>(ref T variable, T value, [CallerMemberName] String propertyName = "")
        {
            if (variable == null && value != null || variable != null && value == null || (variable != null && value != null && !variable.Equals(value)))
            {
                variable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
