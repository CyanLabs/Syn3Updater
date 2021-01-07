using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syn3Updater.Model
{
    public class JsonSettings
    {
        public string CurrentSyncRegion { get; set; }
        public string Lang { get; set; }
        public int CurrentSyncVersion { get; set; } = 3319052;
        public bool CurrentSyncNav { get; set; } = false;
        public string DownloadPath { get; set; }
        public string CurrentInstallMode { get; set; }
        public bool ShowAllReleases { get; set; } = false;
        public string LicenseKey { get; set; }
        public bool DisclaimerAccepted { get; set; } = false;
        public string Theme { get; set; }
    }
}
