using System.Collections.Generic;
using Newtonsoft.Json;
//TODO Sort Nullables
namespace Syn3Updater.Models
{

    public class ReleasesRoot
        {
            [JsonProperty("releases")] public IList<Release> Releases { get; set; }
            [JsonProperty("map_releases")] public IList<Release> MapReleases { get; set; }
        }
        
        public class IvsuRoot
        {
            [JsonProperty("ivsu")] public IList<Ivsu>? Ivsus { get; set; }
        }

        public class NoticesRoot
        {
            [JsonProperty("notices")] public IList<Notice> Notices { get; set; }
        }
        
        public class UseragentRoot
        {
            [JsonProperty("useragents")] public IList<UserAgent> UserAgents { get; set; }
        }
        
        public class Release
        {
            [JsonProperty("id")] public int Id { get; set; }
            [JsonProperty("name")] public string Name { get; set; }
            [JsonProperty("map_version")] public string MapVersion { get; set; }
            [JsonProperty("notes")] public string Notes { get; set; }
            [JsonProperty("regions")] public IList<string> Regions { get; set; }
            [JsonProperty("version")] public string Version { get; set; }
            [JsonProperty("feedbackurl")] public string Feedbackurl { get; set; }
            [JsonProperty("status")] public string Status { get; set; }
            [JsonProperty("autoinstall")] public string Autoinstall { get; set; }
            [JsonProperty("ivsus")] public IList<ReleasesIvsus> IvsusList { get; set; }
        }
        
        public class ReleasesIvsus
        {
            [JsonProperty("id")] public int Id { get; set; }
            [JsonProperty("ivsu")] public Ivsu? Ivsu { get; set; }
            [JsonProperty("map_ivsu")] public Ivsu? MapIvsu { get; set; }
            [JsonProperty("release")] public Release Release { get; set; }
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
            [JsonProperty("filesize")] public long FileSize { get; set; }
            [JsonProperty("source")] public string Source { get; set; }
            [JsonProperty("specialpackage")] public string SpecialPackage { get; set; }
        }

        public class UserAgent
        {
            [JsonProperty("useragent")] public string Useragent { get; set; }
            [JsonProperty("min")] public int Min { get; set; }
            [JsonProperty("max")] public int Max { get; set; }
        }

        
        public class My20Models
        {
            [JsonProperty("model")] public string Model { get; set; }
        }

        public class My20ModelsRoot
        {
            [JsonProperty("my20models")] public List<My20Models> My20Models { get; set; }
        }

        public class Notice
        {
            [JsonProperty("id")] public int Id { get; set; }
            [JsonProperty("date_created")] public string DateCreated { get; set; }
            [JsonProperty("date_updated")] public string DateUpdated { get; set; }
            [JsonProperty("notice")] public string NoticeContent { get; set; }
            [JsonProperty("title")] public string Title { get; set; }
            [JsonProperty("enabled")] public bool Enabled { get; set; }
            [JsonProperty("important")] public bool Important { get; set; }
        }

        public class Changelogs
        {
            [JsonProperty("data")] public IList<Changelog> Changelog { get; set; }
        }

        public class Changelog
        {
            [JsonProperty("version")] public string Version { get; set; }
            [JsonProperty("date")] public string Date { get; set; }
            [JsonProperty("changelog")] public string ReleaseNotes { get; set; }
            [JsonProperty("branch")] public string Branch { get; set; }
        }
        
        public class CountriesRoot
        {
            [JsonProperty("countries")] public IList<Country> Countries { get; set; }
        }
        
        public class Country
        {
            [JsonProperty("region")] public string Region { get; set; }
            [JsonProperty("code")] public string Code { get; set; }
            [JsonProperty("name")] public int Name { get; set; }
        }
}