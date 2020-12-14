using System;
using System.Windows;
using System.Windows.Threading;

namespace Syn3Updater.Helper
{
    public static class ExceptionExtension
    {
        #region Methods

        public static string GetFullMessage(this Exception ex, string message = "")
        {
            if (ex == null) return string.Empty;

            message += ex.Message;

            if (ex.InnerException != null)
                message += "\r\nInnerException: " + GetFullMessage(ex.InnerException);

            return message;
        }

        private static readonly Action EmptyDelegate = delegate () { };


        public static void Refresh(this UIElement uiElement)

        {
            uiElement?.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }

        #endregion
    }
}
