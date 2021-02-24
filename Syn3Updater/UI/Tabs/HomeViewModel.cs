using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using Cyanlabs.Syn3Updater.Helper;
using Cyanlabs.Syn3Updater.Model;
using Newtonsoft.Json;

namespace Cyanlabs.Syn3Updater.UI.Tabs
{
    internal class HomeViewModel : LanguageAwareBaseViewModel
    {
        #region Constructors

        private ActionCommand _startButton;
        private ActionCommand _refreshUSB;
        private ActionCommand _regionInfo;
        public ActionCommand RefreshUSB => _refreshUSB ??= new ActionCommand(RefreshUsb);
        public ActionCommand RegionInfo => _regionInfo ??= new ActionCommand(RegionInfoAction);
        public ActionCommand StartButton => _startButton ??= new ActionCommand(StartAction);

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

        private int _selectedRegionIndex;

        public int SelectedRegionIndex
        {
            get => _selectedRegionIndex;
            set => SetProperty(ref _selectedRegionIndex, value);
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

        private Visibility _notesVisibility;

        public Visibility NotesVisibility
        {
            get => _notesVisibility;
            set => SetProperty(ref _notesVisibility, value);
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
                    ApplicationManager.Instance.Settings.CurrentSyncRegion = value;
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
            NotesVisibility = Visibility.Hidden;
            CurrentSyncNav = ApplicationManager.Instance.Settings.CurrentSyncNav ? "Yes" : "No";
            CurrentSyncRegion = ApplicationManager.Instance.Settings.CurrentSyncRegion;
            CurrentSyncVersion = ApplicationManager.Instance.SyncVersion;
            DownloadLocation = ApplicationManager.Instance.DownloadPath;
            SelectedMapVersionIndex = -1;
            SelectedReleaseIndex = -1;
            SelectedRegionIndex = -1;
            StartEnabled = false;
            IvsuList = new ObservableCollection<SyncModel.SyncIvsu>();
            InstallMode = "";
            RefreshUsb();
            SyncMapVersion = new ObservableCollection<string>();
            SyncVersion?.Clear();
            SyncMapVersion?.Clear();
            DriveDetailsVisible = SelectedDrive == null || SelectedDrive.Path == "" ? Visibility.Hidden : Visibility.Visible;
            ApplicationManager.Logger.Info($"Current Sync Details - Region: {CurrentSyncRegion} - Version: {CurrentSyncVersion} - Navigation: {CurrentSyncNav}");
        }

        public void Init()
        {
            SelectedRegionIndex = -1;
            ApplicationManager.Instance.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiSecret.Token);
            SyncRegions = new ObservableCollection<SyncModel.SyncRegion>
            {
                new SyncModel.SyncRegion {Code = "EU", Name = "Europe"},
                new SyncModel.SyncRegion {Code = "NA", Name = "North America & Canada"},
                new SyncModel.SyncRegion {Code = "CN", Name = "China"},
                new SyncModel.SyncRegion {Code = "ANZ", Name = "Australia, New Zealand, South America, Turkey & Taiwan"},
                new SyncModel.SyncRegion {Code = "ROW", Name = "Middle East, Africa, India, Sri Lanka, Israel, South East Asia, Caribbean & Central America"}
            };
            SyncVersionsEnabled = false;
        }

        private void RegionInfoAction()
        {
            Process.Start(Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "en"
                ? "https://cyanlabs.net/api/Syn3Updater/region.php"
                : $"https://translate.google.co.uk/translate?hl=&sl=en&tl={Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName}&u=https%3A%2F%2Fcyanlabs.net%2Fapi%2FSyn3Updater%2Fregion.php");
        }

