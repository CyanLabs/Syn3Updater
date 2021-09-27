using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DynamicData;
using GraphQL;
using JetBrains.Annotations;
using Nito.AsyncEx;
using ReactiveUI;
using Syn3Updater.Helpers;
using Syn3Updater.Models;

namespace Syn3Updater.ViewModels
{
    public class HomePageViewModel : ViewModelBase
    {
        private IvsuRoot? _syncVersions;

        public IvsuRoot? SyncVersions
        {
            get => _syncVersions;
            set => this.RaiseAndSetIfChanged(ref _syncVersions, value);
        }

        private string? _currentVersion;

        public string? CurrentVersion
        {
            get => _currentVersion;
            set
            {
                if (value == null || !value.Any(char.IsDigit)) return;
                AppMan.App.Settings.CurrentVersion = int.Parse(new string(value.Where(char.IsDigit).ToArray()));
                this.RaiseAndSetIfChanged(ref _currentVersion, value);
            }
        }

        private ObservableCollection<USBDriveModel.Drive>? _driveList;

        public ObservableCollection<USBDriveModel.Drive>? DriveList
        {
            get => _driveList;
            set => this.RaiseAndSetIfChanged(ref _driveList, value);
        }

        private USBDriveModel.Drive? _selectedDrive;

        public USBDriveModel.Drive? SelectedDrive
        {
            get => _selectedDrive;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedDrive, value);
                UpdateDriveInfo();
            }
        }

        private Interrogator.LogResult? _logResult;

        public Interrogator.LogResult? LogResult
        {
            get => _logResult;
            set => this.RaiseAndSetIfChanged(ref _logResult, value);
        }

        private bool _createInterrogatorEnabled;

        public bool CreateInterrogatorEnabled
        {
            get => _createInterrogatorEnabled;
            set => this.RaiseAndSetIfChanged(ref _createInterrogatorEnabled, value);
        }

        private bool _startEnabled;

        public bool StartEnabled
        {
            get => _startEnabled;
            set => this.RaiseAndSetIfChanged(ref _startEnabled, value);
        }

        private bool _interrogatorDescriptionVisible;

        public bool InterrogatorDescriptionVisible
        {
            get => _interrogatorDescriptionVisible;
            set => this.RaiseAndSetIfChanged(ref _interrogatorDescriptionVisible, value);
        }

        private bool _interrogatorOutputVisible;

        public bool InterrogatorOutputVisible
        {
            get => _interrogatorOutputVisible;
            set => this.RaiseAndSetIfChanged(ref _interrogatorOutputVisible, value);
        }

        private ObservableCollection<string> _regions;

        public ObservableCollection<string> Regions
        {
            get => _regions;
            set => this.RaiseAndSetIfChanged(ref _regions, value);
        }

        private ObservableCollection<string> _releases;

        public ObservableCollection<string> Releases
        {
            get => _releases;
            set => this.RaiseAndSetIfChanged(ref _releases, value);
        }  
        
        private ObservableCollection<string> _mapreleases;

        public ObservableCollection<string> MapReleases
        {
            get => _mapreleases;
            set
            {
                this.RaiseAndSetIfChanged(ref _mapreleases, value);
            }
        }
        
        private string _selectedRegion;

        public string SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedRegion, value);
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
                this.RaiseAndSetIfChanged(ref _selectedRelease, value);
                ResetMapReleaseInformation();
                SelectedMapRelease = AsyncContext.Run(async () => await GetMapReleaseInformation(SelectedRegion,_compat));
            }
        }
       
        private string _selectedMapRelease;

        public string SelectedMapRelease
        {
            get => _selectedMapRelease;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedMapRelease, value);
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
            StartEnabled = false;
            SyncVersions = AsyncContext.Run(async () => await ApiHelper.GetSyncVersions());

            if (OperatingSystem.IsWindows())
            {
                DriveList = USBHelper.RefreshDevicesWindows(true);
            }
            else if (OperatingSystem.IsMacOS())
            {
                DriveList = new ObservableCollection<USBDriveModel.Drive>
                {
                    new()
                    {
                        Encrypted = false, EncryptionStatus = "null", Fake = false, FileSystem = "MAC OSX LOL", FreeSpace = "999999PB WOW", Model = "CYANLABS AWESOMENESS",
                        Letter = "Z", Name = "CYANLABS ROCKS", PartitionType = "GPT", Path = "NoPath"
                    }
                };
            }

            AppMan.App.ShowInterrogatorLogCompleted += delegate { InterrogatorDescriptionVisible = true; };
        }

        private void UpdateDriveInfo()
        {
            CreateInterrogatorEnabled = SelectedDrive?.Name != null;
        }

        [UsedImplicitly]
        private async void PrepareInterrogatorUSB()
        {
            await USBHelper.LogPrepareUSBAction(SelectedDrive, SelectedDrive?.Letter, CurrentVersion);
        }

        [UsedImplicitly]
        private async void ScanInterrogatorUSB()
        {
            Releases = new ObservableCollection<string>();
            MapReleases = new ObservableCollection<string>();
            USBHelper usbHelper = new();
            LogResult = await usbHelper.LogParseXmlAction(SelectedDrive?.Letter);
            SelectedRegion = LogResult.Region;
            InterrogatorOutputVisible = true;
        }
        
        private async Task<string> GetReleaseInformation(string region)
        {
            if (region is "" or "???" or "NON-NAV") return string.Empty;
            Releases.Clear();
            GraphQLResponse<ReleasesRoot> graphQlResponse = await AppMan.App.GraphQlClient.SendQueryAsync<ReleasesRoot>(GraphQlHelper.GetReleases(region));
            ReleasesRoot latestRelease = graphQlResponse.Data;

            Releases.AddRange(latestRelease.Releases.Select(x => x.Name));
            _compat = latestRelease.Releases.FirstOrDefault().Version[..3];
            return latestRelease.Releases.FirstOrDefault().Name;
        }
        
        private async Task<string> GetMapReleaseInformation(string region, string compat)
        {
            if (region is "" or "???" or "NON-NAV") return string.Empty;
            MapReleases.Clear();
            GraphQLResponse<ReleasesRoot> graphQlResponse = await AppMan.App.GraphQlClient.SendQueryAsync<ReleasesRoot>(GraphQlHelper.GetMapReleases(region, compat));
            ReleasesRoot latestMapRelease = graphQlResponse.Data;
            
            MapReleases.AddRange(latestMapRelease.MapReleases.Select(x => x.Name));
            return latestMapRelease.MapReleases.FirstOrDefault().Name;
        }

        private void ResetMapReleaseInformation()
        {
            MapReleases = new ObservableCollection<string>();
            StartEnabled = false;
        }
        
        private async Task<ObservableCollection<SModel.Ivsu>> GetReleaseIVSUs()
        {
            ObservableCollection<SModel.Ivsu> ivsuList = new();
            string navtype = LogResult.Navigation ? "nav" : "nonnav";
            
            GraphQLResponse<ReleasesRoot> graphQlResponse = await AppMan.App.GraphQlClient.SendQueryAsync<ReleasesRoot>(GraphQlHelper.GetReleaseIvsus(SelectedRelease,navtype));
            ReleasesRoot jsonIvsUs = graphQlResponse.Data;
            foreach (ReleasesIvsus item in jsonIvsUs.Releases[0].IvsusList.Where(x => x.Ivsu != null && (x.Ivsu.Regions.Contains("ALL") || x.Ivsu.Regions.Contains(SelectedRegion))))
            {
                ivsuList.Add(new SModel.Ivsu
                {
                    Type = item.Ivsu.Type,
                    Name = item.Ivsu.Name,
                    Version = item.Ivsu.Version,
                    Notes = item.Ivsu.Notes,
                    Url = item.Ivsu.Url,
                    Md5 = item.Ivsu.Md5,
                    Selected = true,
                    FileName = FileHelper.url_to_filename(item.Ivsu.Url),
                    FileSize = item.Ivsu.FileSize
                });
            }
            
            GraphQLResponse<ReleasesRoot> graphQlResponse2 = await AppMan.App.GraphQlClient.SendQueryAsync<ReleasesRoot>(GraphQlHelper.GetMapReleaseIvsus(SelectedMapRelease));
            ReleasesRoot jsonMapIvsUs = graphQlResponse2.Data;
            foreach (ReleasesIvsus item in jsonMapIvsUs.MapReleases[0].IvsusList.Where(x => x.MapIvsu != null && (x.MapIvsu.Regions.Contains("ALL") || x.MapIvsu.Regions.Contains(SelectedRegion))))
            {
                ivsuList.Add(new SModel.Ivsu
                {
                    Type = item.MapIvsu.Type,
                    Name = item.MapIvsu.Name,
                    Version = item.MapIvsu.Version,
                    Notes = item.MapIvsu.Notes,
                    Url = item.MapIvsu.Url,
                    Md5 = item.MapIvsu.Md5,
                    Selected = true,
                    FileName = FileHelper.url_to_filename(item.MapIvsu.Url),
                    FileSize = item.MapIvsu.FileSize,
                    Source = item.MapIvsu.Source
                });
            }
            return ivsuList;
        }

        [UsedImplicitly]
        private async void Begin()
        {
            AppMan.App.Ivsus = await GetReleaseIVSUs();
            AppMan.App.SelectedRegion = SelectedRegion;
            AppMan.App.SelectedRelease = SelectedRelease;
            AppMan.App.SelectedMapVersion = SelectedMapRelease;
            AppMan.App.IsDownloading = true;
            AppMan.App.FireDownloadsStartEvent();
            AppMan.App.DriveLetter = SelectedDrive?.Letter;
        }
    }
}