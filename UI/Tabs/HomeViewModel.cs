using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using Microsoft.VisualBasic.FileIO;
using Syn3Updater.Helper;
using Syn3Updater.Model;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using AutoUpdaterDotNET;
using Newtonsoft.Json;
using Syn3Updater.Helpers;

namespace Syn3Updater.UI.Tabs
{
    public class HomeViewModel : LanguageAwareBaseViewModel
    {
        private const string ApiBase = "https://api.cyanlabs.net/fordsyncdownloader/";
        private const string ApiAppReleasesConst = ApiBase + "items/releases?sort=-name&limit=-1&[published]";
        private const string ApiMapReleasesConst = ApiBase + "items/map_releases?sort=-name&limit=-1&[regionplaceholder]&[published]";
        private string _apiAppReleases;
        private string _apiMapReleases;
        private const string ApiAppReleaseSingle = ApiBase + "items/releases?sort=-name&limit=-1&fields=*.*.*&filter[name]=";
        private const string ApiMapReleaseSingle = ApiBase + "items/map_releases?sort=-name&limit=-1&fields=*.*.*&filter[name]=";
        private const string ApiToken = "hjuBpZko7BUAFeA2NnJsuhO6";

        private static readonly HttpClient Client = new HttpClient();

        private JsonReleases _jsonMapReleases, _jsonReleases;

        private string _stringReleasesJson, _stringMapReleasesJson, _stringDownloadJson, _stringMapDownloadJson;

        private const int SyncBlacklistedVersion = 3419274;
        private const int SyncReformatVersion = 3200000;

        public void Init()
        {
            Properties.Settings.Default.CurrentInstallMode = "automatic";
            Properties.Settings.Default.Save();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiToken);
            SyncRegions = new ObservableCollection<SyncRegion>
            {
                new SyncRegion {Code = "EU", Name = "Europe"},
                new SyncRegion {Code = "NA", Name = "North America & Canada"},
                new SyncRegion {Code = "CN", Name = "China"},
                new SyncRegion {Code = "ANZ", Name = "Australia & New Zealand"},
                new SyncRegion {Code = "ROW", Name = "Rest Of World"}
            };
            OnPropertyChanged("SyncRegions");
            RefreshUsb();
        }

        
        private void RegionInfoAction()
        {
            if (Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "en")
            {
                Process.Start("https://cyanlabs.net/api/Syn3Updater/region.php");
            }
            else
            {
                Process.Start(
                    "https://translate.google.co.uk/translate?hl=&sl=en&tl={Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName}&u=https%3A%2F%2Fcyanlabs.net%2Fapi%2FSyn3Updater%2Fregion.php");
            }
        }

        private void RefreshUsb()
        {
            DriveList = new ObservableCollection<Drive>();
            ManagementObjectSearcher driveQuery =
                new ManagementObjectSearcher("select * from Win32_DiskDrive WHERE InterfaceType='USB'");
            foreach (ManagementBaseObject o in driveQuery.Get())
            {
                ManagementObject d = (ManagementObject)o;
                string diskName = Convert.ToString(d.Properties["Caption"].Value);
                string friendlySize = Functions.BytesToString(Convert.ToInt64(d.Properties["Size"].Value));
                if (friendlySize != "0B")
                {
                    DriveList.Add(new Drive { Path = d.Path.RelativePath, Name = diskName + " " + friendlySize});
                }
            }
            OnPropertyChanged("DriveList");
        }

        private void UpdateDriveInfo()
        {
            if (SelectedDrive == null)
            {
                DriveLetter = "";
                DriveName = "";
                DriveFileSystem = "";
            }
            else
            {
                string partitionQueryText = $@"associators of {{{SelectedDrive.Path}}} where AssocClass = Win32_DiskDriveToDiskPartition";
                ManagementObjectSearcher partitionQuery = new ManagementObjectSearcher(partitionQueryText);
                try
                {
                    foreach (ManagementBaseObject o in partitionQuery.Get())
                    {
                        ManagementObject p = (ManagementObject)o;
                        string logicalDriveQueryText = $@"associators of {{{p.Path.RelativePath}}} where AssocClass = Win32_LogicalDiskToPartition";
                        ManagementObjectSearcher logicalDriveQuery = new ManagementObjectSearcher(logicalDriveQueryText);
                        foreach (ManagementBaseObject managementBaseObject in logicalDriveQuery.Get())
                        {
                            ManagementObject ld = (ManagementObject)managementBaseObject;
                            DriveLetter = Convert.ToString(ld.Properties["DeviceId"].Value);
                            DriveFileSystem = p.Properties["Type"].Value.ToString().Contains("GPT:") ? "GPT" : "MBR";
                            DriveFileSystem += " " + Convert.ToString(ld.Properties["FileSystem"].Value);
                            DriveName = Convert.ToString(ld.Properties["VolumeName"].Value);
                        }
                    }
                }
                catch (ManagementException)
                {
                    //TODO Implement Catch
                }
            }
            OnPropertyChanged("DriveLetter");
            OnPropertyChanged("DriveName");
            OnPropertyChanged("DriveFileSystem");
        }

