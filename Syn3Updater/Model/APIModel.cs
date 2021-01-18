using System.Collections.Generic;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global

namespace Cyanlabs.Syn3Updater.Model
{
    //public static class APISecretExample
    //{
    //    public const string Token = "TOKEN";
    //}

    public class Api
    {
        #region Properties & Fields

        private const string Base = "https://api.cyanlabs.net/fordsyncdownloader/";
        public const string AppReleasesConst = Base + "items/releases?sort=-name&limit=-1&[published]";
        public const string MapReleasesConst = Base + "items/map_releases?sort=-name&limit=-1&[regionplaceholder]&[published]";
        public const string AppReleaseSingle = Base + "items/releases?sort=-name&limit=-1&fields=*.*.*&filter[name]=";
        public const string MapReleaseSingle = Base + "items/map_releases?sort=-name&limit=-1&fields=*.*.*&filter[name]=";
        public const string IVSUSingle = Base + "items/ivsu?limit=1&fields=version&filter[name]=";

        public const string CrashLogURL = "https://cyanlabs.net/api/Syn3Updater/crash-logs/?uuid=";
        public const string CrashLogPost = "https://cyanlabs.net/api/Syn3Updater/crash-logs/post.php";
        
        public const string LogURL = "https://cyanlabs.net/api/Syn3Updater/logs/?uuid=";
        public const string LogPost = "https://cyanlabs.net/api/Syn3Updater/logs/post.php";

        public const int BlacklistedVersion = 3419274;
        public const int ReformatVersion = 3200000;

        public static SyncModel.SyncIvsu ReformatTool, DowngradeApp, DowngradeTool, InterrogatorTool, GracenotesRemoval, SmallVoicePackage;

        private const string AsBuiltBase = "https://asbuilt.cyanlabs.net/";
        public const string AsBuiltPost =  "https://cyanlabs.net/api/Syn3Updater/apim-asbuilt-decode/";
        public const string AsBuiltOutput = AsBuiltBase + "?filename=";

        public const string SpecialPackageBase = "https://cyanlabs.net/api/syn3updater/get/";
        public const string GetDowngradeApp = SpecialPackageBase + "downgradeapp";
        public const string GetDowngradeTool = SpecialPackageBase + "downgradetool";
        public const string GetLogTool = SpecialPackageBase + "logtool";
        public const string GetGracenotesRemoval = SpecialPackageBase + "gracenotesremoval";
        public const string GetSmallVoice = SpecialPackageBase + "smallvoice";
        public const string GetReformat = SpecialPackageBase + "reformat";

        public class Data
        {
            public int id { get; set; }
            public string name { get; set; }
            public string notes { get; set; }
            public IList<string> regions { get; set; }
            public string status { get; set; }
            public string version { get; set; }
            public IList<Ivsus> ivsus { get; set; }

        }

        public class Ivsu
        {
            public int id { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public IList<string> regions { get; set; }
            public string md5 { get; set; }
            public string url { get; set; }
            public string notes { get; set; }
            public string version { get; set; }
        }

        public class Ivsus
        {
            public int id { get; set; }
            public Ivsu ivsu { get; set; }
            public Map_Ivsu map_ivsu { get; set; }
            public Release release { get; set; }
        }

        public class JsonReleases
        {
            public IList<Data> data { get; set; }
        }

        public class Map_Ivsu
        {
            public int id { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public IList<string> regions { get; set; }
            public string md5 { get; set; }
            public string notes { get; set; }
            public string url { get; set; }
            public string version { get; set; }
        }

        public class Release
        {
            public int id { get; set; }
            public string name { get; set; }
            public string map_version { get; set; }
            public string notes { get; set; }
            public IList<string> regions { get; set; }
        }

        public class Root
        {
            [JsonProperty("data")]
            public Data Data { get; set; }
        }

        #endregion
    }
}