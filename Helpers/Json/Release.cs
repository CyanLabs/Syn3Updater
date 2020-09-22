using System.Collections.Generic;

namespace Syn3Updater.Helpers
{
    public class Release
    {
        public int id { get; set; }
        public string name { get; set; }
        public string map_version { get; set; }
        public string notes { get; set; }
        public IList<string> regions { get; set; }
    }
}