using System.Collections.Generic;

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

        public const int BlacklistedVersion = 3419274;
        public const int ReformatVersion = 3200000;

        public const string ReformatToolUrl = "https://cyanlabs.net/api/Syn3Updater/reformat.php";
        public const string ReformatToolFileName = "1u5t-14g386-cb.tar.gz";
        public const string ReformatToolName = "1u5t-14g386-cb";
        public const string ReformatToolMd5 = "75E08C3EED8D2039BAF65B6156F79106";

        public const string DowngradeAppUrl = "https://ivsubinaries.azureedge.net/swparts/4U5T-14G381-AN_1552583626000.TAR.GZ";
        public const string DowngradeAppFileName = "4U5T-14G381-AN_1552583626000.TAR.GZ";
        public const string DowngradeAppName = "4U5T-14G381-AN";
        public const string DowngradeAppMd5 = "0553D1A474FBF9F0DB68A9C96FBDA7CB";

        public const string DowngradeToolUrl = "https://ivsubinaries.azureedge.net/swparts/GB5T-14G386-SC_85041.tar.gz";
        public const string DowngradeToolFileName = "GB5T-14G386-SC_85041.tar.gz";
        public const string DowngradeToolName = "GB5T-14G386-SC";
        public const string DowngradeToolMd5 = "E16F5E01D816E738E2B68592BDC22F3F";

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

        #endregion
    }
}