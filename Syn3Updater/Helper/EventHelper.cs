using System;

namespace Cyanlabs.Syn3Updater.Helper
{
    public class EventArgs<T> : EventArgs
    {
        #region Constructors

        public EventArgs(T value)
        {
            Value = value;
        }

        public T Value { get; }

        #endregion
    }

    public static class EventRaiser
    {
        #region Methods

        public static void Raise<T>(this EventHandler<EventArgs<T>> handler, object sender, T value)
        {
            handler?.Invoke(sender, new EventArgs<T>(value));
        }

        #endregion
    }
}