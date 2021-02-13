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
    public class Api
    {
        #region Properties & Fields

        private const string Base = "https://syn3updater.cyanlabs.net/";
        public const string AppReleasesConst = Base + "items/releases?fields=name,notes,regions,version&sort=-name&limit=-1&[published]";
        public const string MapReleasesConst = Base + "items/map_releases?fields=name,regions&sort=-name&limit=-1&[regionplaceholder]&[published]";
        public const string AppReleaseSingle = Base + "items/releases?deep[ivsus][filter][ivsu][navtype][_in]=[navplaceholder],all&fields=*.ivsu.*.&filter[name]=";
        public const string MapReleaseSingle = Base + "items/map_releases?fields=*.*.*&filter[name]=";
        public const string IvsuSingle = Base + "items/ivsu?limit=1&fields=version&filter[name]=";

        public const string CrashLogUrl = "https://api.cyanlabs.net/Syn3Updater/crash-logs/?uuid=";
        public const string CrashLogPost = "https://api.cyanlabs.net/Syn3Updater/crash-logs/post.php";

        public const string LogUrl = "https://api.cyanlabs.net/Syn3Updater/logs/?uuid=";
        public const string LogPost = "https://api.cyanlabs.net/Syn3Updater/logs/post.php";

        public const int BlacklistedVersion = 3419274;
        public const int ReformatVersion = 3200000;

        public static SyncModel.SyncIvsu ReformatTool, DowngradeApp, DowngradeTool, InterrogatorTool, GracenotesRemoval, SmallVoicePackage;

        private const string AsBuiltBase = "https://asbuilt.cyanlabs.net/";
        public const string AsBuiltPost = "https://api.cyanlabs.net/Syn3Updater/apim-asbuilt-decode/";
        public const string AsBuiltOutput = AsBuiltBase + "?filename=";

        public const string SpecialPackageBase = "https://api.cyanlabs.net/syn3updater/get/";
        public const string GetDowngradeApp = SpecialPackageBase + "downgradeapp";
        public const string GetDowngradeTool = SpecialPackageBase + "downgradetool";
        public const string GetLogTool = SpecialPackageBase + "logtool";
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
        }

        public class Release
        {
            [JsonProperty("id")] public int Id { get; set; }
            [JsonProperty("name")] public string Name { get; set; }
            [JsonProperty("map_version")] public string MapVersion { get; set; }
            [JsonProperty("notes")] public string Notes { get; set; }
            [JsonProperty("regions")] public IList<string> Regions { get; set; }
        }

        #endregion
    }
}