namespace Syn3Updater.Models
{
    /// <summary>
    ///     Json class for Syn3Updater MainSettings
    /// </summary>
    public class MainSettings
    {
        public string? DownloadPath { get; set; }
        public string LicenseKey { get; set; } = "";
        public bool DisclaimerAccepted { get; set; } = false;
        public string? LogPath { get; set; }
        public string Profile { get; set; } = "default";
    }

    public class ProfileSettings
    {
        public string? CurrentRegion { get; set; }
        public int CurrentVersion { get; set; } = 0;
        public bool CurrentNav { get; set; } = false;
        public bool? My20 { get; set; } = null;
        public string InstallMode { get; set; } = "autodetect";
        public int DownloadConnections { get; set; } = 1;
    }
}