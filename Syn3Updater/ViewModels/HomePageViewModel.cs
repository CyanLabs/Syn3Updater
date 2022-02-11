using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using JetBrains.Annotations;
using Nito.AsyncEx;
using Syn3Updater.Helpers;
using Syn3Updater.Models;
using Syn3Updater.Services;

namespace Syn3Updater.ViewModels
{
    public class HomePageViewModel : BaseViewModel
    {
        public readonly GraphQLHttpClient GraphQlClient = new(Api.Syn3UpdaterGraphQl, new NewtonsoftJsonSerializer())
        {
            HttpClient = { DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", ApiSecret.Token) } }
        };

        private readonly ObservableCollection<string> _syncVersions;

        public ObservableCollection<string> SyncVersions
        {
            get => _syncVersions;
            private init => SetProperty(ref _syncVersions, value);
        }

        private string _currentVersion = string.Empty;

        public string CurrentVersion
        {
            get => _currentVersion;
            set
            {
                SetProperty(ref _currentVersion, value);
                if (value == string.Empty || !value.Any(char.IsDigit) || value.Length < 9)
                {
                    CreateInterrogatorEnabled = false;
                    return;
                }
                //AppMan.App.Settings.CurrentVersion = int.Parse(new string(value.Where(char.IsDigit).ToArray()));
                CreateInterrogatorEnabled = SelectedDrive?.Path != null;
            }
        }

        private string _installMode = string.Empty;

        public string InstallMode
        {
            get => _installMode;
            set => SetProperty(ref _installMode, value);
        }

        private string? _installModeForced;

        public string? InstallModeForced
        {
            get => _installModeForced;
            set => SetProperty(ref _installModeForced, value);
        }

        private string _my20Mode;

        public string? My20Mode
        {
            get => _my20Mode;
            set => SetProperty(ref _my20Mode, value);
        }

        private ObservableCollection<USBDriveModel.Drive>? _driveList;

        public ObservableCollection<USBDriveModel.Drive>? DriveList
        {
            get => _driveList;
            set => SetProperty(ref _driveList, value);
        }

        private USBDriveModel.Drive _selectedDrive;

        public USBDriveModel.Drive SelectedDrive
        {
            get => _selectedDrive;
            set
            {
                SetProperty(ref _selectedDrive, value);
                UpdateDriveInfo();
            }
        }

        private Interrogator.LogResult _logResult;

        public Interrogator.LogResult LogResult
        {
            get => _logResult;
            private set => SetProperty(ref _logResult, value);
        }

        private bool _createInterrogatorEnabled;

        public bool CreateInterrogatorEnabled
        {
            get => _createInterrogatorEnabled;
            set => SetProperty(ref _createInterrogatorEnabled, value);
        }

        private bool _startEnabled;

        public bool StartEnabled
        {
            get => _startEnabled;
            set => SetProperty(ref _startEnabled, value);
        }

        private bool _interrogatorDescriptionVisible;

        public bool InterrogatorDescriptionVisible
        {
            get => _interrogatorDescriptionVisible;
            set => SetProperty(ref _interrogatorDescriptionVisible, value);
        }

        private bool _interrogatorOutputVisible;

        public bool InterrogatorOutputVisible
        {
            get => _interrogatorOutputVisible;
            set => SetProperty(ref _interrogatorOutputVisible, value);
        }

        private bool _regionUnknown;

        public bool RegionUnknown
        {
            get => _regionUnknown;
            set => SetProperty(ref _regionUnknown, value);
        }

        private ObservableCollection<string> _regions;

        public ObservableCollection<string> Regions
        {
            get => _regions;
            set => SetProperty(ref _regions, value);
        }

        private ObservableCollection<string> _releases = new();

        public ObservableCollection<string> Releases
        {
            get => _releases;
            set => SetProperty(ref _releases, value);
        }

        private ObservableCollection<string> _mapReleases = new();

        public ObservableCollection<string> MapReleases
        {
            get => _mapReleases;
            set => SetProperty(ref _mapReleases, value);
        }

        private string _selectedRegion;

        public string SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                SetProperty(ref _selectedRegion, value);
                ResetMapReleaseInformation();
                SelectedRelease = AsyncContext.Run(async () => await GetReleaseInformation(SelectedRegion));
            }
        }

        private string _selectedRelease;

        public string SelectedRelease
        {
            get => _selectedRelease;
            set
            {
                SetProperty(ref _selectedRelease, value);
                ResetMapReleaseInformation();
                SelectedMapRelease = AsyncContext.Run(async () => await GetMapReleaseInformation(SelectedRegion, _compat));
            }
        }

        private string _selectedMapRelease;

        public string SelectedMapRelease
        {
            get => _selectedMapRelease;
            set
            {
                SetProperty(ref _selectedMapRelease, value);
                UpdateInstallMode();
                StartEnabled = SelectedMapRelease != string.Empty;
            }
        }

        private string _compat = "";

        public HomePageViewModel()
        {
            Regions = new ObservableCollection<string> { "CN", "EU", "NA", "ANZ", "ROW" };
            CreateInterrogatorEnabled = false;
            InterrogatorDescriptionVisible = false;
            InterrogatorOutputVisible = false;
            //InstallModeForced = AppMan.App.ModeForced ? "Yes" : "No";
            StartEnabled = false;
            SyncVersions = AsyncContext.Run(async () => await ApiHelper.GetSyncVersions());

            if (OperatingSystem.IsWindows())
            {
                DriveList = USBHelper.RefreshDevicesWindows();
            }
            else if (OperatingSystem.IsMacOS())
            {
                DriveList = USBHelper.RefreshDevicesMac();
            }
            //AppMan.App.ShowInterrogatorLogCompleted += delegate { InterrogatorDescriptionVisible = true; };
        }

