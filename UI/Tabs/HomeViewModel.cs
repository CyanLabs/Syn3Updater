using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Windows;
using Newtonsoft.Json;
using Syn3Updater.Helper;
using Syn3Updater.Model;

namespace Syn3Updater.UI.Tabs
{
    public class HomeViewModel : LanguageAwareBaseViewModel
    {
        private const string ApiBase = "https://api.cyanlabs.net/fordsyncdownloader/";
        private const string ApiToken = "hjuBpZko7BUAFeA2NnJsuhO6";

        private const string ApiAppReleasesConst = ApiBase + "items/releases?sort=-name&limit=-1&[published]";
        private const string ApiMapReleasesConst = ApiBase + "items/map_releases?sort=-name&limit=-1&[regionplaceholder]&[published]";
        private const string ApiAppReleaseSingle = ApiBase + "items/releases?sort=-name&limit=-1&fields=*.*.*&filter[name]=";
        private const string ApiMapReleaseSingle = ApiBase + "items/map_releases?sort=-name&limit=-1&fields=*.*.*&filter[name]=";

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

        private static readonly HttpClient Client = new HttpClient();
        private string _apiAppReleases;
        private string _apiMapReleases;

        private bool _appsselected;

        private bool _canceldownload;

        private string _currentSyncNav;

        private string _currentSyncRegion;

        private string _currentSyncVersion;

        private string _downloadLocation;
        private string _driveFileSystem;
        private string _driveLetter;

        private ObservableCollection<Drive> _driveList;
        private string _driveName;

        private JsonReleases _jsonMapReleases, _jsonReleases;

        private ActionCommand _refreshUSB;

        private ActionCommand _regionInfo;

        private Drive _selectedDrive;

        private string _selectedMapVersion;

        private int _selectedMapVersionIndex;

        private SyncRegion _selectedRegion;

        private string _selectedRelease;

        private int _selectedReleaseIndex;

        private ActionCommand _startButton;

        private string _stringCompatibility;

        private string _stringReleasesJson, _stringMapReleasesJson, _stringDownloadJson, _stringMapDownloadJson;
        private ObservableCollection<SyncRegion> _syncRegions;
        private ObservableCollection<string> _syncVersion;
        private ObservableCollection<string> _syncMapVersion;
        private bool _syncVersionsEnabled;
        private bool _syncMapVersionsEnabled;
        private string _notes;
        private ObservableCollection<LocalIvsu> _ivsuList;
        private string _installMode;
        private bool _startEnabled;

        public ActionCommand RefreshUSB => _refreshUSB ?? (_refreshUSB = new ActionCommand(RefreshUsb));
        public ActionCommand RegionInfo => _regionInfo ?? (_regionInfo = new ActionCommand(RegionInfoAction));
        public ActionCommand StartButton => _startButton ?? (_startButton = new ActionCommand(StartAction));

        public string DriveLetter
        {
            get => _driveLetter;
            set => SetProperty(ref _driveLetter, value);
        }

        public string DriveName
        {
            get => _driveName;
            set => SetProperty(ref _driveName, value);
        }

        public string DriveFileSystem
        {
            get => _driveFileSystem;
            set => SetProperty(ref _driveFileSystem, value);
        }

        public Drive SelectedDrive
        {
            get => _selectedDrive;
            set
            {
                SetProperty(ref _selectedDrive, value);
                UpdateDriveInfo();
            }
        }

