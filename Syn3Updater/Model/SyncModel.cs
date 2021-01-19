namespace Cyanlabs.Syn3Updater.Model
{
    /// <summary>
    ///     Json class for additional Sync Models such as SyncIvsu and SyncRegion
    /// </summary>
    public static class SyncModel
    {
        #region Properties & Fields

        public class SyncRegion
        {
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class SyncIvsu
        {
            public bool Selected { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public string Version { get; set; }
            public string Notes { get; set; }
            public string Url { get; set; }
            public string Md5 { get; set; }
            public string FileName { get; set; }
        }

        #endregion
    }
}