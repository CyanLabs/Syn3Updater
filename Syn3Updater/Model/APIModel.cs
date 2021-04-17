using System.Collections.Generic;
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

        private const string Syn3UpdaterBase = "https://syn3updater.cyanlabs.net/";
        public const string APIBase = "https://api.cyanlabs.net/";
        private const string AsBuiltBase = "https://asbuilt.cyanlabs.net/";

        public const string AppReleasesConst = Syn3UpdaterBase + "items/releases?fields=name,notes,regions,version&sort=-name&limit=-1&[published]";
        public const string MapReleasesConst = Syn3UpdaterBase + "items/map_releases?fields=name,regions,esn&sort=-name&limit=-1&[published]";
        public const string AppReleaseSingle = Syn3UpdaterBase + "items/releases?deep[ivsus][_filter][ivsu][navtype][_in]=[navplaceholder],all&fields=*.ivsu.*.&filter[name]=";
        public const string MapReleaseSingle = Syn3UpdaterBase + "items/map_releases?fields=*.*.*&filter[name]=";
        public const string IvsuSingle = Syn3UpdaterBase + "items/ivsu?limit=1&fields=version&filter[name]=";
        public const string NoticesURL = Syn3UpdaterBase + "items/notices?fields=*&limit=-1&filter[enabled]=true";

        public const string CrashLogUrl = "https://cyanlabs.net/syn3-updater-crash-log/?uuid=";
        public const string LogUrl = "https://cyanlabs.net/syn3-updater-log/?uuid=";

        public const string CrashLogPost = APIBase + "Syn3Updater/crash-logs/post.php";
        public const string LogPost = APIBase + "Syn3Updater/logs/post.php";
        public const string HeaderURL = APIBase + "syn3updater/useragents";
        public const string My20URL = APIBase + "syn3updater/my20";
        public const string ChangelogURL = APIBase + "app/syn3updater/githubchangelog";
        public const string SpecialPackageBase = APIBase + "syn3updater/get/";

        public const int BlacklistedVersion = 3419274;
        public const int ReformatVersion = 3200000;

        public static SModel.Ivsu ReformatTool, DowngradeApp, DowngradeTool, InterrogatorTool, GracenotesRemoval, SmallVoicePackage;

        public const string AsBuiltPost = APIBase + "Syn3Updater/apim-asbuilt-decode/";
        public const string AsBuiltOutput = AsBuiltBase + "?filename=";

        public const string GetDowngradeApp = SpecialPackageBase + "downgradeapp";
        public const string GetDowngradeTool = SpecialPackageBase + "downgradetool";
        public const string GetLogTool34 = SpecialPackageBase + "logtool34";
        public const string GetLogTool32 = SpecialPackageBase + "logtool32";
        public const string GetLogTool30 = SpecialPackageBase + "logtool30";
        public const string GetGracenotesRemoval = SpecialPackageBase + "gracenotesremoval";
        public const string GetSmallVoice = SpecialPackageBase + "smallvoice";
        public const string GetReformat = SpecialPackageBase + "reformat";

        public class Data
        {
            [JsonProperty("id")] public int Id { get; set; }
            [JsonProperty("name")] public string Name { get; set; }
            [JsonProperty("notes")] public string Notes { get; set; }
            [JsonProperty("regions")] public IList<string> Regions { get; set; }
            [JsonProperty("status")] public string Status { get; set; }
            [JsonProperty("version")] public string Version { get; set; }
            [JsonProperty("ivsus")] public IList<Ivsus> IvsusList { get; set; }
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
        }

        public class Ivsus
        {
            [JsonProperty("id")] public int Id { get; set; }
            [JsonProperty("ivsu")] public Ivsu Ivsu { get; set; }
            [JsonProperty("map_ivsu")] public MapIvsu MapIvsu { get; set; }
            [JsonProperty("release")] public Release Release { get; set; }
        }

        public class JsonReleases
        {
            [JsonProperty("data")] public IList<Data> Releases { get; set; }
        }

        public class MapIvsu
        {
            [JsonProperty("id")] public int Id { get; set; }
            [JsonProperty("name")] public string Name { get; set; }
            [JsonProperty("type")] public string Type { get; set; }
            [JsonProperty("regions")] public IList<string> Regions { get; set; }
            [JsonProperty("md5")] public string Md5 { get; set; }
            [JsonProperty("notes")] public string Notes { get; set; }
            [JsonProperty("url")] public string Url { get; set; }
            [JsonProperty("version")] public string Version { get; set; }
            [JsonProperty("source")] public string Source { get; set; }
            [JsonProperty("filesize")] public long FileSize { get; set; }
        }

        public class Release
        {
            [JsonProperty("id")] public int Id { get; set; }
            [JsonProperty("name")] public string Name { get; set; }
            [JsonProperty("map_version")] public string MapVersion { get; set; }
            [JsonProperty("notes")] public string Notes { get; set; }
            [JsonProperty("regions")] public IList<string> Regions { get; set; }
        }

        public class Header
        {
            [JsonProperty("ua")] public string Ua { get; set; }
            [JsonProperty("min")] public int Min { get; set; }
            [JsonProperty("max")] public int Max { get; set; }
        }

        public class Headers
        {
            [JsonProperty("data")] public IList<Header> Header { get; set; }
        }

        public class My20Models
        {
            [JsonProperty("data")] public IList<string> My20Model { get; set; }
        }

        public class Notices
        {
            [JsonProperty("data")] public IList<Notice> Notice { get; set; }
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
        #endregion
    }
}