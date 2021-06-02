using System;

namespace Cyanlabs.Syn3Updater.Helper
{
    /// <summary>
    ///     Helper class for EventRaising
    /// </summary>
    public class EventArgs<T> : EventArgs
    {
        #region Constructors

        public EventArgs(T value, int part)
        {
            Value = value;
            Part = part;
        }

        public T Value { get; }
        public int Part { get; }

        #endregion
    }

    public static class EventRaiser
    {
        #region Methods

        public static void Raise<T>(this EventHandler<EventArgs<T>> handler, object sender, T value, int part)
        {
            handler?.Invoke(sender, new EventArgs<T>(value, part));
        }

        #endregion
    }
}