using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Linq;
using AsyncAwaitBestPractices.MVVM;
using Cyanlabs.Syn3Updater.Helper;
using Cyanlabs.Syn3Updater.Model;
using Cyanlabs.Updater.Common;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using Formatting = Newtonsoft.Json.Formatting;

namespace Cyanlabs.Syn3Updater.UI.Tabs
{
    internal class UtilityViewModel : LanguageAwareBaseViewModel
    {
        #region Constructors

        private AsyncCommand _viewAsBuilt;
        public AsyncCommand ViewAsBuilt => _viewAsBuilt ??= new AsyncCommand(UploadFile);

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

        private static readonly HttpClient Client = new HttpClient();

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

        private string _driveFileSystem;

        public string DriveFileSystem
        {
            get => _driveFileSystem;
            set => SetProperty(ref _driveFileSystem, value);
        }

        private string _logXmlDetails;

        public string LogXmlDetails
        {
            get => _logXmlDetails;
            set => SetProperty(ref _logXmlDetails, value);
        }

        private Visibility _toggleLogXmlDetails;

        public Visibility ToggleLogXmlDetails
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
            ToggleLogXmlDetails = Visibility.Hidden;
            UtiltyButtonStatus = !string.IsNullOrWhiteSpace(SelectedDrive?.Path);
        }

        public void Init()
        {
            RefreshUsbAction();
            _usbHelper = new USBHelper();
        }

        private void RefreshUsbAction()
        {
            try
            {
                ObservableCollection<USBHelper.Drive> tmpDriveList = USBHelper.RefreshDevices(false);
                if (tmpDriveList.Count > 0) DriveList = tmpDriveList;
            }
            catch (XamlParseException e)
            {
                ModernWpf.MessageBox.Show(e.GetFullMessage(), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                AppMan.Logger.Info("ERROR: " + e.GetFullMessage());
            }
            catch (UnauthorizedAccessException e)
            {
                ModernWpf.MessageBox.Show(e.GetFullMessage(), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                AppMan.Logger.Info("ERROR: " + e.GetFullMessage());
            }
        }

        private void UpdateDriveInfo()
        {
            USBHelper.DriveInfo driveInfo = USBHelper.UpdateDriveInfo(SelectedDrive);

            // Update app level vars
            AppMan.App.DriveFileSystem = driveInfo.FileSystem;
            AppMan.App.DrivePartitionType = driveInfo.PartitionType;
            AppMan.App.DriveName = SelectedDrive?.Name;
            AppMan.App.SkipFormat = driveInfo.SkipFormat;

            // Update local level vars
            DriveLetter = driveInfo.Letter;
            DriveFileSystem = driveInfo.PartitionType + " " + driveInfo.FileSystem;
            DriveName = driveInfo.Name;

            ReloadTab();
            if (SelectedDrive?.Path != "")
                AppMan.Logger.Info(
                    $"USB Drive selected - Name: {driveInfo.Name} - FileSystem: {driveInfo.FileSystem} - PartitionType: {driveInfo.PartitionType} - Letter: {driveInfo.Letter}");
        }

        private async Task LogPrepareUSBAction()
        {
            await USBHelper.LogPrepareUSBAction(SelectedDrive, DriveLetter);
        }

        private XDocument _node;


        private USBHelper _usbHelper;
        private async Task LogParseXmlAction()
        {
            LogXmlDetails = await _usbHelper.LogParseXmlAction();
            ToggleLogXmlDetails = Visibility.Visible;
        }

        private async Task GracenotesRemovalAction()
        {
            //Reset ApplicationManager variables
            AppMan.App.Ivsus.Clear();
            AppMan.App.DownloadOnly = false;
            AppMan.App.DriveLetter = DriveLetter;
            AppMan.App.Action = "gracenotesremoval";
            AppMan.App.SelectedRelease = "Gracenotes Removal";
            //don't call ConfigureAwait(false) here either 
            Api.GracenotesRemoval = await ApiHelper.GetSpecialIvsu(Api.GetGracenotesRemoval);
            AppMan.App.Ivsus.Add(Api.GracenotesRemoval);
            AppMan.App.InstallMode = "autoinstall";

            if (SanityCheckHelper.CancelDownloadCheck(SelectedDrive, false)) return;

            //ApplicationManager.Instance.DriveNumber = SelectedDrive.Path.Replace("Win32_DiskDrive.DeviceID=\"\\\\\\\\.\\\\PHYSICALDRIVE", "").Replace("\"", "");
            AppMan.App.IsDownloading = true;
            AppMan.Logger.Info("Starting process (Gracenotes Removal");
            AppMan.App.FireDownloadsTabEvent();
        }

        private async Task SmallVoiceAction()
        {
            //Reset ApplicationManager variables
            AppMan.App.Ivsus.Clear();
            AppMan.App.DownloadOnly = false;
            AppMan.App.DriveLetter = DriveLetter;
            AppMan.App.Action = "voiceshrinker";
            AppMan.App.SelectedRelease = "Voice Package Shrinker";

            Api.SmallVoicePackage = await ApiHelper.GetSpecialIvsu(Api.GetSmallVoice);
            AppMan.App.Ivsus.Add(Api.SmallVoicePackage);
            AppMan.App.InstallMode = "autoinstall";

            if (SanityCheckHelper.CancelDownloadCheck(SelectedDrive, false)) return;

            //ApplicationManager.Instance.DriveNumber = SelectedDrive.Path.Replace("Win32_DiskDrive.DeviceID=\"\\\\\\\\.\\\\PHYSICALDRIVE", "").Replace("\"", "");
            AppMan.App.IsDownloading = true;
            AppMan.Logger.Info("Starting process (Voice Package Shrinker");
            AppMan.App.FireDownloadsTabEvent();
        }

        private async Task DowngradeAction()
        {
            //Reset ApplicationManager variables
            AppMan.App.Ivsus.Clear();
            AppMan.App.DownloadOnly = false;
            AppMan.App.DriveLetter = DriveLetter;
            AppMan.App.Action = "downgrade";
            AppMan.App.SelectedRelease = "Enforced Downgrade";
            Api.DowngradeApp = await ApiHelper.GetSpecialIvsu(Api.GetDowngradeApp);
            AppMan.App.Ivsus.Add(Api.DowngradeApp);
            AppMan.App.InstallMode = "autoinstall";

            if (SanityCheckHelper.CancelDownloadCheck(SelectedDrive, false)) return;

            //ApplicationManager.Instance.DriveNumber = SelectedDrive.Path.Replace("Win32_DiskDrive.DeviceID=\"\\\\\\\\.\\\\PHYSICALDRIVE", "").Replace("\"", "");
            AppMan.App.IsDownloading = true;
            AppMan.Logger.Info("Starting process (Enforced Downgrade");
            AppMan.App.FireDownloadsTabEvent();
        }

        private void TroubleshootingDetailsAction()
        {
            Process.Start("https://community.cyanlabs.net/t/tutorial-sync-3-4-non-nav-apim-failure-to-update-to-newer-version-sync-3-4/1984");
        }

        private void UploadLogAction()
        {
            if (DriveLetter != "" && File.Exists(DriveLetter + @"\log.txt"))
            {
                string logfile = File.ReadAllText(DriveLetter + @"\log.txt");
                USBHelper.UploadLog(logfile);
            }
            else
            {
                ModernWpf.MessageBox.Show(LM.GetValue("MessageBox.UploadLogNoDrive"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task UploadFile()
        {
            await _usbHelper.UploadFile();
        }

        #endregion
    }
}