using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    internal class HomeViewModel : LanguageAwareBaseViewModel
    {
        #region Constructors

        private static readonly HttpClient Client = new HttpClient();

        private ActionCommand _startButton;
        private ActionCommand _refreshUSB;
        private ActionCommand _regionInfo;
        public ActionCommand RefreshUSB => _refreshUSB ?? (_refreshUSB = new ActionCommand(RefreshUsb));
        public ActionCommand RegionInfo => _regionInfo ?? (_regionInfo = new ActionCommand(RegionInfoAction));
        public ActionCommand StartButton => _startButton ?? (_startButton = new ActionCommand(StartAction));

        #endregion

        #region Properties & Fields

        private string _apiAppReleases, _apiMapReleases;
        private string _stringCompatibility, _stringReleasesJson, _stringMapReleasesJson, _stringDownloadJson, _stringMapDownloadJson;
        private Api.JsonReleases _jsonMapReleases, _jsonReleases;

        private string _driveLetter;

        public string DriveLetter
        {
            get => _driveLetter;
            set => SetProperty(ref _driveLetter, value);
        }

        private string _driveName;

        public string DriveName
        {
            get => _driveName;
            set => SetProperty(ref _driveName, value);
        }

        private string _driveFileSystem;

        public string DriveFileSystem
        {
            get => _driveFileSystem;
            set => SetProperty(ref _driveFileSystem, value);
        }

        private USBHelper.Drive _selectedDrive;

        public USBHelper.Drive SelectedDrive
        {
            get => _selectedDrive;
            set
            {
                SetProperty(ref _selectedDrive, value);
                UpdateDriveInfo();
            }
        }

        private SyncModel.SyncRegion _selectedRegion;

        public SyncModel.SyncRegion SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                SetProperty(ref _selectedRegion, value);
                if (value != null) UpdateSelectedRegion();
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

        private int _selectedMapVersionIndex;

        public int SelectedMapVersionIndex
        {
            get => _selectedMapVersionIndex;
            set => SetProperty(ref _selectedMapVersionIndex, value);
        }

        private int _selectedReleaseIndex;

        public int SelectedReleaseIndex
        {
            get => _selectedReleaseIndex;
            set => SetProperty(ref _selectedReleaseIndex, value);
        }

        private ObservableCollection<USBHelper.Drive> _driveList;

        public ObservableCollection<USBHelper.Drive> DriveList
        {
            get => _driveList;
            set => SetProperty(ref _driveList, value);
        }

        private ObservableCollection<SyncModel.SyncRegion> _syncRegions;

        public ObservableCollection<SyncModel.SyncRegion> SyncRegions
        {
            get => _syncRegions;
            set => SetProperty(ref _syncRegions, value);
        }

        private ObservableCollection<string> _syncVersion;

        public ObservableCollection<string> SyncVersion
        {
            get => _syncVersion;
            set => SetProperty(ref _syncVersion, value);
        }

        private ObservableCollection<string> _syncMapVersion;

        public ObservableCollection<string> SyncMapVersion
        {
            get => _syncMapVersion;
            set => SetProperty(ref _syncMapVersion, value);
        }

        private bool _syncVersionsEnabled;

        public bool SyncVersionsEnabled
        {
            get => _syncVersionsEnabled;
            set => SetProperty(ref _syncVersionsEnabled, value);
        }

        private bool _syncMapVersionsEnabled;

        public bool SyncMapVersionsEnabled
        {
            get => _syncMapVersionsEnabled;
            set => SetProperty(ref _syncMapVersionsEnabled, value);
        }

        private string _notes;

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        private ObservableCollection<SyncModel.SyncIvsu> _ivsuList;

        public ObservableCollection<SyncModel.SyncIvsu> IvsuList
        {
            get => _ivsuList;
            set => SetProperty(ref _ivsuList, value);
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
            set => SetProperty(ref _currentSyncVersion, value);
        }

        private string _currentSyncNav;

        public string CurrentSyncNav
        {
            get => _currentSyncNav;
            set => SetProperty(ref _currentSyncNav, value);
        }

        private string _downloadLocation;

        public string DownloadLocation
        {
            get => _downloadLocation;
            set
            {
                if (value != null) SetProperty(ref _downloadLocation, value);
            }
        }

        private string _installMode;

        public string InstallMode
        {
            get => _installMode;
            set => SetProperty(ref _installMode, value);
        }

        private bool _startEnabled;

        public bool StartEnabled
        {
            get => _startEnabled;
            set => SetProperty(ref _startEnabled, value);
        }

        private Visibility _driveDetailsVisible;
        public Visibility DriveDetailsVisible
        {
            get => _driveDetailsVisible;
            set => SetProperty(ref _driveDetailsVisible, value);
        }

        #endregion

        #region Methods

        public void ReloadSettings()
        {
            DriveDetailsVisible = SelectedDrive?.Name == LanguageManager.GetValue("Home.NoUSB") ? Visibility.Hidden : Visibility.Visible;
            CurrentSyncNav = Properties.Settings.Default.CurrentSyncNav ? "Yes" : "No";
            CurrentSyncRegion = Properties.Settings.Default.CurrentSyncRegion;
            CurrentSyncVersion = ApplicationManager.Instance.SyncVersion;
            DownloadLocation = ApplicationManager.Instance.DownloadPath;
            SelectedMapVersionIndex = -1;
            SelectedReleaseIndex = -1;
            StartEnabled = false;
            IvsuList = new ObservableCollection<SyncModel.SyncIvsu>();
            InstallMode = "";
        }

        public void Init()
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiSecret.Token);
            SyncRegions = new ObservableCollection<SyncModel.SyncRegion>
            {
                new SyncModel.SyncRegion {Code = "EU", Name = "Europe"},
                new SyncModel.SyncRegion {Code = "NA", Name = "North America & Canada"},
                new SyncModel.SyncRegion {Code = "CN", Name = "China"},
                new SyncModel.SyncRegion {Code = "ANZ", Name = "Australia & New Zealand"},
                new SyncModel.SyncRegion {Code = "ROW", Name = "Rest Of World"}
            };
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
            DriveList = USBHelper.refresh_devices(true);
        }

        private void UpdateDriveInfo()
        {
            StartEnabled = SelectedRelease != null && SelectedRegion != null && SelectedMapVersion != null && SelectedDrive != null;
            USBHelper.DriveInfo driveInfo = USBHelper.UpdateDriveInfo(SelectedDrive);

            // Update app level vars
            ApplicationManager.Instance.DriveFileSystem = driveInfo.FileSystem;
            ApplicationManager.Instance.DrivePartitionType = driveInfo.PartitionType;
            ApplicationManager.Instance.DriveName = driveInfo.Name;
            ApplicationManager.Instance.SkipFormat = driveInfo.SkipFormat;

            // Update local level vars
            DriveLetter = driveInfo.Letter;
            DriveFileSystem = driveInfo.PartitionType + " " + driveInfo.FileSystem;
            DriveName = driveInfo.Name;
            DriveDetailsVisible = driveInfo.Name == null ? Visibility.Hidden : Visibility.Visible;
            ApplicationManager.Logger.Info(
                $"[App] USB Drive selected - Name: {driveInfo.Name} - FileSystem: {driveInfo.FileSystem} - PartitionType: {driveInfo.PartitionType} - Letter: {driveInfo.Letter}");
        }

        private void UpdateSelectedRegion()
        {
            ApplicationManager.Logger.Info($"[Settings] Current Sync Details - Region: {CurrentSyncRegion} - Version: {CurrentSyncVersion} - Navigation: {CurrentSyncNav}");
            if (SelectedRegion.Code != "")
            {
                IvsuList.Clear();
                SelectedMapVersion = null;
                SelectedRelease = null;
                SyncMapVersion.Clear();
                if (Properties.Settings.Default.ShowAllReleases)
                {
                    _apiMapReleases = Api.MapReleasesConst.Replace("[published]", $"filter[key][in]=public,{Properties.Settings.Default.LicenseKey}");
                    _apiAppReleases = Api.AppReleasesConst.Replace("[published]", $"filter[key][in]=public,{Properties.Settings.Default.LicenseKey}");
                    //https://api.cyanlabs.net/fordsyncdownloader/items/map_releases?sort=-name&limit=-1&filter[regions]=ANZ&filter[compatibility][contains]=3.4&filter[status][in]=published,private&filter[key][in]=admin@cyanlabs.net,public
                }
                else
                {
                    _apiMapReleases = Api.MapReleasesConst.Replace("[published]",
                        $"filter[status][in]=published,private&filter[key][in]=public,{Properties.Settings.Default.LicenseKey}");
                    _apiAppReleases = Api.AppReleasesConst.Replace("[published]",
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
                    if (Properties.Settings.Default.CurrentSyncVersion >= Api.ReformatVersion)
                        SyncMapVersion.Add(LanguageManager.GetValue("String.KeepExistingMaps"));
                }


                _jsonReleases = JsonConvert.DeserializeObject<Api.JsonReleases>(_stringReleasesJson);
                SyncVersion = new ObservableCollection<string>();

                foreach (Api.Data item in _jsonReleases.data)
                    if (item.regions.Contains(SelectedRegion.Code))
                        SyncVersion.Add(item.name);

                SyncVersionsEnabled = true;
                StartEnabled = SelectedRelease != null && SelectedRegion != null && SelectedMapVersion != null && SelectedDrive != null;
            }
        }

        private void UpdateSelectedRelease()
        {
            if (SelectedRelease != "")
            {
                SelectedMapVersion = null;
                IvsuList.Clear();
                foreach (Api.Data item in _jsonReleases.data)
                    if (item.name == SelectedRelease)
                    {
                        _stringCompatibility = item.version.Substring(0, 3);
                        if (item.notes == null) continue;
                        Notes = item.notes.Replace("\n", Environment.NewLine);
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
                        if (Properties.Settings.Default.CurrentSyncVersion >= Api.ReformatVersion)
                            SyncMapVersion.Add(LanguageManager.GetValue("String.KeepExistingMaps"));
                    }
                    else
                    {
                        SyncMapVersion.Add(LanguageManager.GetValue("String.NonNavAPIM"));
                    }

                    _jsonMapReleases = JsonConvert.DeserializeObject<Api.JsonReleases>(_stringMapReleasesJson);
                    foreach (Api.Data item in _jsonMapReleases.data) SyncMapVersion.Add(item.name);
                }

                SyncMapVersionsEnabled = true;
                StartEnabled = SelectedRelease != null && SelectedRegion != null && SelectedMapVersion != null;
            }
        }

        private void UpdateSelectedMapVersion()
        {
            if (SelectedMapVersion != "")
            {
                IvsuList.Clear();

                //LESS THAN 3.2
                if (Properties.Settings.Default.CurrentSyncVersion < Api.ReformatVersion)
                {
                    InstallMode = "reformat";
                }

                //Above 3.2 and  Below 3.4.19274
                else if (Properties.Settings.Default.CurrentSyncVersion >= Api.ReformatVersion && Properties.Settings.Default.CurrentSyncVersion < Api.BlacklistedVersion)
                {
                    //Update Nav?
                    if (SelectedMapVersion == LanguageManager.GetValue("String.NoMaps") || SelectedMapVersion == LanguageManager.GetValue("String.NonNavAPIM") ||
                        SelectedMapVersion == LanguageManager.GetValue("String.KeepExistingMaps"))
                        InstallMode = Properties.Settings.Default.CurrentInstallMode == "autodetect" ? "autoinstall" : Properties.Settings.Default.CurrentInstallMode;
                    else
                        InstallMode = Properties.Settings.Default.CurrentInstallMode == "autodetect" ? "reformat" : Properties.Settings.Default.CurrentInstallMode;
                }

                //3.4.19274 or above
                else if (Properties.Settings.Default.CurrentSyncVersion >= Api.BlacklistedVersion)
                {
                    //Update Nav?
                    if (SelectedMapVersion == LanguageManager.GetValue("String.NoMaps") || SelectedMapVersion == LanguageManager.GetValue("String.NonNavAPIM") ||
                        SelectedMapVersion == LanguageManager.GetValue("String.KeepExistingMaps"))
                        InstallMode = Properties.Settings.Default.CurrentInstallMode == "autodetect" ? "autoinstall" : Properties.Settings.Default.CurrentInstallMode;
                    else
                        InstallMode = Properties.Settings.Default.CurrentInstallMode == "autodetect" ? "downgrade" : Properties.Settings.Default.CurrentInstallMode;
                }

                ApplicationManager.Instance.Action = "main";
                ApplicationManager.Instance.InstallMode = InstallMode;

                HttpResponseMessage response = Client.GetAsync(Api.AppReleaseSingle + SelectedRelease).Result;
                _stringDownloadJson = response.Content.ReadAsStringAsync().Result;

                response = Client.GetAsync(Api.MapReleaseSingle + SelectedMapVersion).Result;
                _stringMapDownloadJson = response.Content.ReadAsStringAsync().Result;

                Api.JsonReleases jsonIvsUs = JsonConvert.DeserializeObject<Api.JsonReleases>(_stringDownloadJson);
                Api.JsonReleases jsonMapIvsUs = JsonConvert.DeserializeObject<Api.JsonReleases>(_stringMapDownloadJson);

                foreach (Api.Ivsus item in jsonIvsUs.data[0].ivsus)
                    if (item.ivsu.regions.Contains("ALL") || item.ivsu.regions.Contains(SelectedRegion.Code))
                    {
                        string fileName = item.ivsu.url.Substring(item.ivsu.url.LastIndexOf("/", StringComparison.Ordinal) + 1,
                            item.ivsu.url.Length - item.ivsu.url.LastIndexOf("/", StringComparison.Ordinal) - 1);
                        IvsuList.Add(new SyncModel.SyncIvsu
                        {
                            Type = item.ivsu.type, Name = item.ivsu.name, Version = item.ivsu.version,
                            Notes = item.ivsu.notes, Url = item.ivsu.url, Md5 = item.ivsu.md5, Selected = true,
                            FileName = fileName
                        });
                    }

                if (SelectedMapVersion != LanguageManager.GetValue("String.NoMaps") && SelectedMapVersion != LanguageManager.GetValue("String.NonNavAPIM") &&
                    SelectedMapVersion != LanguageManager.GetValue("String.KeepExistingMaps"))
                    foreach (Api.Ivsus item in jsonMapIvsUs.data[0].ivsus)
                        if (item.map_ivsu.regions.Contains("ALL") || item.map_ivsu.regions.Contains(SelectedRegion.Code))
                        {
                            string fileName = item.map_ivsu.url.Substring(item.map_ivsu.url.LastIndexOf("/", StringComparison.Ordinal) + 1,
                                item.map_ivsu.url.Length - item.map_ivsu.url.LastIndexOf("/", StringComparison.Ordinal) - 1);
                            IvsuList.Add(new SyncModel.SyncIvsu
                            {
                                Type = item.map_ivsu.type, Name = item.map_ivsu.name, Version = item.map_ivsu.version,
                                Notes = item.map_ivsu.notes, Url = item.map_ivsu.url, Md5 = item.map_ivsu.md5,
                                Selected = true, FileName = fileName
                            });
                        }

                StartEnabled = SelectedRelease != null && SelectedRegion != null && SelectedMapVersion != null;
            }
        }

        private void StartAction()
        {
            ApplicationManager.Instance.Ivsus.Clear();

            if (InstallMode == "downgrade")
            {
                ApplicationManager.Instance.Ivsus.Add(Api.DowngradeApp);
                ApplicationManager.Instance.Ivsus.Add(Api.DowngradeTool);
            }

            if (InstallMode == "reformat" || InstallMode == "downgrade")
            {
                ApplicationManager.Instance.Ivsus.Add(Api.ReformatTool);
            }


            ApplicationManager.Instance.DownloadOnly = false;
            if (Debugger.IsAttached)
                ApplicationManager.Logger.Debug("[App] Debugger is attached redirecting URL's to 127.0.0.1");
            foreach (SyncModel.SyncIvsu item in IvsuList)
                if (item.Selected)
                {
                    if (item.Type == "APPS") ApplicationManager.Instance.AppsSelected = true;

                    if (Debugger.IsAttached)
                    {
                        Uri myUri = new Uri(item.Url);
                        item.Url = item.Url.Replace(myUri.Host, "127.0.0.1").Replace(myUri.Scheme, "http"); // host is "www.contoso.com"
                    }

                    ApplicationManager.Instance.Ivsus.Add(item);
                }

            bool canceldownload = false;
            //Install Mode is reformat or downgrade My20 warning
            if (InstallMode == "reformat" || InstallMode == "downgrade")
            {
                if (ModernWpf.MessageBox.Show(string.Format(LanguageManager.GetValue("MessageBox.CancelMy20"), InstallMode), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    if (ModernWpf.MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelMy20Final"), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                        canceldownload = true;
                }
                else
                {
                    canceldownload = true;
                }
            }

            //Warn is users region is different to new selection
            if (SelectedRegion.Code != Properties.Settings.Default.CurrentSyncRegion)
                if (ModernWpf.MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelRegionMismatch"), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    canceldownload = true;

            //Cancel no apps package selected
            if (ApplicationManager.Instance.AppsSelected == false && (InstallMode == "reformat" || InstallMode == "downgrade"))
            {
                ModernWpf.MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelNoApps"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                canceldownload = true;
            }

            if (!canceldownload && !SanityCheckHelper.CancelDownloadCheck(SelectedDrive))
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
                ApplicationManager.Instance.FireDownloadsTabEvent();
            }
        }

        #endregion
    }
}