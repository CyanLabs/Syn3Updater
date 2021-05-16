namespace Cyanlabs.Updater.Common
{
    /// <summary>
    ///     Shared class used for Launcher preferences
    /// </summary>
    public class LauncherPrefs
    {
        public enum ReleaseType
        {
            Stable,
            Beta,
            Alpha
        }

        public ReleaseType ReleaseBranch { get; set; }
        public string ReleaseInstalled { get; set; } = "0.0.0.0";
        public ReleaseType ReleaseTypeInstalled { get; set; }
    }
}