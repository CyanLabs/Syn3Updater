using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using AsyncAwaitBestPractices.MVVM;
using Cyanlabs.Syn3Updater.Helper;
using Cyanlabs.Syn3Updater.Model;
using Ookii.Dialogs.Wpf;

namespace Cyanlabs.Syn3Updater.UI.Tabs
{
    internal class UtilityViewModel : LanguageAwareBaseViewModel
    {
        #region Constructors

        private AsyncCommand _viewAsBuilt;
        public AsyncCommand ViewAsBuilt => _viewAsBuilt ??= new AsyncCommand(UploadFile);

        private ActionCommand _copyAsBuilt;
        public ActionCommand CopyAsBuilt => _copyAsBuilt ??= new ActionCommand(CopyAsBuiltAction);

        private ActionCommand _refreshUSB;
        public ActionCommand RefreshUSB => _refreshUSB ??= new ActionCommand(RefreshUsbAction);

        private AsyncCommand _logPrepareUSB;
        public AsyncCommand LogPrepareUSB => _logPrepareUSB ??= new AsyncCommand(LogPrepareUSBAction);

        private AsyncCommand _logParseXml;
        public AsyncCommand LogParseXml => _logParseXml ??= new AsyncCommand(LogParseXmlAction);

        private AsyncCommand _gracenotesRemovalUSB;
        public AsyncCommand GracenotesRemovalUSB => _gracenotesRemovalUSB ??= new AsyncCommand(GracenotesRemovalAction);

        private AsyncCommand _smallVoiceUSB;
        public AsyncCommand SmallVoiceUSB => _smallVoiceUSB ??= new AsyncCommand(SmallVoiceAction);

        private AsyncCommand _downgradeUSB;
        public AsyncCommand DowngradeUSB => _downgradeUSB ??= new AsyncCommand(DowngradeAction);

        private ActionCommand _troubleshootingDetails;
        public ActionCommand TroubleshootingDetails => _troubleshootingDetails ??= new ActionCommand(TroubleshootingDetailsAction);

        private ActionCommand _uploadLog;
        public ActionCommand UploadLog => _uploadLog ??= new ActionCommand(UploadLogAction);

        #endregion

        #region Properties & Fields

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

        private string _driveSize;

        public string DriveSize
        {
            get => _driveSize;
            set => SetProperty(ref _driveSize, value);
        }

        private string _driveFreeSpace;

        public string DriveFreeSpace
        {
            get => _driveFreeSpace;
            set => SetProperty(ref _driveFreeSpace, value);
        }

        private string _driveFileSystem;

        public string DriveFileSystem
        {
            get => _driveFileSystem;
            set => SetProperty(ref _driveFileSystem, value);
        }

        private string _logXmlDetails1;

        public string LogXmlDetails1
        {
            get => _logXmlDetails1;
            set => SetProperty(ref _logXmlDetails1, value);
        }

        private string _logXmlDetails2;

        public string LogXmlDetails2
        {
            get => _logXmlDetails2;
            set => SetProperty(ref _logXmlDetails2, value);
        }

        private string _logXmlDetails3;

        public string LogXmlDetails3
        {
            get => _logXmlDetails3;
            set => SetProperty(ref _logXmlDetails3, value);
        }

        private bool _toggleLogXmlDetails;

