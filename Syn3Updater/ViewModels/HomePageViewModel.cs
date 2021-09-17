using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Nito.AsyncEx;
using ReactiveUI;
using Syn3Updater.Helpers;
using Syn3Updater.Models;

namespace Syn3Updater.ViewModels
{
        public class HomePageViewModel : ViewModelBase
        {
            private IvsuRoot _syncVersions;

            public IvsuRoot SyncVersions
            {
                get => _syncVersions;
                set => this.RaiseAndSetIfChanged(ref _syncVersions, value);
            }

            private string _currentVersion;

            public string CurrentVersion
            {
                get => _currentVersion;
                set => this.RaiseAndSetIfChanged(ref _currentVersion, value);
            }

            private ObservableCollection<USBDriveModel.Drive> _driveList;

            public ObservableCollection<USBDriveModel.Drive> DriveList
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
            
            private bool _createInterrogatorEnabled;
            public bool CreateInterrogatorEnabled
            {
                get => _createInterrogatorEnabled;
                set => this.RaiseAndSetIfChanged(ref _createInterrogatorEnabled, value);
            }

            private bool _scanInterrogatorEnabled;
            public bool ScanInterrogatorEnabled
            {
                get => _scanInterrogatorEnabled;
                set => this.RaiseAndSetIfChanged(ref _scanInterrogatorEnabled, value);
            }

            private bool _startEnabled;
            public bool StartEnabled
            {
                get => _startEnabled;
                set => this.RaiseAndSetIfChanged(ref _startEnabled, value);
            }
            
            public HomePageViewModel()
            {
                CreateInterrogatorEnabled = false;
                ScanInterrogatorEnabled = false;
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
            }
            
            private void UpdateDriveInfo()
            {
                CreateInterrogatorEnabled = SelectedDrive.Name != null;
            }

            private async void PrepareInterrogatorUSB()
            {
                await USBHelper.LogPrepareUSBAction(SelectedDrive, SelectedDrive?.Letter,CurrentVersion);
            }
        }
    }