        public SyncRegion SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                SetProperty(ref _selectedRegion, value);
                if (value != null) UpdateSelectedRegion();
            }
        }

        public string SelectedRelease
        {
            get => _selectedRelease;
            set
            {
                SetProperty(ref _selectedRelease, value);
                if (value != null) UpdateSelectedRelease();
            }
        }

        public string SelectedMapVersion
        {
            get => _selectedMapVersion;
            set
            {
                SetProperty(ref _selectedMapVersion, value);
                if (value != null) UpdateSelectedMapVersion();
            }
        }

        public int SelectedMapVersionIndex
        {
            get => _selectedMapVersionIndex;
            set => SetProperty(ref _selectedMapVersionIndex, value);
        }

        public int SelectedReleaseIndex
        {
            get => _selectedReleaseIndex;
            set => SetProperty(ref _selectedReleaseIndex, value);
        }

        public ObservableCollection<Drive> DriveList
        {
            get => _driveList;
            set => SetProperty(ref _driveList, value);
        }

        public ObservableCollection<SyncRegion> SyncRegions
        {
            get => _syncRegions;
            set => SetProperty(ref _syncRegions, value);
        }

        public ObservableCollection<string> SyncVersion
        {
            get => _syncVersion;
            set => SetProperty(ref _syncVersion, value);
        }

        public ObservableCollection<string> SyncMapVersion
        {
            get => _syncMapVersion;
            set => SetProperty(ref _syncMapVersion, value);
        }

        public bool SyncVersionsEnabled
        {
            get => _syncVersionsEnabled;
            set => SetProperty(ref _syncVersionsEnabled, value);
        }

        public bool SyncMapVersionsEnabled
        {
            get => _syncMapVersionsEnabled;
            set => SetProperty(ref _syncMapVersionsEnabled, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public ObservableCollection<LocalIvsu> IvsuList
        {
            get => _ivsuList;
            set => SetProperty(ref _ivsuList, value);
        }

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

        public string CurrentSyncVersion
        {
            get => _currentSyncVersion;
            set => SetProperty(ref _currentSyncVersion, value);
        }

        public string CurrentSyncNav
        {
            get => _currentSyncNav;
            set => SetProperty(ref _currentSyncNav, value);
        }

        public string DownloadLocation
        {
            get => _downloadLocation;
            set
            {
                if (value != null) SetProperty(ref _downloadLocation, value);
            }
        }

        public string InstallMode
        {
            get => _installMode;
            set => SetProperty(ref _installMode, value);
        }

        public bool StartEnabled
        {
            get => _startEnabled;
            set => SetProperty(ref _startEnabled, value);
        }

        public void ReloadSettings()
        {
            string _version = Properties.Settings.Default.CurrentSyncVersion.ToString();
            string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            if (_version.Length >= 5) CurrentSyncVersion = $"{_version[0]}{decimalSeparator}{_version[1]}{decimalSeparator}{_version.Substring(2, _version.Length - 2)}";
            CurrentSyncNav = Properties.Settings.Default.CurrentSyncNav ? "Yes" : "No";
            CurrentSyncRegion = Properties.Settings.Default.CurrentSyncRegion;
            DownloadLocation = ApplicationManager.Instance.DownloadLocation;
            SelectedMapVersionIndex = -1;
            SelectedReleaseIndex = -1;
            StartEnabled = false;
            //OnPropertyChanged("StartEnabled");
            IvsuList = new ObservableCollection<LocalIvsu>();
            //OnPropertyChanged("IvsuList");
            InstallMode = "";
            //OnPropertyChanged("InstallMode");
        }

        public void Init()
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiToken);
            SyncRegions = new ObservableCollection<SyncRegion>
            {
                new SyncRegion {Code = "EU", Name = "Europe"},
                new SyncRegion {Code = "NA", Name = "North America & Canada"},
                new SyncRegion {Code = "CN", Name = "China"},
                new SyncRegion {Code = "ANZ", Name = "Australia & New Zealand"},
                new SyncRegion {Code = "ROW", Name = "Rest Of World"}
            };
            //OnPropertyChanged("SyncRegions");
            SyncVersionsEnabled = false;
            RefreshUsb();
            SyncMapVersion = new ObservableCollection<string>();
        }

        private void RegionInfoAction()
        {
            Process.Start(Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "en"
                ? "https://cyanlabs.net/api/Syn3Updater/region.php"
                : $"https://translate.google.co.uk/translate?hl=&sl=en&tl={Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName}&u=https%3A%2F%2Fcyanlabs.net%2Fapi%2FSyn3Updater%2Fregion.php");
        }

        private void RefreshUsb()
        {
            DriveList = USBHelper.refresh_devices();
        }

        private void UpdateDriveInfo()
        {
            USBHelper.DriveInfo drive_info = USBHelper.UpdateDriveInfo(SelectedDrive);

            // Update app level vars
            ApplicationManager.Instance.DriveFileSystem = drive_info.FileSystem;
            ApplicationManager.Instance.DrivePartitionType = drive_info.PartitionType;
            ApplicationManager.Instance.DriveName = drive_info.Name;
            ApplicationManager.Instance.SkipFormat = drive_info.SkipFormat;

            // Update local level vars
            DriveLetter = drive_info.Letter;
            DriveFileSystem = drive_info.PartitionType + " " + drive_info.FileSystem;
            DriveName = drive_info.Name;

            ApplicationManager.Logger.Info(
                $"[App] USB Drive selected - Name: {drive_info.Name} - FileSystem: {drive_info.FileSystem} - PartitionType: {drive_info.PartitionType} - Letter: {drive_info.Letter}");
            //OnPropertyChanged("DriveLetter");
            //OnPropertyChanged("DriveName");
            //OnPropertyChanged("DriveFileSystem");
        }

        private void UpdateSelectedRegion()
        {
            ApplicationManager.Logger.Info($"[Settings] Current Sync Details - Region: {CurrentSyncRegion} - Version: {CurrentSyncVersion} - Navigation: {CurrentSyncNav}");
            if (SelectedRegion.Code != "")
            {
                IvsuList.Clear();
                SelectedMapVersion = null;
                SelectedRelease = null;
                //OnPropertyChanged("SelectedRelease");
                //OnPropertyChanged("SelectedMapVersion");
                if (Properties.Settings.Default.ShowAllReleases)
                {
                    _apiMapReleases = ApiMapReleasesConst.Replace("[published]", $"filter[key][in]=public,{Properties.Settings.Default.LicenseKey}");
                    _apiAppReleases = ApiAppReleasesConst.Replace("[published]", $"filter[key][in]=public,{Properties.Settings.Default.LicenseKey}");
                    //https://api.cyanlabs.net/fordsyncdownloader/items/map_releases?sort=-name&limit=-1&filter[regions]=ANZ&filter[compatibility][contains]=3.4&filter[status][in]=published,private&filter[key][in]=admin@cyanlabs.net,public
                }
                else
                {
                    _apiMapReleases = ApiMapReleasesConst.Replace("[published]",
                        $"filter[status][in]=published,private&filter[key][in]=public,{Properties.Settings.Default.LicenseKey}");
                    _apiAppReleases = ApiAppReleasesConst.Replace("[published]",
                        $"filter[status][in]=published,private&filter[key][in]=public,{Properties.Settings.Default.LicenseKey}");
                }

                try
                {
                    HttpResponseMessage response = Client.GetAsync(_apiAppReleases).Result;
                    _stringReleasesJson = response.Content.ReadAsStringAsync().Result;
                }
                // ReSharper disable once RedundantCatchClause
                catch (WebException)
                {
                    throw;
                    //TODO Exception handling
                }

                if (!Properties.Settings.Default.CurrentSyncNav)
                {
                    SyncMapVersion.Add(LanguageManager.GetValue("String.NonNavAPIM"));
                }
                else
                {
                    if (Properties.Settings.Default.CurrentSyncVersion >= SyncReformatVersion)
                        SyncMapVersion.Add(LanguageManager.GetValue("String.KeepExistingMaps"));
                }

                //OnPropertyChanged("SyncMapVersion");

                _jsonReleases = JsonConvert.DeserializeObject<JsonReleases>(_stringReleasesJson);
                SyncVersion = new ObservableCollection<string>();
                foreach (Data item in _jsonReleases.data)
                    if (item.regions.Contains(SelectedRegion.Code))
                        SyncVersion.Add(item.name);
                //OnPropertyChanged("SyncVersion");

                SyncVersionsEnabled = true;
                //OnPropertyChanged("SyncVersionsEnabled");

                StartEnabled = SelectedRelease != null && SelectedRegion != null && SelectedMapVersion != null;
                //OnPropertyChanged("StartEnabled");
            }
        }

        private void UpdateSelectedRelease()
        {
            if (SelectedRelease != "")
            {
                SelectedMapVersion = null;
                //OnPropertyChanged("SelectedMapVersion");
                IvsuList.Clear();
                foreach (Data item in _jsonReleases.data)
                    if (item.name == SelectedRelease)
                    {
                        _stringCompatibility = item.version.Substring(0, 3);
                        if (item.notes == null) continue;
                        Notes = item.notes.Replace("\n", Environment.NewLine);
                        //OnPropertyChanged("Notes");
                    }

                _apiMapReleases = _apiMapReleases.Replace("[regionplaceholder]", $"filter[regions]={SelectedRegion.Code}&filter[compatibility][contains]={_stringCompatibility}");

                HttpResponseMessage response = Client.GetAsync(_apiMapReleases).Result;
                _stringMapReleasesJson = response.Content.ReadAsStringAsync().Result;

                if (Properties.Settings.Default.CurrentSyncNav)
                {
                    SyncMapVersion.Clear();
                    SyncMapVersion.Add(LanguageManager.GetValue("String.NoMaps"));
                    if (Properties.Settings.Default.CurrentSyncNav)
                    {
                        if (Properties.Settings.Default.CurrentSyncVersion >= SyncReformatVersion)
                            SyncMapVersion.Add(LanguageManager.GetValue("String.KeepExistingMaps"));
                    }
                    else
                    {
                        SyncMapVersion.Add(LanguageManager.GetValue("String.NonNavAPIM"));
                    }

                    _jsonMapReleases = JsonConvert.DeserializeObject<JsonReleases>(_stringMapReleasesJson);
                    foreach (Data item in _jsonMapReleases.data) SyncMapVersion.Add(item.name);
                }

                SyncMapVersionsEnabled = true;
                //OnPropertyChanged("SyncMapVersionsEnabled");
                StartEnabled = SelectedRelease != null && SelectedRegion != null && SelectedMapVersion != null;
                //OnPropertyChanged("StartEnabled");
            }
        }

        private void UpdateSelectedMapVersion()
        {
            if (SelectedMapVersion != "")
            {
                IvsuList.Clear();

                //LESS THAN 3.2
                if (Properties.Settings.Default.CurrentSyncVersion < SyncReformatVersion)
                {
                    InstallMode = "reformat";
                }

                //Above 3.2 and  Below 3.4.19274
                else if (Properties.Settings.Default.CurrentSyncVersion >= SyncReformatVersion && Properties.Settings.Default.CurrentSyncVersion < SyncBlacklistedVersion)
                {
                    //Update Nav?
                    if (SelectedMapVersion == LanguageManager.GetValue("String.NoMaps") || SelectedMapVersion == LanguageManager.GetValue("String.NonNavAPIM") ||
                        SelectedMapVersion == LanguageManager.GetValue("String.KeepExistingMaps"))
                        InstallMode = Properties.Settings.Default.CurrentInstallMode == "autodetect" ? "autoinstall" : Properties.Settings.Default.CurrentInstallMode;
                    else
                        InstallMode = Properties.Settings.Default.CurrentInstallMode == "autodetect" ? "reformat" : Properties.Settings.Default.CurrentInstallMode;
                }

                //3.4.19274 or above
                else if (Properties.Settings.Default.CurrentSyncVersion >= SyncBlacklistedVersion)
                {
                    //Update Nav?
                    if (SelectedMapVersion == LanguageManager.GetValue("String.NoMaps") || SelectedMapVersion == LanguageManager.GetValue("String.NonNavAPIM") ||
                        SelectedMapVersion == LanguageManager.GetValue("String.KeepExistingMaps"))
                        InstallMode = Properties.Settings.Default.CurrentInstallMode == "autodetect" ? "autoinstall" : Properties.Settings.Default.CurrentInstallMode;
                    else
                        InstallMode = Properties.Settings.Default.CurrentInstallMode == "autodetect" ? "downgrade" : Properties.Settings.Default.CurrentInstallMode;
                }

                //OnPropertyChanged("InstallMode");

                if (InstallMode == "downgrade")
                {
                    IvsuList.Add(new LocalIvsu
                    {
                        Type = "APPS",
                        Name = DowngradePackageAppName,
                        Version = "",
                        Notes = LanguageManager.GetValue("String.Required"),
                        Url = DowngradePackageAppUrl,
                        Md5 = DowngradePackageAppMd5,
                        Selected = true,
                        FileName = DowngradePackageAppFileName
                    });

                    IvsuList.Add(new LocalIvsu
                    {
                        Type = "TOOL",
                        Name = DowngradePackageToolName,
                        Version = "",
                        Notes = LanguageManager.GetValue("String.Required"),
                        Url = DowngradePackageToolUrl,
                        Md5 = DowngradePackageToolMd5,
                        Selected = true,
                        FileName = DowngradePackageToolFileName
                    });
                }

                if (InstallMode == "reformat" || InstallMode == "downgrade")
                    IvsuList.Add(new LocalIvsu
                    {
                        Type = "TOOL",
                        Name = SyncReformatToolName,
                        Version = "",
                        Notes = LanguageManager.GetValue("String.Required"),
                        Url = SyncReformatToolUrl,
                        Md5 = SyncReformatToolMd5,
                        Selected = true,
                        FileName = SyncReformatToolFileName
                    });

                ApplicationManager.Instance.InstallMode = InstallMode;

                HttpResponseMessage response = Client.GetAsync(ApiAppReleaseSingle + SelectedRelease).Result;
                _stringDownloadJson = response.Content.ReadAsStringAsync().Result;

                response = Client.GetAsync(ApiMapReleaseSingle + SelectedMapVersion).Result;
                _stringMapDownloadJson = response.Content.ReadAsStringAsync().Result;

                JsonReleases jsonIvsUs = JsonConvert.DeserializeObject<JsonReleases>(_stringDownloadJson);
                JsonReleases jsonMapIvsUs = JsonConvert.DeserializeObject<JsonReleases>(_stringMapDownloadJson);

                foreach (Ivsus item in jsonIvsUs.data[0].ivsus)
                    if (item.ivsu.regions.Contains("ALL") || item.ivsu.regions.Contains(SelectedRegion.Code))
                    {
                        string fileName = item.ivsu.url.Substring(item.ivsu.url.LastIndexOf("/", StringComparison.Ordinal) + 1,
                            item.ivsu.url.Length - item.ivsu.url.LastIndexOf("/", StringComparison.Ordinal) - 1);
                        IvsuList.Add(new LocalIvsu
                        {
                            Type = item.ivsu.type, Name = item.ivsu.name, Version = item.ivsu.version,
                            Notes = item.ivsu.notes, Url = item.ivsu.url, Md5 = item.ivsu.md5, Selected = true,
                            FileName = fileName
                        });
                    }

                if (SelectedMapVersion != LanguageManager.GetValue("String.NoMaps") && SelectedMapVersion != LanguageManager.GetValue("String.NonNavAPIM") &&
                    SelectedMapVersion != LanguageManager.GetValue("String.KeepExistingMaps"))
                    foreach (Ivsus item in jsonMapIvsUs.data[0].ivsus)
                        if (item.map_ivsu.regions.Contains("ALL") || item.map_ivsu.regions.Contains(SelectedRegion.Code))
                        {
                            string fileName = item.map_ivsu.url.Substring(item.map_ivsu.url.LastIndexOf("/", StringComparison.Ordinal) + 1,
                                item.map_ivsu.url.Length - item.map_ivsu.url.LastIndexOf("/", StringComparison.Ordinal) - 1);
                            IvsuList.Add(new LocalIvsu
                            {
                                Type = item.map_ivsu.type, Name = item.map_ivsu.name, Version = item.map_ivsu.version,
                                Notes = item.map_ivsu.notes, Url = item.map_ivsu.url, Md5 = item.map_ivsu.md5,
                                Selected = true, FileName = fileName
                            });
                        }

                //OnPropertyChanged("IvsuList");
                StartEnabled = SelectedRelease != null && SelectedRegion != null && SelectedMapVersion != null;
                //OnPropertyChanged("StartEnabled");
            }
        }

        private void StartAction()
        {
            _canceldownload = false;
            ApplicationManager.Instance.Ivsus.Clear();
            ApplicationManager.Instance.DownloadOnly = false;
            if (Debugger.IsAttached)
                ApplicationManager.Logger.Debug("[App] Debugger is attached redirecting URL's to 127.0.0.1");
            foreach (LocalIvsu item in IvsuList)
                if (item.Selected)
                {
                    if (item.Type == "APPS") _appsselected = true;

                    if (Debugger.IsAttached)
                    {
                        Uri myUri = new Uri(item.Url);
                        item.Url = item.Url.Replace(myUri.Host, "127.0.0.1").Replace(myUri.Scheme, "http"); // host is "www.contoso.com"
                    }

                    ApplicationManager.Instance.Ivsus.Add(item);
                }

            if (!CancelledDownload())
            {
                if (ApplicationManager.Instance.DownloadOnly == false)
                {
                    ApplicationManager.Instance.DriveNumber = SelectedDrive.Path.Replace("Win32_DiskDrive.DeviceID=\"\\\\\\\\.\\\\PHYSICALDRIVE", "").Replace("\"", "");
                    ApplicationManager.Instance.DriveLetter = DriveLetter;
                }

                ApplicationManager.Instance.SelectedRegion = SelectedRegion.Code;
                ApplicationManager.Instance.SelectedRelease = SelectedRelease;
                ApplicationManager.Instance.SelectedMapVersion = SelectedMapVersion;
                ApplicationManager.Instance.IsDownloading = true;
                ApplicationManager.Logger.Info($@"[App] Starting process ({SelectedRelease} - {SelectedRegion} - {SelectedMapVersion})");
                StartEnabled = false;
                //OnPropertyChanged("StartEnabled");
                ApplicationManager.Instance.FireDownloadsTabEvent();
            }
        }

        private bool CancelledDownload()
        {
            //No USB drive selected, download only?
            if ((SelectedDrive == null || SelectedDrive.Name == LanguageManager.GetValue("Home.NoUSB")) && _canceldownload == false)
            {
                if (MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelNoUSB"), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    ApplicationManager.Logger.Info("[App] No usb has been selected, download only mode activated");
                    ApplicationManager.Instance.DownloadOnly = true;
                }
                else
                {
                    _canceldownload = true;
                }
            }

            if ((InstallMode == "reformat" || InstallMode == "downgrade") && _canceldownload == false && ApplicationManager.Instance.DownloadOnly == false)
            {
                if (MessageBox.Show(string.Format(LanguageManager.GetValue("MessageBox.CancelMy20"), InstallMode), "Syn3 Updater", MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    if (MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelMy20Final"), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning) !=
                        MessageBoxResult.Yes)
                        _canceldownload = true;
                }
                else
                {
                    _canceldownload = true;
                }
            }

            //Check region is the same.
            if (SelectedRegion.Code != Properties.Settings.Default.CurrentSyncRegion && _canceldownload == false)
                if (MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelRegionMismatch"), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning) !=
                    MessageBoxResult.Yes)
                    _canceldownload = true;


            if (!string.IsNullOrEmpty(DriveLetter))
                if (DownloadLocation.Contains(DriveLetter) && _canceldownload == false)
                    MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelDownloadIsDrive"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            if (!string.IsNullOrEmpty(DriveLetter) && ApplicationManager.Instance.SkipFormat)
            {
                if (SelectedDrive != null && MessageBox.Show(string.Format(LanguageManager.GetValue("MessageBox.OptionalFormatUSB"), SelectedDrive.Name, DriveLetter),
                    "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                    ApplicationManager.Instance.SkipFormat = false;
                else
                    ApplicationManager.Logger.Info("[App] USB Drive not formatted, using existing filesystem and files");
            }

            if (_appsselected == false && _canceldownload == false && (InstallMode == "reformat" || InstallMode == "downgrade"))
            {
                MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelNoApps"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                _canceldownload = true;
            }

            // ReSharper disable once InvertIf
            if (ApplicationManager.Instance.DownloadOnly == false && _canceldownload == false && ApplicationManager.Instance.SkipFormat == false)
                if (SelectedDrive != null && MessageBox.Show(string.Format(LanguageManager.GetValue("MessageBox.CancelFormatUSB"), SelectedDrive.Name, DriveLetter), "Syn3 Updater",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    _canceldownload = true;
            return _canceldownload;
        }

        public class Drive
        {
            public string Path { get; set; }
            public string Name { get; set; }
        }

        public class SyncRegion
        {
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class LocalIvsu
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
    }
}