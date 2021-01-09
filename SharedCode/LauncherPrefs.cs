namespace SharedCode
{
    public class LauncherPrefs
    {
        public enum ReleaseType
        {
            Release,
            Beta,
            CI
        }
        public ReleaseType ReleaseBranch { get; set; } = ReleaseType.Release;
        public int ReleaseInstalled { get; set; } = 0;
        public ReleaseType ReleaseTypeInstalled { get; set; } = ReleaseType.Release;
    }
}
