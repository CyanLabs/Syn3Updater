using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Cyanlabs.Launcher
{
    /// <inheritdoc />
    /// <summary>
    ///     Represents a basic bindable class which notifies when a property value changes.
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        #region Events

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Methods
        /// <summary>
        ///     Checks if the property already matches the desired value and updates it if not.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="value">Value to apply.</param>
        /// <param name="propertyName">
        ///     Name of the property used to notify listeners. This value is optional
        ///     and can be provided automatically when invoked from compilers that support <see cref="CallerMemberNameAttribute" />
        ///     .
        /// </param>
        /// <returns><c>true</c> if the value was changed, <c>false</c> if the existing value matched the desired value.</returns>
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
        #endregion
    }
}