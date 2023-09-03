using System.Collections.Generic;
using GraphQL;
using Newtonsoft.Json;

namespace Cyanlabs.Syn3Updater.Model
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
        #region Properties & Fields
        public const string Syn3UpdaterGraphQl = "https://syn3updater.cyanlabs.net/graphql";

        public const string APIBase = "https://api.cyanlabs.net/";
        private const string AsBuiltBase = "https://cyanlabs.net/fhub/sync3/apim-asbuilt-decoder/";

        public const string CrashLogUrl = "https://cyanlabs.net/syn3-updater-crash-log/?uuid=";
        public const string LogUrl = "https://cyanlabs.net/syn3-updater-log/?uuid=";
        public const string CrashLogPost = APIBase + "Syn3Updater/crash-logs/post.php";
        public const string LogPost = APIBase + "Syn3Updater/logs/post.php";
        public const string ChangelogURL = APIBase + "app/syn3updater/githubchangelog/json";

        public const int BlacklistedVersion = 3419274;
        public const int ReformatVersion = 3200000;

        public static SModel.Ivsu ReformatTool, DowngradeAppIvsu, DowngradeToolIvsu, InterrogatorTool, GracenotesRemoval, SmallVoicePackage, RWDataCleaner;

        public const string AsBuiltPost = APIBase + "Syn3Updater/apim-asbuilt-decode/";
        public const string AsBuiltOutput = AsBuiltBase + "?filename=";

        public static class SpecialPackages
        {
            public const string
                DowngradeApp = "downgradeapp",
                DowngradeTool = "gracenotesremoval",
                LogToolAD = "logtool34-2",
                LogToolAC = "logtool34",
                LogToolAB = "logtool32",
                LogToolAA = "logtool30",
                GraceNotesRemoval = "gracenotesremoval",
                SmallVoice = "smallvoice",
                Reformat = "reformat",
                RWDataCleaner = "rwdatacleaner";
        }
        #endregion
        
        #region JSONClasses
        
        public class ReleasesRoot
        {
            [JsonProperty("releases")] public IList<Release> Releases { get; set; }
            [JsonProperty("map_releases")] public IList<Release> MapReleases { get; set; }
        }
        
        public class IvsuRoot
        {
            [JsonProperty("ivsu")] public IList<Ivsu> Ivsus { get; set; }
        }

        public class NoticesRoot
        {
            [JsonProperty("notices")] public IList<Notice> Notices { get; set; }
        }
        
        public class UseragentRoot
        {
            [JsonProperty("useragents")] public IList<UserAgent> UserAgents { get; set; }
        }
        
        public class Release
        {
            [JsonProperty("id")] public int Id { get; set; }
            [JsonProperty("name")] public string Name { get; set; }
            [JsonProperty("map_version")] public string MapVersion { get; set; }
            [JsonProperty("notes")] public string Notes { get; set; }
            [JsonProperty("regions")] public IList<string> Regions { get; set; }
            [JsonProperty("version")] public string Version { get; set; }
            [JsonProperty("feedbackurl")] public string Feedbackurl { get; set; }
            [JsonProperty("status")] public string Status { get; set; }
            [JsonProperty("ivsus")] public IList<ReleasesIvsus> IvsusList { get; set; }
        }
        
        public class ReleasesIvsus
        {
            [JsonProperty("id")] public int Id { get; set; }
            [JsonProperty("ivsu")] public Ivsu Ivsu { get; set; }
            [JsonProperty("map_ivsu")] public Ivsu MapIvsu { get; set; }
            [JsonProperty("release")] public Release Release { get; set; }
        }
        
        public class Ivsu
        {
            [JsonProperty("id")] public int Id { get; set; }
            [JsonProperty("name")] public string Name { get; set; }
            [JsonProperty("type")] public string Type { get; set; }
            [JsonProperty("regions")] public IList<string> Regions { get; set; }
            [JsonProperty("md5")] public string Md5 { get; set; }
            [JsonProperty("url")] public string Url { get; set; }
            [JsonProperty("notes")] public string Notes { get; set; }
            [JsonProperty("version")] public string Version { get; set; }
            [JsonProperty("filesize")] public long FileSize { get; set; }
            [JsonProperty("source")] public string Source { get; set; }
            [JsonProperty("specialpackage")] public string SpecialPackage { get; set; }
        }

        public class UserAgent
        {
            [JsonProperty("useragent")] public string Useragent { get; set; }
            [JsonProperty("min")] public int Min { get; set; }
            [JsonProperty("max")] public int Max { get; set; }
        }

        
        public class My20Models
        {
            [JsonProperty("model")] public string Model { get; set; }
        }

        public class My20ModelsRoot
        {
            [JsonProperty("my20models")] public List<My20Models> My20Models { get; set; }
        }

        public class Notice
        {
            [JsonProperty("id")] public int Id { get; set; }
            [JsonProperty("date_created")] public string DateCreated { get; set; }
            [JsonProperty("date_updated")] public string DateUpdated { get; set; }
            [JsonProperty("notice")] public string NoticeContent { get; set; }
            [JsonProperty("title")] public string Title { get; set; }
            [JsonProperty("enabled")] public bool Enabled { get; set; }
            [JsonProperty("important")] public bool Important { get; set; }
        }

        public class Changelogs
        {
            [JsonProperty("data")] public IList<Changelog> Changelog { get; set; }
        }

        public class Changelog
        {
            [JsonProperty("version")] public string Version { get; set; }
            [JsonProperty("date")] public string Date { get; set; }
            [JsonProperty("changelog")] public string ReleaseNotes { get; set; }
            [JsonProperty("branch")] public string Branch { get; set; }
        }
        #endregion
    }
}