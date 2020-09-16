using System.Collections.Generic;

namespace Syn3Updater.Helpers
{
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
}