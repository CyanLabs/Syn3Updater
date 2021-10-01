﻿using System;
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
using Syn3Updater.Services;

namespace Syn3Updater.ViewModels
{
    public class HomePageViewModel : ViewModelBase
    {
        private readonly ObservableCollection<string> _syncVersions;

        public ObservableCollection<string> SyncVersions
        {
            get => _syncVersions;
            private init => this.RaiseAndSetIfChanged(ref _syncVersions, value);
        }

        private string _currentVersion = string.Empty;

        public string CurrentVersion
        {
            get => _currentVersion;
            set
            {
                this.RaiseAndSetIfChanged(ref _currentVersion, value);
                if (value == string.Empty|| !value.Any(char.IsDigit) || value.Length < 9)
                {
                    CreateInterrogatorEnabled = false;
                    return;
                }
                AppMan.App.Settings.CurrentVersion = int.Parse(new string(value.Where(char.IsDigit).ToArray()));
                CreateInterrogatorEnabled = SelectedDrive?.Path != null;
            }
        }

        private ObservableCollection<USBDriveModel.Drive>? _driveList;

        public ObservableCollection<USBDriveModel.Drive>? DriveList
        {
            get => _driveList;
            set => this.RaiseAndSetIfChanged(ref _driveList, value);
        }

        private USBDriveModel.Drive _selectedDrive;

        public USBDriveModel.Drive SelectedDrive
        {
            get => _selectedDrive;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedDrive, value);
                UpdateDriveInfo();
            }
        }

        private Interrogator.LogResult _logResult;

        public Interrogator.LogResult LogResult
        {
            get => _logResult;
            private set => this.RaiseAndSetIfChanged(ref _logResult, value);
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
        
        private bool _regionUnknown;

        public bool RegionUnknown
        {
            get => _regionUnknown;
            set => this.RaiseAndSetIfChanged(ref _regionUnknown, value);
        }

        private ObservableCollection<string> _regions;

        public ObservableCollection<string> Regions
        {
            get => _regions;
            set => this.RaiseAndSetIfChanged(ref _regions, value);
        }

        private ObservableCollection<string> _releases = new();

        public ObservableCollection<string> Releases
        {
            get => _releases;
            set => this.RaiseAndSetIfChanged(ref _releases, value);
        }  
        
        private ObservableCollection<string> _mapReleases = new();

        public ObservableCollection<string> MapReleases
        {
            get => _mapReleases;
            set => this.RaiseAndSetIfChanged(ref _mapReleases, value);
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
                DriveList = USBHelper.RefreshDevicesWindows();
            }
            else if (OperatingSystem.IsMacOS())
            {
                DriveList = USBHelper.RefreshDevicesMac();
            }
            AppMan.App.ShowInterrogatorLogCompleted += delegate { InterrogatorDescriptionVisible = true; };
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
            if (OperatingSystem.IsWindows())
            {
                LogResult = await USBHelper.LogParseXmlAction(SelectedDrive.Path);
            }
            else if (OperatingSystem.IsMacOS())
            {
                LogResult = await USBHelper.LogParseXmlAction(SelectedDrive.VolumeName);
            }
            
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

            Releases.AddRange(latestRelease.Releases.Select(x => x.Name));
            _compat = latestRelease.Releases.FirstOrDefault()?.Version[..3] ?? "3.4";
            return latestRelease.Releases.FirstOrDefault()?.Name ?? throw new InvalidOperationException();
        }
        
        private async Task<string> GetMapReleaseInformation(string region, string compat)
        {
            if (region is "???" or null)
            {
                RegionUnknown = true;
                return string.Empty;
            }
            RegionUnknown = false;
            MapReleases.Clear();
            GraphQLResponse<ReleasesRoot> graphQlResponse = await AppMan.App.GraphQlClient.SendQueryAsync<ReleasesRoot>(GraphQlHelper.GetMapReleases(region, compat));
            ReleasesRoot latestMapRelease = graphQlResponse.Data;
            
            MapReleases.AddRange(latestMapRelease.MapReleases.Select(x => x.Name));
            MapReleases.Add("No Maps");
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
            AppMan.App.Ivsus = await HomeViewModelService.GetReleaseIvsus(SelectedRegion,SelectedRelease,SelectedMapRelease,LogResult.Navigation);
            AppMan.App.IsDownloading = true;
            AppMan.App.DrivePath = SelectedDrive.Path ?? throw new InvalidOperationException();
            AppMan.App.FireDownloadsStartEvent();
        }
    }
}