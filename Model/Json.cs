using System.Collections.Generic;

// ReSharper disable InconsistentNaming

namespace Syn3Updater.Model
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

    public class Maps
    {
        public string EU { get; set; }
        public string NA { get; set; }
        public string CN { get; set; }
        public string ANZ { get; set; }
        public string ROW { get; set; }
    }

    public class Release
    {
        public int id { get; set; }
        public string name { get; set; }
        public string map_version { get; set; }
        public string notes { get; set; }
        public IList<string> regions { get; set; }
    }
}