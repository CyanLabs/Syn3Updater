namespace Cyanlabs.Syn3Updater.Model
{
    /// <summary>
    ///     Json class for additional Models such as Ivsu and SRegion
    /// </summary>
    public static class SModel
    {
        #region Properties & Fields

        public class SRegion
        {
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class Ivsu
        {
            public bool Selected { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public string Version { get; set; }
            public string Notes { get; set; }
            public string Url { get; set; }
            public string Md5 { get; set; }
            public string FileName { get; set; }
            public string Source { get; set; }
            public long FileSize { get; set; }
        }

        #endregion
    }
}