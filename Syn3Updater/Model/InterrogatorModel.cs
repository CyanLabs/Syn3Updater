using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cyanlabs.Syn3Updater.Model
{
    /// <summary>
    ///     Json Class to parse the Interrogator log Direct Configuration XML to JSON
    /// </summary>
    public class Interrogator
    {
        public class InterrogatorModel
        {
            [JsonProperty("?xml")] public Xml Xml { get; set; }
            [JsonProperty("p:OTAModuleSnapShot")] public POtaModuleSnapShot POtaModuleSnapShot { get; set; }
        }

        public class POtaModuleSnapShot
        {
            [JsonProperty("@xmlns:d2p1")] public string XmlnsD2P1 { get; set; }
            [JsonProperty("@xmlns:xsi")] public Uri XmlnsXsi { get; set; }
            [JsonProperty("@xmlns:p")] public string XmlnsP { get; set; }
            [JsonProperty("@version")] public DateTimeOffset Version { get; set; }
            [JsonProperty("@xsi:schemaLocation")] public string XsiSchemaLocation { get; set; }
            [JsonProperty("p:VIN")] public string PVin { get; set; }
            [JsonProperty("p:ModuleName")] public string PModuleName { get; set; }
            [JsonProperty("p:RequestRole")] public PRequestRole PRequestRole { get; set; }
            [JsonProperty("p:BroadcastDTCType")] public object PBroadcastDtcType { get; set; }
            [JsonProperty("p:Node")] public PNode PNode { get; set; }
        }

        public class PNode
        {
            [JsonProperty("@isFlashed")] public bool IsFlashed { get; set; }
            [JsonProperty("@specificationCategory")] public string SpecificationCategory { get; set; }
            [JsonProperty("d2p1:Address")] public string D2P1Address { get; set; }
            [JsonProperty("d2p1:ECUAcronym")] public D2P1EcuAcronym D2P1EcuAcronym { get; set; }
            [JsonProperty("d2p1:ODLNetwork")] public D2P1OdlNetwork D2P1OdlNetwork { get; set; }
            [JsonProperty("d2p1:AdditionalAttributes")] public D2P1AdditionalAttributes D2P1AdditionalAttributes { get; set; }
        }

        public class D2P1AdditionalAttributes
        {
            [JsonProperty("@logGeneratedDateTime")]
            public DateTimeOffset LogGeneratedDateTime { get; set; }
            [JsonProperty("@RAM")] public long Ram { get; set; }
            [JsonProperty("@vmcuVersion")] public string VmcuVersion { get; set; }
            [JsonProperty("d2p1:PartitionHealth")] public D2P1PartitionHealth[] D2P1PartitionHealth { get; set; }
            [JsonProperty("d2p1:InstallationLog")] public object D2P1InstallationLog { get; set; }
            [JsonProperty("d2p1:SyncData")] public string D2P1SyncData { get; set; }
        }

        public class D2P1PartitionHealth
        {
            [JsonProperty("@type")] public string Type { get; set; }
            [JsonProperty("@total")] public string Total { get; set; }
            [JsonProperty("@available")] public string Available { get; set; }
        }

        public class D2P1EcuAcronym
        {
            [JsonProperty("@name")] public string Name { get; set; }
            [JsonProperty("d2p1:State")] public D2P1State D2P1State { get; set; }
        }

        public class D2P1State
        {
            [JsonProperty("d2p1:Gateway")] public D2P1Gateway D2P1Gateway { get; set; }
        }

        public class D2P1Gateway
        {
            [JsonProperty("@gatewayType")] public string GatewayType { get; set; }
            [JsonProperty("d2p1:DID")] public D2P1Did[] D2P1Did { get; set; }
        }

        public class D2P1Did
        {
            [JsonProperty("@didFormat", NullValueHandling = NullValueHandling.Ignore)]
            public string DidFormat { get; set; }
            [JsonProperty("@didType")] public string DidType { get; set; }
            [JsonProperty("@didValue")] public string DidValue { get; set; }
            [JsonProperty("@responseLength", NullValueHandling = NullValueHandling.Ignore)] public long? ResponseLength { get; set; }
            [JsonProperty("d2p1:Response")] public string D2P1Response { get; set; }
            [JsonProperty("d2p1:IsConfig")] public bool D2P1IsConfig { get; set; }
        }

        public class D2P1OdlNetwork
        {
            [JsonProperty("@d2p1:NetworkDataRate")] public long D2P1NetworkDataRate { get; set; }
            [JsonProperty("@d2p1:NetworkName")] public string D2P1NetworkName { get; set; }
            [JsonProperty("@d2p1:NetworkProtocol")] public string D2P1NetworkProtocol { get; set; }
            [JsonProperty("@d2p1:DLCName")] public string D2P1DlcName { get; set; }
            [JsonProperty("@d2p1:Pins")] public string D2P1Pins { get; set; }
        }

        public class PRequestRole
        {
            [JsonProperty("d2p1:Role")] public string D2P1Role { get; set; }
            [JsonProperty("d2p1:RoleSource")] public string D2P1RoleSource { get; set; }
            [JsonProperty("d2p1:RoleDesc")] public string D2P1RoleDesc { get; set; }
            [JsonProperty("d2p1:RoleID")] public string D2P1RoleId { get; set; }
        }

        public class Xml
        {
            [JsonProperty("@version")] public string Version { get; set; }
            [JsonProperty("@encoding")] public string Encoding { get; set; }
        }
    }
}