using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Cyanlabs.Launcher
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal bool Set<T>(ref T variable, T value, [CallerMemberName] string propertyName = "")
        {
            if (variable == null && value != null || variable != null && value == null || variable != null && value != null && !variable.Equals(value))
            {
                variable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }
    }
}