        public bool ToggleLogXmlDetails
        {
            get => _toggleLogXmlDetails;
            set => SetProperty(ref _toggleLogXmlDetails, value);
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

        private ObservableCollection<USBHelper.Drive> _driveList;

        public ObservableCollection<USBHelper.Drive> DriveList
        {
            get => _driveList;
            set => SetProperty(ref _driveList, value);
        }

        private bool _utiltyButtonStatus;

        public bool UtiltyButtonStatus
        {
            get => _utiltyButtonStatus;
            set => SetProperty(ref _utiltyButtonStatus, value);
        }

        #endregion

        #region Methods

        public void ReloadTab()
        {
            ToggleLogXmlDetails = false;
            UtiltyButtonStatus = !string.IsNullOrWhiteSpace(SelectedDrive?.Path);
        }

        public void Init()
        {
            RefreshUsbAction();
            _usbHelper = new USBHelper();
        }

        private async void RefreshUsbAction()
        {
            try
            {
                DriveList = USBHelper.RefreshDevices(false);
            }
            catch (Exception e)
            {
                await UIHelper.ShowErrorDialog(e.GetFullMessage());
                AppMan.Logger.Info("ERROR: " + e.GetFullMessage());
            }
        }

        private void UpdateDriveInfo()
        {
            if (SelectedDrive?.Encrypted != null && SelectedDrive.Encrypted)
            {
                UIHelper.ShowErrorDialog(LM.GetValue("MessageBox.EncryptedDrive"));
                RefreshUsbAction();
                return;
            }
            if (SelectedDrive?.Name != null)
            {
                // Update app level vars
                AppMan.App.DriveFileSystem = SelectedDrive?.FileSystem;
                AppMan.App.DrivePartitionType = SelectedDrive?.PartitionType;
                AppMan.App.DriveName = SelectedDrive?.Name;
                AppMan.App.SkipFormat = SelectedDrive.SkipFormat;

                // Update local level vars
                DriveFileSystem = SelectedDrive?.PartitionType + " " + SelectedDrive?.FileSystem;
                DriveName = SelectedDrive?.Name;
                DriveSize = SelectedDrive?.Size;
                DriveFreeSpace = SelectedDrive?.FreeSpace;
                ReloadTab();
                AppMan.Logger.Info(
                    $"USB Drive selected - Name: {SelectedDrive?.Name} - FileSystem: {SelectedDrive?.FileSystem} - PartitionType: {SelectedDrive?.PartitionType} - Letter: {SelectedDrive?.Letter}");
            }

            DriveLetter = SelectedDrive?.Letter;
        }

        private async Task LogPrepareUSBAction()
        {
            UtiltyButtonStatus = false;
            await USBHelper.LogPrepareUSBAction(SelectedDrive, DriveLetter);
            UtiltyButtonStatus = true;
        }

        private USBHelper _usbHelper;

        private async Task LogParseXmlAction()
        {
            string[] logDetails = await _usbHelper.LogParseXmlAction();
            if (logDetails.Length >= 3)
            {
                LogXmlDetails1 = logDetails[0];
                LogXmlDetails2 = logDetails[1];
                LogXmlDetails3 = logDetails[2];
                ToggleLogXmlDetails = true;
            }
        }

        private async Task GracenotesRemovalAction()
        {
            UtiltyButtonStatus = false;
            //Reset ApplicationManager variables
            AppMan.App.Ivsus.Clear();
            AppMan.App.DriveLetter = DriveLetter;
            AppMan.App.Action = "gracenotesremoval";
            AppMan.App.SelectedRelease = "Gracenotes Removal";
            //don't call ConfigureAwait(false) here either 
            try
            {
                Api.GracenotesRemoval = await ApiHelper.GetSpecialIvsu(Api.SpecialPackages.GraceNotesRemoval);
            }
            catch (TaskCanceledException e)
            {
                await UIHelper.ShowErrorDialog(e.GetFullMessage());
                return;
            }

            AppMan.App.Ivsus.Add(Api.GracenotesRemoval);
            AppMan.App.InstallMode = "autoinstall";

            if (await SanityCheckHelper.CancelDownloadCheck(SelectedDrive) || Api.GracenotesRemoval == null) return;

            //ApplicationManager.Instance.DriveNumber = SelectedDrive.Path.Replace("Win32_DiskDrive.DeviceID=\"\\\\\\\\.\\\\PHYSICALDRIVE", "").Replace("\"", "");
            AppMan.App.IsDownloading = true;
            AppMan.Logger.Info("Starting process (Gracenotes Removal");
            AppMan.App.FireDownloadsTabEvent();
            UtiltyButtonStatus = true;
        }

        private async Task SmallVoiceAction()
        {
            UtiltyButtonStatus = false;
            //Reset ApplicationManager variables
            AppMan.App.Ivsus.Clear();
            AppMan.App.DriveLetter = DriveLetter;
            AppMan.App.Action = "voiceshrinker";
            AppMan.App.SelectedRelease = "Voice Package Shrinker";

            Api.SmallVoicePackage = await ApiHelper.GetSpecialIvsu(Api.SpecialPackages.SmallVoice);
            try
            {
                AppMan.App.Ivsus.Add(Api.SmallVoicePackage);
            }
            catch (TaskCanceledException e)
            {
                await UIHelper.ShowErrorDialog(e.GetFullMessage());
                return;
            }

            AppMan.App.InstallMode = "autoinstall";

            if (await SanityCheckHelper.CancelDownloadCheck(SelectedDrive) || Api.SmallVoicePackage == null) return;

            //ApplicationManager.Instance.DriveNumber = SelectedDrive.Path.Replace("Win32_DiskDrive.DeviceID=\"\\\\\\\\.\\\\PHYSICALDRIVE", "").Replace("\"", "");
            AppMan.App.IsDownloading = true;
            AppMan.Logger.Info("Starting process (Voice Package Shrinker");
            AppMan.App.FireDownloadsTabEvent();
            UtiltyButtonStatus = true;
        }

        private async Task DowngradeAction()
        {
            UtiltyButtonStatus = false;
            //Reset ApplicationManager variables
            AppMan.App.Ivsus.Clear();
            AppMan.App.DriveLetter = DriveLetter;
            AppMan.App.Action = "downgrade";
            AppMan.App.SelectedRelease = "Enforced Downgrade";
            Api.DowngradeAppIvsu = await ApiHelper.GetSpecialIvsu(Api.SpecialPackages.DowngradeApp);
            try
            {
                AppMan.App.Ivsus.Add(Api.DowngradeAppIvsu);
            }
            catch (TaskCanceledException e)
            {
                await UIHelper.ShowErrorDialog(e.GetFullMessage());
                return;
            }

            AppMan.App.InstallMode = "autoinstall";
            if (await SanityCheckHelper.CancelDownloadCheck(SelectedDrive) || Api.DowngradeAppIvsu == null) return;

            //ApplicationManager.Instance.DriveNumber = SelectedDrive.Path.Replace("Win32_DiskDrive.DeviceID=\"\\\\\\\\.\\\\PHYSICALDRIVE", "").Replace("\"", "");
            AppMan.App.IsDownloading = true;
            AppMan.Logger.Info("Starting process (Enforced Downgrade");
            AppMan.App.FireDownloadsTabEvent();
            UtiltyButtonStatus = true;
        }

        private static void TroubleshootingDetailsAction()
        {
            SystemHelper.OpenWebPage("https://community.cyanlabs.net/t/tutorial-sync-3-4-non-nav-apim-failure-to-update-to-newer-version-sync-3-4/1984");
        }

        private void UploadLogAction()
        {
            if (!string.IsNullOrEmpty(DriveLetter) && File.Exists(DriveLetter + @"\log.txt"))
            {
                string logfile = File.ReadAllText(DriveLetter + @"\log.txt");
                USBHelper.UploadLog(logfile);
            }
            else
            {
                VistaFileDialog dialog = new VistaOpenFileDialog { Filter = "Syn3 Updater Log Files|*.txt" };
                if (dialog.ShowDialog().GetValueOrDefault())
                {
                    string logfile = File.ReadAllText(dialog.FileName);
                    USBHelper.UploadLog(logfile);
                }
            }
        }

        private async Task UploadFile()
        {
            await _usbHelper.UploadFile();
        }

        private void CopyAsBuiltAction()
        {
            Clipboard.SetText($"{LogXmlDetails1}{Environment.NewLine}{Environment.NewLine}{LogXmlDetails2}{Environment.NewLine}{LogXmlDetails3}");
        }
        #endregion
    }
}