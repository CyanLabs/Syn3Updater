using System;
using System.Collections;
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
using System.Security.RightsManagement;
using System.Threading.Tasks;
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

        public const string SyncReformatToolUrl = "https://cyanlabs.net/api/Syn3Updater/reformat.php";
        public const string SyncReformatToolFileName = "1u5t-14g386-cb.tar.gz";
        public const string SyncReformatToolName = "1u5t-14g386-cb"; 
        public const string SyncReformatToolMd5 = "75E08C3EED8D2039BAF65B6156F79106";

        public const string DowngradePackageAppUrl = "https://ivsubinaries.azureedge.net/swparts/4U5T-14G381-AN_1552583626000.TAR.GZ";
        public const string DowngradePackageAppFileName = "4U5T-14G381-AN_1552583626000.TAR.GZ";
        public const string DowngradePackageAppName = "4U5T-14G381-AN"; 
        public const string DowngradePackageAppMd5 = "0553D1A474FBF9F0DB68A9C96FBDA7CB";

        public const string DowngradePackageToolUrl = "https://ivsubinaries.azureedge.net/swparts/GB5T-14G386-SC_85041.tar.gz";
        public const string DowngradePackageToolFileName = "GB5T-14G386-SC_85041.tar.gz";
        public const string DowngradePackageToolName = "GB5T-14G386-SC"; 
        public const string DowngradePackageToolMd5 = "E16F5E01D816E738E2B68592BDC22F3F";

        private bool _appsselected;

        public void ReloadSettings()
        {
            CurrentSyncVersion = Properties.Settings.Default.CurrentSyncVersion.ToString();
            CurrentSyncNav = Properties.Settings.Default.CurrentSyncNav ? "Yes" : "No";
            CurrentSyncRegion = Properties.Settings.Default.CurrentSyncRegion;
            DownloadLocation = ApplicationManager.Instance.DownloadLocation;
        }
        public void Init()
        {
            CurrentSyncVersion = Properties.Settings.Default.CurrentSyncVersion.ToString();
            CurrentSyncNav = Properties.Settings.Default.CurrentSyncNav ? "Yes" : "No";
            CurrentSyncRegion = Properties.Settings.Default.CurrentSyncRegion;
            DownloadLocation = ApplicationManager.Instance.DownloadLocation;

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
            SyncVersionsEnabled = false;
            RefreshUsb();
            IvsuList = new ObservableCollection<Ivsu>{};
            OnPropertyChanged("IvsuList");
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
                    "https://translate.google.co.uk/translate?hl=&sl=en&tl="+Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName+"&u=https%3A%2F%2Fcyanlabs.net%2Fapi%2FSyn3Updater%2Fregion.php");
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
            IvsuList.Clear();
            SelectedMapVersion = null;
            SelectedRelease = null;
            OnPropertyChanged("SelectedRelease");
            OnPropertyChanged("SelectedMapVersion");
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
            if (!Properties.Settings.Default.CurrentSyncNav)
            {
               SyncMapVersion.Add(@"Non Nav APIM");
            }
            else
            {
                if (Properties.Settings.Default.CurrentSyncVersion >= SyncReformatVersion)
                {
                    SyncMapVersion = new ObservableCollection<string> {@"Keep Existing Maps"};
                }
            }
            OnPropertyChanged("SyncMapVersion");

            _jsonReleases = JsonConvert.DeserializeObject<JsonReleases>(_stringReleasesJson);
            SyncVersion = new ObservableCollection<string>();
            foreach (Data item in _jsonReleases.data)
                if (item.regions.Contains(SelectedRegion.Code))
                    SyncVersion.Add(item.name);
            OnPropertyChanged("SyncVersion");

            SyncVersionsEnabled = true;
            OnPropertyChanged("SyncVersionsEnabled");

            StartEnabled = SelectedRelease != null && SelectedRegion != null && SelectedMapVersion != null;
            OnPropertyChanged("StartEnabled");
        }

        private string _stringCompatibility;
        private void UpdateSelectedRelease()
        {
            SelectedMapVersion = null;
            OnPropertyChanged("SelectedMapVersion");
            IvsuList.Clear();
            foreach (Data item in _jsonReleases.data)
                if (item.name == SelectedRelease)
                {
                    _stringCompatibility = item.version.Substring(0, 3);
                    if (item.notes == null) continue;
                    Notes = item.notes.Replace("\n", Environment.NewLine);
                    OnPropertyChanged("Notes");
                }

            _apiMapReleases = _apiMapReleases.Replace("[regionplaceholder]", $"filter[regions]="+SelectedRegion.Code+"&filter[compatibility][contains]="+_stringCompatibility);

            HttpResponseMessage response = Client.GetAsync(_apiMapReleases).Result;
            _stringMapReleasesJson = response.Content.ReadAsStringAsync().Result;

            if (Properties.Settings.Default.CurrentSyncNav)
            {
                SyncMapVersion.Clear();
                SyncMapVersion.Add("No Maps");
                if (Properties.Settings.Default.CurrentSyncNav)
                {
                    if (Properties.Settings.Default.CurrentSyncVersion >= SyncReformatVersion)
                    {
                        SyncMapVersion.Add("Keep Existing Maps");
                    }
                }
                else
                {
                    SyncMapVersion.Add("Non Nav APIM");
                }
                _jsonMapReleases = JsonConvert.DeserializeObject<JsonReleases>(_stringMapReleasesJson);
                foreach (Data item in _jsonMapReleases.data)
                {
                    SyncMapVersion.Add(item.name);
                }
            }
            SyncMapVersionsEnabled = true;
            OnPropertyChanged("SyncMapVersionsEnabled");
            StartEnabled = SelectedRelease != null && SelectedRegion != null && SelectedMapVersion != null;
            OnPropertyChanged("StartEnabled");
        }

        private void UpdateSelectedMapVersion()
        {
            IvsuList.Clear();
            //ResetControls();

            //LESS THAN 3.2
            if (Properties.Settings.Default.CurrentSyncVersion < SyncReformatVersion)
            {
                InstallMode = "reformat";
            }

            //Above 3.2 and  Below 3.4.19274
            else if (Properties.Settings.Default.CurrentSyncVersion >= SyncReformatVersion && Properties.Settings.Default.CurrentSyncVersion < SyncBlacklistedVersion)
            {
                //Update Nav?
                if (SelectedMapVersion == @"No Maps" || SelectedMapVersion == @"Non Nav APIM" || SelectedMapVersion == @"Keep Existing Maps")
                {
                    InstallMode = Properties.Settings.Default.CurrentInstallMode == "automatic" ? @"autoinstall" : Properties.Settings.Default.CurrentInstallMode;
                }
                else
                {
                    InstallMode = Properties.Settings.Default.CurrentInstallMode == "automatic" ? @"reformat" : Properties.Settings.Default.CurrentInstallMode;
                }
            }

            //3.4.19274 or above
            else if (Properties.Settings.Default.CurrentSyncVersion >= SyncBlacklistedVersion)
            {
                //Update Nav?
                if (SelectedMapVersion == @"No Maps" || SelectedMapVersion == @"Non Nav APIM" || SelectedMapVersion == @"Keep Existing Maps")
                {
                    InstallMode = Properties.Settings.Default.CurrentInstallMode == "automatic" ? @"autoinstall" : Properties.Settings.Default.CurrentInstallMode;
                }
                else
                {
                    InstallMode = Properties.Settings.Default.CurrentInstallMode == "automatic" ? @"downgrade" : Properties.Settings.Default.CurrentInstallMode;
                }
            }
            OnPropertyChanged("InstallMode");

            if (InstallMode == @"downgrade")
            {
                IvsuList.Add(new Ivsu()
                {
                    Type = "APP",
                    Name = DowngradePackageAppName,
                    Version = "",
                    Notes = "REQUIRED!",
                    Url = DowngradePackageAppUrl,
                    Md5 = DowngradePackageAppMd5,
                    Selected = true,
                    FileName = DowngradePackageAppFileName
                });

                IvsuList.Add(new Ivsu()
                {
                    Type = "TOOL",
                    Name = DowngradePackageToolName,
                    Version = "",
                    Notes = "REQUIRED!",
                    Url = DowngradePackageToolUrl,
                    Md5 = DowngradePackageToolMd5,
                    Selected = true,
                    FileName = DowngradePackageToolFileName
                });
            }

            if (InstallMode == @"reformat" || InstallMode == @"downgrade")
            {
                IvsuList.Add(new Ivsu()
                {
                    Type = "TOOL",
                    Name = SyncReformatToolName,
                    Version = "",
                    Notes = "REQUIRED!",
                    Url = SyncReformatToolUrl,
                    Md5 = SyncReformatToolMd5,
                    Selected = true,
                    FileName = SyncReformatToolFileName
                });
            }

            ApplicationManager.Instance.InstallMode = InstallMode;

            HttpResponseMessage response = Client.GetAsync(ApiAppReleaseSingle + SelectedRelease).Result;
            _stringDownloadJson = response.Content.ReadAsStringAsync().Result;

            response = Client.GetAsync(ApiMapReleaseSingle + SelectedMapVersion).Result;
            _stringMapDownloadJson = response.Content.ReadAsStringAsync().Result;

            JsonReleases jsonIvsUs = JsonConvert.DeserializeObject<JsonReleases>(_stringDownloadJson);
            JsonReleases jsonMapIvsUs = JsonConvert.DeserializeObject<JsonReleases>(_stringMapDownloadJson);

            foreach (Ivsus item in jsonIvsUs.data[0].ivsus)
                if (item.ivsu.regions.Contains(@"ALL") || item.ivsu.regions.Contains(SelectedRegion.Code))
                {
                    string _fileName = item.ivsu.url.Substring(item.ivsu.url.LastIndexOf("/", StringComparison.Ordinal) + 1,
                        item.ivsu.url.Length - item.ivsu.url.LastIndexOf("/", StringComparison.Ordinal) - 1);
                    IvsuList.Add(new Ivsu() { Type = item.ivsu.type, Name = item.ivsu.name, Version = item.ivsu.version, Notes = item.ivsu.notes, Url = item.ivsu.url, Md5 = item.ivsu.md5, Selected = true, FileName = _fileName });
                }

            if (SelectedMapVersion != @"No Maps" && SelectedMapVersion != @"Non Nav APIM" && SelectedMapVersion != @"Keep Existing Maps")
                foreach (Ivsus item in jsonMapIvsUs.data[0].ivsus)
                    if (item.map_ivsu.regions.Contains(@"ALL") || item.map_ivsu.regions.Contains(SelectedRegion.Code))
                    {
                        string _fileName = item.map_ivsu.url.Substring(item.map_ivsu.url.LastIndexOf("/", StringComparison.Ordinal) + 1,
                            item.map_ivsu.url.Length - item.map_ivsu.url.LastIndexOf("/", StringComparison.Ordinal) - 1);
                        IvsuList.Add(new Ivsu() { Type = item.map_ivsu.type, Name = item.map_ivsu.name, Version = item.map_ivsu.version, Notes = item.map_ivsu.notes, Url = item.map_ivsu.url, Md5 = item.map_ivsu.md5, Selected = true, FileName = _fileName });
                    }
            OnPropertyChanged("IvsuList");
            StartEnabled = SelectedRelease != null && SelectedRegion != null && SelectedMapVersion != null;
            OnPropertyChanged("StartEnabled");
        }

        private void StartAction()
        {
            //DisableControls();
            ApplicationManager.Instance._ivsus = new ObservableCollection<Ivsu>();
            foreach (Ivsu item in IvsuList)
            {
                if (item.Selected)
                {
                    if (item.Type == "APPS")
                    {
                        _appsselected = true;
                    }
//#if DEBUG
                    Uri myUri = new Uri(item.Url);
                    item.Url = item.Url.Replace(myUri.Host,"127.0.0.1").Replace(myUri.Scheme,"http");  // host is "www.contoso.com"
//#endif
                    ApplicationManager.Instance._ivsus.Add(item);
                }
            }

            if (!CancelledDownload())
            {
                ApplicationManager.Instance.drivenumber = SelectedDrive.Path.Replace("Win32_DiskDrive.DeviceID=\"\\\\\\\\.\\\\PHYSICALDRIVE", "").Replace("\"", "");
                ApplicationManager.Instance.driveletter = DriveLetter;
                ApplicationManager.Instance.selectedregion = SelectedRegion.Code;
                ApplicationManager.Instance.selectedrelease = SelectedRelease;
                ApplicationManager.Instance.selectedmapversion = SelectedMapVersion;
                ApplicationManager.Instance.FireDownloadsTabEvent();
                ApplicationManager.Logger.Info($@"Starting process ({SelectedRelease} - {SelectedRegion} - {SelectedMapVersion})");
                StartEnabled = false;
                OnPropertyChanged("StartEnabled");
            }
        }

        private bool _canceldownload;
        private bool CancelledDownload()
        {
            //No USB drive selected, download only?
            if (SelectedDrive == null && _canceldownload == false)
            {
                if (MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelNoUSB"), "Syn3 Updater",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    ApplicationManager.Instance._downloadonly = true;
                }
                else
                {
                    _canceldownload = true;
                }
            }

            if ((InstallMode == "reformat" || InstallMode == "downgrade") && _canceldownload == false && ApplicationManager.Instance._downloadonly == false)
            {
                if (MessageBox.Show(string.Format(LanguageManager.GetValue("MessageBox.CancelMy20"), InstallMode), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    if (MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelMy20Final"), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                        _canceldownload = true;
                }
                else
                {
                    _canceldownload = true;
                }
            }

            //Check region is the same.
            if (SelectedRegion.Code != Properties.Settings.Default.CurrentSyncRegion && _canceldownload == false)
            {
                if (MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelRegionMismatch"), "Syn3 Updater",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                {
                    _canceldownload = true;
                }
            }


            if (!string.IsNullOrEmpty(DriveLetter))
            {
                if (DownloadLocation.Contains(DriveLetter) && _canceldownload == false)
                    MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelDownloadIsDrive"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            if (_appsselected == false && _canceldownload == false && (InstallMode == "reformat" || InstallMode == "downgrade"))
                MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelNoApps"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            // ReSharper disable once InvertIf
            if (ApplicationManager.Instance._downloadonly == false && _canceldownload == false)
            {
                if (MessageBox.Show(string.Format(LanguageManager.GetValue("MessageBox.CancelFormatUSB"), SelectedDrive.Name, DriveLetter), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    _canceldownload = true;
            }
            return _canceldownload;
        }

        private ActionCommand _refreshUSB;
        public ActionCommand RefreshUSB => _refreshUSB ?? (_refreshUSB = new ActionCommand(RefreshUsb));

        private ActionCommand _regionInfo;
        public ActionCommand RegionInfo => _regionInfo ?? (_regionInfo = new ActionCommand(RegionInfoAction));

        private ActionCommand _startButton;
        public ActionCommand StartButton => _startButton ?? (_startButton = new ActionCommand(StartAction));

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
                if(value != null) UpdateSelectedRegion();
            }
        }

        private string _selectedRelease;

        public string SelectedRelease
        {
            get => _selectedRelease;
            set
            {
                SetProperty(ref _selectedRelease, value);
                if (value != null) UpdateSelectedRelease();
            }
        }

        private string _selectedMapVersion;

        public string SelectedMapVersion
        {
            get => _selectedMapVersion;
            set
            {
                SetProperty(ref _selectedMapVersion, value);
                if (value != null) UpdateSelectedMapVersion();
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

        public bool SyncVersionsEnabled { get; set; }

        public bool SyncMapVersionsEnabled { get; set; }

        public string Notes { get; set; }

        public ObservableCollection<Ivsu> IvsuList { get; set; }

        public class Ivsu
        {
            public bool Selected { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public string Version { get; set; }
            public string Notes { get; set; }
            public string Url { get; set; }
            public string Md5 { get; set; }
            
            public string FileName { get; set; }
        }

        private string _currentSyncRegion;

        public string CurrentSyncRegion
        {
            get => _currentSyncRegion;
            set
            {
                if (value != null)
                {
                    SetProperty(ref _currentSyncRegion, value);
                    Properties.Settings.Default.CurrentSyncRegion = value;
                }
            }
        }

        private string _currentSyncVersion;

        public string CurrentSyncVersion
        {
            get => _currentSyncVersion;
            set
            {
                SetProperty(ref _currentSyncVersion, value);
            }
        }

        private string _currentSyncNav;

        public string CurrentSyncNav
        {
            get => _currentSyncNav;
            set
            {
                SetProperty(ref _currentSyncNav, value);
            }
        }

        private string _downloadLocation;

        public string DownloadLocation
        {
            get => _downloadLocation;
            set
            {
                if (value != null)
                {
                    SetProperty(ref _downloadLocation, value);
                }
            }
        }

        public string InstallMode { get; set; }

        public bool StartEnabled { get; set; }

        MainWindowViewModel Parent { get; set; }
    }
}