namespace Cyanlabs.Syn3Updater.Model
{
    /// <summary>
    ///     Json class for Syn3Updater MainSettings
    /// </summary>
    public class JsonMainSettings
    {
        public string Lang { get; set; }
        public string DownloadPath { get; set; }
        public string LicenseKey { get; set; } = "";
        public bool DisclaimerAccepted { get; set; } = false;
        public string Theme { get; set; }
        public string LogPath { get; set; }
        public string Profile { get; set; }
        
        // Old settings for migration purposes
        public string CurrentRegion { get; set; }
        public int CurrentVersion { get; set; } = 3319052;
        public bool CurrentNav { get; set; } = false;
        public bool My20 { get; set; } = false;
    }
    
    public class JsonSettings
    {
        public string CurrentRegion { get; set; }
        public int CurrentVersion { get; set; } = 3319052;
        public bool CurrentNav { get; set; } = false;
        public bool My20 { get; set; } = false;
    }
}