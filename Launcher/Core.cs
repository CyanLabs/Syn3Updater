using SharedCode;

namespace Cyanlabs.Launcher
{
    public static class Core
    {
        public static LauncherPrefs LauncherPrefs;

        static Core()
        {
            LauncherPrefs = new LauncherPrefs();
        }

        public static UpgradingWindow UpgradingWindow { get; set; }
    }
}