using System;

namespace Cyanlabs.Syn3Updater.Model
{
    /// <summary>
    ///     Json class for Log Model
    /// </summary>
    public static class LogModel
    {
        #region Properties & Fields
        public class Log
        {
            public bool Selected { get; set; }
            public string FileName { get; set; }
            public string Profile { get; set; }
            public DateTime Date{ get; set; }
        }
        #endregion
    }
}