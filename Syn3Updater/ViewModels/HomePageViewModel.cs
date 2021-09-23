using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Cyanlabs.Syn3Updater.Model;
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

        private Api.ReleaseDetails? _releaseDetails;

        public Api.ReleaseDetails? ReleaseDetails
        {
            get => _releaseDetails;
            set => this.RaiseAndSetIfChanged(ref _releaseDetails, value);
        }

        public HomePageViewModel()
        {
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
            USBHelper usbHelper = new();
            LogResult = await usbHelper.LogParseXmlAction(SelectedDrive?.Letter);
            InterrogatorOutputVisible = true;
            GetReleaseInformation();
        }

        private async void GetReleaseInformation()
        {
            if (LogResult?.Region is "" or "???" or "NON-NAV") return;

            GraphQLResponse<ReleasesRoot> graphQlResponse = await AppMan.App.GraphQlClient.SendQueryAsync<ReleasesRoot>(GraphQlHelper.GetLatestRelease(LogResult.Region));
            ReleasesRoot latestRelease = graphQlResponse.Data;

            string compat = latestRelease.Releases.FirstOrDefault().Version[..3];

            graphQlResponse = await AppMan.App.GraphQlClient.SendQueryAsync<ReleasesRoot>(GraphQlHelper.GetLatestMapRelease(LogResult.Region, compat));
            ReleasesRoot latestMapRelease = graphQlResponse.Data;

            ReleaseDetails = new Api.ReleaseDetails
            {
                Region = LogResult.Region,
                Release = latestRelease.Releases.FirstOrDefault().Name,
                Maps = latestMapRelease.MapReleases.FirstOrDefault().Name
            };
            GetReleaseIVSUs();
        }

        private ObservableCollection<SModel.Ivsu> _ivsuList;
        private async void GetReleaseIVSUs()
        {
            _ivsuList = new ObservableCollection<SModel.Ivsu>();
            string navtype = LogResult.Navigation ? "nav" : "nonnav";
            var graphQlResponse = await AppMan.App.GraphQlClient.SendQueryAsync<ReleasesRoot>(GraphQlHelper.GetReleaseIvsus(ReleaseDetails.Release,navtype));
            ReleasesRoot jsonIvsUs = graphQlResponse.Data;

            foreach (ReleasesIvsus item in jsonIvsUs.Releases[0].IvsusList.Where(ivsus => ivsus.Ivsu != null))
                if (item.Ivsu.Regions.Contains("ALL") || item.Ivsu.Regions.Contains(ReleaseDetails.Region))
                    _ivsuList?.Add(new SModel.Ivsu
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


            var graphQlResponse2 = await AppMan.App.GraphQlClient.SendQueryAsync<ReleasesRoot>(GraphQlHelper.GetMapReleaseIvsus(ReleaseDetails.Maps));
            ReleasesRoot jsonMapIvsUs = graphQlResponse2.Data;
            
            foreach (ReleasesIvsus item in jsonMapIvsUs.MapReleases[0].IvsusList.Where(ivsus => ivsus.MapIvsu != null))
                if (item.MapIvsu.Regions.Contains("ALL") || item.MapIvsu.Regions.Contains(ReleaseDetails.Region))
                    _ivsuList?.Add(new SModel.Ivsu
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
            StartEnabled = true;
        }

        [UsedImplicitly]
        private async void Begin()
        {
            AppMan.App.Ivsus = _ivsuList;
            AppMan.App.SelectedRegion = ReleaseDetails.Region;
            AppMan.App.SelectedRelease = ReleaseDetails.Release;
            AppMan.App.SelectedMapVersion = ReleaseDetails.Maps;
            AppMan.App.IsDownloading = true;
            AppMan.App.FireDownloadsStartEvent();
            AppMan.App.DriveLetter = SelectedDrive?.Letter;
        }
    }
}