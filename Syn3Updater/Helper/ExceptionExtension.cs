using System;

namespace Cyanlabs.Syn3Updater.Helper
{
    /// <summary>
    ///     Helper class to extend functionality of Exceptions
    /// </summary>
    public static class ExceptionExtension
    {
        #region Methods

        /// <summary>
        ///     Recursively get main Exception message and all InnerException messages
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="message"></param>
        /// <returns>all innerexception messages as type String</returns>
        public static string GetFullMessage(this Exception ex, string message = "")
        {
            if (ex == null) return string.Empty;

            message += ex.Message;

            if (ex.InnerException != null)
                message += $"\r\n{GetFullMessage(ex.InnerException)}";

            return message;
        }

        #endregion
    }
}