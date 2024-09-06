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

    public class AssemblyModelNotFound : Exception
    {
        public AssemblyModelNotFound()
        {
        }

        public AssemblyModelNotFound(string message)
            : base(message)
        {
        }

        public AssemblyModelNotFound(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class AssemblyVersionIncompatible : Exception
    {
        public AssemblyVersionIncompatible()
        {
        }

        public AssemblyVersionIncompatible(string message)
            : base(message)
        {
        }

        public AssemblyVersionIncompatible(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}