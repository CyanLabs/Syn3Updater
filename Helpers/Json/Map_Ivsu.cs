using System.Collections.Generic;

namespace Sync3Updater.Helpers
{
    public class Map_Ivsu
    {
        public int id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public IList<string> regions { get; set; }
        public string md5 { get; set; }
        public string url { get; set; }
    }
}