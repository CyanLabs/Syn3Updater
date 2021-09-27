namespace Syn3Updater.Models
{
    /*
        /// <summary>
        ///     Class to store the API Token, this is a duplicate with palceholder token
        ///     of the file APISecretModel.cs which is not included in the GitHub Repo
        /// </summary>
        public static class ApiSecret
        {
            public const string Token = "TOKEN";
        }
    */

    /// <summary>
    ///     Class for Api related properties and models
    ///     Json Classes for parsing JSON correctly
    /// </summary>
    public static class Api
    {
        public const string Syn3UpdaterGraphQl = "https://syn3updater.cyanlabs.net/graphql";

        public const string APIBase = "https://api.cyanlabs.net/";
        private const string AsBuiltBase = "https://asbuilt.cyanlabs.net/";

        public const string CrashLogUrl = "https://cyanlabs.net/syn3-updater-crash-log/?uuid=";
        public const string LogUrl = "https://cyanlabs.net/syn3-updater-log/?uuid=";
        public const string CrashLogPost = APIBase + "Syn3Updater/crash-logs/post.php";
        public const string LogPost = APIBase + "Syn3Updater/logs/post.php";
        public const string ChangelogURL = APIBase + "app/syn3updater/githubchangelog/json";

        public const int BlacklistedVersion = 3419274;
        public const int ReformatVersion = 3200000;

        public static SModel.Ivsu? ReformatTool, DowngradeAppIvsu, DowngradeToolIvsu, InterrogatorTool, GracenotesRemoval, SmallVoicePackage;

        public const string AsBuiltPost = APIBase + "Syn3Updater/apim-asbuilt-decode/";
        public const string AsBuiltOutput = AsBuiltBase + "?filename=";

        public static class SpecialPackages
        {
            public const string
                DowngradeApp = "downgradeapp",
                DowngradeTool = "gracenotesremoval",
                LogTool34 = "logtool34",
                LogTool32 = "logtool34",
                LogTool30 = "logtool30",
                GraceNotesRemoval = "gracenotesremoval",
                SmallVoice = "smallvoice",
                Reformat = "reformat";
        }
    }
}