        private void UpdateSelectedRegion() 
        {
            if (Properties.Settings.Default.ShowAllReleases)
            {
                _apiMapReleases = ApiMapReleasesConst.Replace("[published]", $"filter[key][in]=public,{Properties.Settings.Default.LicenseKey}");
                _apiAppReleases = ApiAppReleasesConst.Replace("[published]", $"filter[key][in]=public,{Properties.Settings.Default.LicenseKey}");
                //https://api.cyanlabs.net/fordsyncdownloader/items/map_releases?sort=-name&limit=-1&filter[regions]=ANZ&filter[compatibility][contains]=3.4&filter[status][in]=published,private&filter[key][in]=admin@cyanlabs.net,public
            }
            else
            {
                _apiMapReleases = ApiMapReleasesConst.Replace("[published]", $"filter[status][in]=published,private&filter[key][in]=public,{Properties.Settings.Default.LicenseKey}");
                _apiAppReleases = ApiAppReleasesConst.Replace("[published]", $"filter[status][in]=published,private&filter[key][in]=public,{Properties.Settings.Default.LicenseKey}");
            }

            try
            {
                HttpResponseMessage response = Client.GetAsync(_apiAppReleases).Result;
                _stringReleasesJson = response.Content.ReadAsStringAsync().Result;
            }
            catch (WebException webex)
            {
                throw webex;
                //TODO Exception handling
            }

            SyncVersion?.Clear();
            SyncMapVersion?.Clear();
            if (!Properties.Settings.Default.CurrentSyncNav)
            {
               SyncMapVersion.Add(@"Non Nav APIM");
               SelectedMapVersion = "Non Nav APIM";
            }
            else
            {
                if (Properties.Settings.Default.CurrentSyncVersion >= SyncReformatVersion)
                {
                    SyncMapVersion = new ObservableCollection<string>();
                    SyncMapVersion.Add(@"Keep Existing Maps"); 
                    OnPropertyChanged("SyncMapVersion");
                    
                    SelectedMapVersion = "Keep Existing Maps";
                }
            }
            
            _jsonReleases = JsonConvert.DeserializeObject<JsonReleases>(_stringReleasesJson);
            SyncVersion = new ObservableCollection<string>();
            foreach (Data item in _jsonReleases.data)
                if (item.regions.Contains(SelectedRegion.Code))
                    SyncVersion.Add(item.name);
            OnPropertyChanged("SyncVersion");
        }

        private ActionCommand _refreshUSB;
        public ActionCommand RefreshUSB => _refreshUSB ?? (_refreshUSB = new ActionCommand(RefreshUsb));

        private ActionCommand _regionInfo;
        public ActionCommand RegionInfo => _regionInfo ?? (_regionInfo = new ActionCommand(RegionInfoAction));

        public string DriveLetter { get; set; }
        public string DriveName { get; set; }
        public string DriveFileSystem { get; set; }

        private Drive _selectedDrive;

        public Drive SelectedDrive
        {
            get => _selectedDrive;
            set
            {
                SetProperty(ref _selectedDrive, value);
                UpdateDriveInfo();
            }
        }

        private SyncRegion _selectedRegion;

        public SyncRegion SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                SetProperty(ref _selectedRegion, value);
                UpdateSelectedRegion();
            }
        }

        public ObservableCollection<Drive> DriveList { get; set; }
        public class Drive
        {
            public string Path { get; set; }
            public string Name { get; set; }
        }

        public ObservableCollection<SyncRegion> SyncRegions { get; set; }
        public class SyncRegion
        {
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public ObservableCollection<string> SyncVersion { get; set; }
        public ObservableCollection<string> SyncMapVersion { get; set; }

        public string SelectedMapVersion { get; set; }
    }
}