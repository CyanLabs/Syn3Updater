using System.Collections.Generic;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace Cyanlabs.Syn3Updater.Model
{
    /// <summary>
    ///     Json Class to parse the AsBuilt and post to it to our API
    /// </summary>
    public static class AsBuilt
    {
        public class DID
        {
            [JsonProperty("@ID")] public string ID { get; set; }
            [JsonProperty("#text")] public string Text { get; set; }
        }

        public class VEHICLE
        {
            [JsonProperty("@MODULE")] public string MODULE { get; set; }
            [JsonProperty("@VIN")] public string VIN { get; set; }
            [JsonProperty("@VEHICLE_ID")] public string VEHICLEID { get; set; }
            [JsonProperty("@VEHICLE_YEAR")] public string VEHICLEYEAR { get; set; }
            public List<DID> DID { get; set; }
        }

        public class DirectConfiguration
        {
            public VEHICLE VEHICLE { get; set; }
        }
    }
}