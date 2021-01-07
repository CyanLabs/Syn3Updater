using SharedCode;

namespace Launcher
{
    public static class Core
    {
        static Core()
        {
            LauncherPrefs = new LauncherPrefs();
        }

        public static LauncherPrefs LauncherPrefs;
        public static UpgradingWindow UpgradingWindow { get; set; }
    }
}