        private void RefreshUsb()
        {
            try
            {
                ObservableCollection<USBHelper.Drive> tmpDriveList = USBHelper.RefreshDevices(true);
                if (tmpDriveList.Count > 0) DriveList = tmpDriveList;
            }
            catch (XamlParseException e)
            {
                ModernWpf.MessageBox.Show(e.GetFullMessage(), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                ApplicationManager.Logger.Info("ERROR: " + e.GetFullMessage());
            }
            catch (UnauthorizedAccessException e)
            {
                ModernWpf.MessageBox.Show(e.GetFullMessage(), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                ApplicationManager.Logger.Info("ERROR: " + e.GetFullMessage());
            }
        }

        private void UpdateDriveInfo()
        {
            StartEnabled = SelectedRelease != null && SelectedRegion != null && SelectedMapVersion != null && SelectedDrive != null;
            USBHelper.DriveInfo driveInfo = USBHelper.UpdateDriveInfo(SelectedDrive);

            // Update app level vars
            ApplicationManager.Instance.DriveFileSystem = driveInfo.FileSystem;
            ApplicationManager.Instance.DrivePartitionType = driveInfo.PartitionType;
            ApplicationManager.Instance.DriveName = SelectedDrive?.Name;
            ApplicationManager.Instance.SkipFormat = driveInfo.SkipFormat;

            // Update local level vars
            DriveLetter = driveInfo.Letter;
            DriveFileSystem = driveInfo.PartitionType + " " + driveInfo.FileSystem;
            DriveName = driveInfo.Name;
            DriveDetailsVisible = driveInfo.Name == null ? Visibility.Hidden : Visibility.Visible;
        }

        private void UpdateSelectedRegion()
        {
            NotesVisibility = Visibility.Hidden;
            if (SelectedRegion.Code != "")
            {
                IvsuList.Clear();
                SelectedMapVersion = null;
                SelectedRelease = null;
                SyncMapVersion.Clear();
                if (ApplicationManager.Instance.Settings.ShowAllReleases)
                {
                    _apiMapReleases = Api.MapReleasesConst.Replace("[published]", $"filter[key][_in]=public,v2,{ApplicationManager.Instance.Settings.LicenseKey}");
                    _apiAppReleases = Api.AppReleasesConst.Replace("[published]", $"filter[key][_in]=public,v2,{ApplicationManager.Instance.Settings.LicenseKey}");
                }
                else
                {
                    _apiMapReleases = Api.MapReleasesConst.Replace("[published]",
                        $"filter[status][_in]=published,private&filter[key][_in]=public,v2,{ApplicationManager.Instance.Settings.LicenseKey}");
                    _apiAppReleases = Api.AppReleasesConst.Replace("[published]",
                        $"filter[status][_in]=published,private&filter[key][_in]=public,v2,{ApplicationManager.Instance.Settings.LicenseKey}");
                }

                try
                {
                    HttpResponseMessage response = ApplicationManager.Instance.Client.GetAsync(_apiAppReleases).Result;
                    _stringReleasesJson = response.Content.ReadAsStringAsync().Result;
                }
                // ReSharper disable once RedundantCatchClause
                catch (WebException)
                {
                    throw;
                    //TODO Exception handling
                }

                if (!ApplicationManager.Instance.Settings.CurrentSyncNav)
                {
                    SyncMapVersion.Add(LanguageManager.GetValue("String.NonNavAPIM"));
                }
                else
                {
                    if (ApplicationManager.Instance.Settings.CurrentSyncVersion >= Api.ReformatVersion)
                        SyncMapVersion.Add(LanguageManager.GetValue("String.KeepExistingMaps"));
                }


                _jsonReleases = JsonConvert.DeserializeObject<Api.JsonReleases>(_stringReleasesJson);
                SyncVersion = new ObservableCollection<string>();

                foreach (Api.Data item in _jsonReleases.Releases)
                    if (item.Regions.Contains(SelectedRegion.Code))
                        SyncVersion.Add(item.Name);

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
                foreach (Api.Data item in _jsonReleases.Releases)
                    if (item.Name == SelectedRelease)
                    {
                        _stringCompatibility = item.Version.Substring(0, 3);
                        if (item.Notes == null)
                        {
                            NotesVisibility = Visibility.Hidden;
                            continue;
                        }

                        NotesVisibility = Visibility.Visible;
                        Notes = item.Notes.Replace("\n", Environment.NewLine);
                    }

                _apiMapReleases = _apiMapReleases.Replace("[regionplaceholder]", $"filter[regions]={SelectedRegion.Code}&filter[compatibility][_contains]={_stringCompatibility}");

                HttpResponseMessage response = ApplicationManager.Instance.Client.GetAsync(_apiMapReleases).Result;
                _stringMapReleasesJson = response.Content.ReadAsStringAsync().Result;

                if (ApplicationManager.Instance.Settings.CurrentSyncNav)
                {
                    SyncMapVersion.Clear();
                    SyncMapVersion.Add(LanguageManager.GetValue("String.NoMaps"));
                    if (ApplicationManager.Instance.Settings.CurrentSyncNav)
                    {
                        if (ApplicationManager.Instance.Settings.CurrentSyncVersion >= Api.ReformatVersion)
                            SyncMapVersion.Add(LanguageManager.GetValue("String.KeepExistingMaps"));
                    }
                    else
                    {
                        SyncMapVersion.Add(LanguageManager.GetValue("String.NonNavAPIM"));
                    }

                    _jsonMapReleases = JsonConvert.DeserializeObject<Api.JsonReleases>(_stringMapReleasesJson);
                    foreach (Api.Data item in _jsonMapReleases.Releases) SyncMapVersion.Add(item.Name);
                }

                SyncMapVersionsEnabled = true;
                StartEnabled = SelectedRelease != null && SelectedRegion != null && SelectedMapVersion != null && SelectedDrive != null;
            }
        }

        private void UpdateSelectedMapVersion()
        {
            if (SelectedMapVersion != "")
            {
                IvsuList.Clear();

                //LESS THAN 3.2
                if (ApplicationManager.Instance.Settings.CurrentSyncVersion < Api.ReformatVersion)
                {
                    InstallMode = ApplicationManager.Instance.Settings.CurrentInstallMode == "autodetect" ? "reformat" : ApplicationManager.Instance.Settings.CurrentInstallMode;
                }

                //Above 3.2 and  Below 3.4.19274
                else if (ApplicationManager.Instance.Settings.CurrentSyncVersion >= Api.ReformatVersion &&
                         ApplicationManager.Instance.Settings.CurrentSyncVersion < Api.BlacklistedVersion)
                {
                    //Update Nav?
                    if (SelectedMapVersion == LanguageManager.GetValue("String.NoMaps") || SelectedMapVersion == LanguageManager.GetValue("String.NonNavAPIM") ||
                        SelectedMapVersion == LanguageManager.GetValue("String.KeepExistingMaps"))
                        InstallMode = ApplicationManager.Instance.Settings.CurrentInstallMode == "autodetect"
                            ? "autoinstall"
                            : ApplicationManager.Instance.Settings.CurrentInstallMode;
                    else
                        InstallMode = ApplicationManager.Instance.Settings.CurrentInstallMode == "autodetect"
                            ? "reformat"
                            : ApplicationManager.Instance.Settings.CurrentInstallMode;
                }

                //3.4.19274 or above
                else if (ApplicationManager.Instance.Settings.CurrentSyncVersion >= Api.BlacklistedVersion)
                {
                    //Update Nav?
                    if (SelectedMapVersion == LanguageManager.GetValue("String.NoMaps") || SelectedMapVersion == LanguageManager.GetValue("String.NonNavAPIM") ||
                        SelectedMapVersion == LanguageManager.GetValue("String.KeepExistingMaps"))
                        InstallMode = ApplicationManager.Instance.Settings.CurrentInstallMode == "autodetect"
                            ? "autoinstall"
                            : ApplicationManager.Instance.Settings.CurrentInstallMode;
                    else
                        InstallMode = ApplicationManager.Instance.Settings.CurrentInstallMode == "autodetect"
                            ? "downgrade"
                            : ApplicationManager.Instance.Settings.CurrentInstallMode;
                }

                ApplicationManager.Instance.Action = "main";
                ApplicationManager.Instance.InstallMode = InstallMode;

                string appReleaseSingle = ApplicationManager.Instance.Settings.CurrentSyncNav
                    ? Api.AppReleaseSingle.Replace("[navplaceholder]", "nav") + SelectedRelease
                    : Api.AppReleaseSingle.Replace("[navplaceholder]", "nonnav") + SelectedRelease;

                HttpResponseMessage response = ApplicationManager.Instance.Client.GetAsync(appReleaseSingle).Result;
                _stringDownloadJson = response.Content.ReadAsStringAsync().Result;

                response = ApplicationManager.Instance.Client.GetAsync(Api.MapReleaseSingle + SelectedMapVersion).Result;
                _stringMapDownloadJson = response.Content.ReadAsStringAsync().Result;

                Api.JsonReleases jsonIvsUs = JsonConvert.DeserializeObject<Api.JsonReleases>(_stringDownloadJson);
                Api.JsonReleases jsonMapIvsUs = JsonConvert.DeserializeObject<Api.JsonReleases>(_stringMapDownloadJson);

                foreach (Api.Ivsus item in jsonIvsUs.Releases[0].IvsusList)
                    if (item.Ivsu.Regions.Contains("ALL") || item.Ivsu.Regions.Contains(SelectedRegion.Code))
                    {

                        string fileName = FileHelper.url_to_filename(item.Ivsu.Url);
                        IvsuList.Add(new SyncModel.SyncIvsu
                        {
                            Type = item.Ivsu.Type, Name = item.Ivsu.Name, Version = item.Ivsu.Version,
                            Notes = item.Ivsu.Notes, Url = item.Ivsu.Url, Md5 = item.Ivsu.Md5, Selected = true,
                            FileName = fileName
                        });
                    }

                if (SelectedMapVersion != LanguageManager.GetValue("String.NoMaps") && SelectedMapVersion != LanguageManager.GetValue("String.NonNavAPIM") &&
                    SelectedMapVersion != LanguageManager.GetValue("String.KeepExistingMaps"))
                    foreach (Api.Ivsus item in jsonMapIvsUs.Releases[0].IvsusList)
                        if (item.MapIvsu.Regions.Contains("ALL") || item.MapIvsu.Regions.Contains(SelectedRegion.Code))
                        {
                            string fileName = FileHelper.url_to_filename(item.MapIvsu.Url);
                            IvsuList.Add(new SyncModel.SyncIvsu
                            {
                                Type = item.MapIvsu.Type, Name = item.MapIvsu.Name, Version = item.MapIvsu.Version,
                                Notes = item.MapIvsu.Notes, Url = item.MapIvsu.Url, Md5 = item.MapIvsu.Md5,
                                Selected = true, FileName = fileName
                            });
                        }

                StartEnabled = SelectedRelease != null && SelectedRegion != null && SelectedMapVersion != null && SelectedDrive != null;
            }
        }

        private void StartAction()
        {
            ApplicationManager.Logger.Info(
                $"USB Drive selected - Name: {ApplicationManager.Instance.DriveName} - FileSystem: {ApplicationManager.Instance.DriveFileSystem} - PartitionType: {ApplicationManager.Instance.DrivePartitionType} - Letter: {DriveLetter}");
            ApplicationManager.Instance.Ivsus.Clear();

            if (InstallMode == "downgrade")
            {
                Api.DowngradeApp = ApiHelper.GetSpecialIvsu(Api.GetDowngradeApp);
                ApplicationManager.Instance.Ivsus.Add(Api.DowngradeApp);

                Api.DowngradeTool = ApiHelper.GetSpecialIvsu(Api.GetDowngradeTool);
                ApplicationManager.Instance.Ivsus.Add(Api.DowngradeTool);
            }

            if (InstallMode == "reformat" || InstallMode == "downgrade")
            {
                Api.ReformatTool = ApiHelper.GetSpecialIvsu(Api.GetReformat);
                ApplicationManager.Instance.Ivsus.Add(Api.ReformatTool);
            }

            ApplicationManager.Instance.DownloadOnly = false;
            ApplicationManager.Instance.DriveLetter = DriveLetter;
            foreach (SyncModel.SyncIvsu item in IvsuList)
                if (item.Selected)
                {
                    if (item.Type == "APPS") ApplicationManager.Instance.AppsSelected = true;

                    ApplicationManager.Instance.Ivsus.Add(item);
                }

            bool canceldownload = false;
            //Install Mode is reformat or downgrade My20 warning
            if (InstallMode == "reformat" || InstallMode == "downgrade")
            {
                if (ModernWpf.MessageBox.Show(string.Format(LanguageManager.GetValue("MessageBox.CancelMy20"), InstallMode), "Syn3 Updater", MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    if (ModernWpf.MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelMy20Final"), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning) !=
                        MessageBoxResult.Yes)
                        canceldownload = true;
                }
                else
                {
                    canceldownload = true;
                }
            }

            //Warn is users region is different to new selection
            if (SelectedRegion.Code != ApplicationManager.Instance.Settings.CurrentSyncRegion)
                if (ModernWpf.MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelRegionMismatch"), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning) !=
                    MessageBoxResult.Yes)
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
                ApplicationManager.Logger.Info($@"Starting process ({SelectedRelease} - {SelectedRegion.Code} - {SelectedMapVersion})");
                StartEnabled = false;
                ApplicationManager.Instance.FireDownloadsTabEvent();
            }
        }

        #endregion
    }
}