        private void UpdateDriveInfo()
        {
            CreateInterrogatorEnabled = CurrentVersion != string.Empty && SelectedDrive.Path != null;
        }

        [UsedImplicitly]
        private async void PrepareInterrogatorUSB()
        {
            await USBHelper.LogPrepareUSBAction(SelectedDrive, SelectedDrive.Path!, CurrentVersion!);
        }

        [UsedImplicitly]
        private async void ScanInterrogatorUSB()
        {
            USBHelper usbHelper = new();
            if (OperatingSystem.IsWindows())
            {
                LogResult = await usbHelper.LogParseXmlAction(SelectedDrive.Path);
            }
            else if (OperatingSystem.IsMacOS())
            {
                LogResult = await usbHelper.LogParseXmlAction(SelectedDrive.VolumeName);
            }
            //My20Mode = AppMan.App.Settings.My20 switch
            //{
            //    null => "AutoDetect",
            //    true => "Enabled",
            //    false => "Disabled"
            //};

            SelectedRegion = LogResult.Region;
            InterrogatorOutputVisible = true;
        }

        private async Task<string> GetReleaseInformation(string region)
        {
            if (region is "???" or null)
            {
                RegionUnknown = true;
                return string.Empty;
            }
            RegionUnknown = false;
            Releases.Clear();
            GraphQLResponse<ReleasesRoot> graphQlResponse = await AppMan.App.GraphQlClient.SendQueryAsync<ReleasesRoot>(GraphQlHelper.GetReleases(region));
            ReleasesRoot latestRelease = graphQlResponse.Data;

            //Releases.AddRange(latestRelease.Releases.Select(x => x.Name));
            if (AppMan.App.Settings.CurrentVersion >= Api.ReformatVersion) Releases.Add("Only Update Maps");
            _compat = latestRelease.Releases.FirstOrDefault()?.Version[..3] ?? "3.4";
            return latestRelease.Releases.FirstOrDefault()?.Name ?? throw new InvalidOperationException();
        }

        private async Task<string> GetMapReleaseInformation(string region, string compat)
        {
            ReleasesRoot latestMapRelease = new();
            if (LogResult.Navigation)
            {
                if (region is "???" or null)
                {
                    RegionUnknown = true;
                    return string.Empty;
                }
                RegionUnknown = false;
                MapReleases.Clear();
                GraphQLResponse<ReleasesRoot> graphQlResponse = await AppMan.App.GraphQlClient.SendQueryAsync<ReleasesRoot>(GraphQlHelper.GetMapReleases(region, compat));
                latestMapRelease = graphQlResponse.Data;
                //MapReleases.AddRange(latestMapRelease.MapReleases.Select(x => x.Name));
            }

            if (AppMan.App.Settings.CurrentVersion >= Api.ReformatVersion && SelectedRelease != "Only Update Maps") MapReleases.Add("No Maps");
            return LogResult.Navigation == false ? "No Maps" : latestMapRelease.MapReleases.FirstOrDefault()?.Name ?? throw new InvalidOperationException();
        }

        private void ResetMapReleaseInformation()
        {
            MapReleases = new ObservableCollection<string>();
            StartEnabled = false;
        }

        [UsedImplicitly]
        private async void Begin()
        {
            AppMan.App.Ivsus = await HomeViewModelService.GetReleaseIvsus(SelectedRegion, SelectedRelease, SelectedMapRelease, LogResult.Navigation);
            AppMan.App.IsDownloading = true;
            AppMan.App.DrivePath = SelectedDrive.Path ?? throw new InvalidOperationException();
            AppMan.App.FireDownloadsStartEvent();
        }

        private void UpdateInstallMode()
        {
            if (string.IsNullOrWhiteSpace(SelectedMapRelease) || string.IsNullOrWhiteSpace(SelectedRelease)) return;

            //LESS THAN 3.2
            if (AppMan.App.Settings.CurrentVersion < Api.ReformatVersion)
            {
                if (AppMan.App.Settings.My20 == true) InstallMode = "autoinstall";
                if (!AppMan.App.ModeForced)
                    InstallMode = "reformat";
            }

            //Above 3.2 and  Below 3.4.19274
            else if (AppMan.App.Settings.CurrentVersion >= Api.ReformatVersion &&
                     AppMan.App.Settings.CurrentVersion < Api.BlacklistedVersion)
            {
                //Update Nav?
                if (SelectedMapRelease == "No Maps")
                {
                    if (!AppMan.App.ModeForced) InstallMode = "autoinstall";
                }
                else if (AppMan.App.Settings.My20 == true)
                {
                    InstallMode = "autoinstall";
                }
                else if (!AppMan.App.ModeForced)
                {
                    InstallMode = "reformat";
                }
            }

            //3.4.19274 or above
            else if (AppMan.App.Settings.CurrentVersion >= Api.BlacklistedVersion)
            {
                //Update Nav?
                if (SelectedMapRelease == "No Maps" || SelectedRelease == "Only Update Maps")
                {
                    if (!AppMan.App.ModeForced)
                        InstallMode = "autoinstall";
                }
                else if (AppMan.App.Settings.My20 == true)
                {
                    InstallMode = "autoinstall";
                }
                else if (!AppMan.App.ModeForced)
                {
                    InstallMode = "downgrade";
                }
            }

            AppMan.App.Action = "main";
            AppMan.App.InstallMode = InstallMode;
        }
    }
}