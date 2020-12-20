using System;

namespace Syn3Updater.Helper
{
    public class EventArgs<T> : EventArgs
    {
        public EventArgs(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }

    public static class EventRaiser
    {
        public static void Raise<T>(this EventHandler<EventArgs<T>> handler, object sender, T value)
        {
            handler?.Invoke(sender, new EventArgs<T>(value));
        }
    }
}
