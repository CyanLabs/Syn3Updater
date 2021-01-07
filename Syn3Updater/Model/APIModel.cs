using System.Collections.Generic;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global

namespace Syn3Updater.Model
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

        public const string UpdaterReleaseURL = "https://api.github.com/repos/cyanlabs/Syn3Updater/releases/latest";
        public const string UpdaterChangelogURL = "https://cyanlabs.net/api/changelog-github.php?product=Syn3Updater";

        public const string CrashLogURL = "https://cyanlabs.net/api/Syn3Updater/crash-logs/?uuid=";
        public const string CrashLogPost = "https://cyanlabs.net/api/Syn3Updater/crash-logs/post.php";
        
        public const string LogURL = "https://cyanlabs.net/api/Syn3Updater/logs/?uuid=";
        public const string LogPost = "https://cyanlabs.net/api/Syn3Updater/logs/post.php";

        public const int BlacklistedVersion = 3419274;
        public const int ReformatVersion = 3200000;

        public static readonly SyncModel.SyncIvsu ReformatTool = new SyncModel.SyncIvsu
        {
            Type = "TOOL",
            Name = "1u5t-14g386-cb",
            Version = "",
            Notes = LanguageManager.GetValue("String.Required") + " Factory Reformat Tool - Production Cert",
            Url = "https://cyanlabs.net/api/Syn3Updater/reformat.php",
            Md5 = "75E08C3EED8D2039BAF65B6156F79106",
            Selected = true,
            FileName = "1u5t-14g386-cb.tar.gz"
        };

        public static readonly SyncModel.SyncIvsu DowngradeApp = new SyncModel.SyncIvsu
        {
            Type = "APPS",
            Name = "4U5T-14G381-AN",
            Version = "3.3.19052",
            Notes = LanguageManager.GetValue("String.Required"),
            Url = "https://ivsubinaries.azureedge.net/swparts/4U5T-14G381-AN_1552583626000.TAR.GZ",
            Md5 = "0553D1A474FBF9F0DB68A9C96FBDA7CB",
            Selected = true,
            FileName = "4U5T-14G381-AN_1552583626000.TAR.GZ"
        };

        public static readonly SyncModel.SyncIvsu DowngradeTool = new SyncModel.SyncIvsu
        {
            Type = "TOOL",
            Name = "GB5T-14G386-SC",
            Version = "",
            Notes = LanguageManager.GetValue("String.Required"),
            Url = "https://ivsubinaries.azureedge.net/swparts/GB5T-14G386-SC_85041.tar.gz",
            Md5 = "E16F5E01D816E738E2B68592BDC22F3F",
            Selected = true,
            FileName = "GB5T-14G386-SC_85041.tar.gz"
        };

        public static readonly SyncModel.SyncIvsu InterrogatorTool = new SyncModel.SyncIvsu
        {
            Type = "TOOL",
            Name = "GB5T-14G386-AC",
            Version = "",
            Notes = LanguageManager.GetValue("String.Required"),
            Url = "http://ivsu.binaries.ford.com/swparts/GB5T-14G386-AC_1587495565000.TAR.GZ",
            Md5 = "1D8A92F839324C96C7A9AD95A2AFC39E",
            Selected = true,
            FileName = "GB5T-14G386-AC_1587495565000.TAR.GZ"
        };

        public static readonly SyncModel.SyncIvsu GracenotesRemoval = new SyncModel.SyncIvsu
        {
            Type = "TOOL",
            Name = "GB5T-14G386-SC",
            Version = "",
            Notes = "Remove Gracenotes Deletes",
            Url = "https://ivsubinaries.azureedge.net/swparts/GB5T-14G386-SC_85041.tar.gz",
            Md5 = "E16F5E01D816E738E2B68592BDC22F3F",
            Selected = true,
            FileName = "GB5T-14G386-SC_85041.tar.gz"
        };

        public static readonly SyncModel.SyncIvsu SmallVoicePackage = new SyncModel.SyncIvsu
        {
            Type = "VOICE",
            Name = "5U5T-14G391-CM",
            Version = "3.2.20142",
            Notes = "",
            Url = "https://ivsubinaries.azureedge.net/swparts/5U5T-14G391-CM_1598635923000.TAR.GZ",
            Md5 = "07BA3A81B976701ED18E2718D866418B",
            Selected = true,
            FileName = "5U5T-14G391-CM_1598635923000.TAR.GZ"
        };

        private const string AsBuiltBase = "https://asbuilt.cyanlabs.net/";
        public const string AsBuiltPost =  "https://cyanlabs.net/api/Syn3Updater/apim-asbuilt-decode/";
        public const string AsBuiltOutput = AsBuiltBase + "?filename=";
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