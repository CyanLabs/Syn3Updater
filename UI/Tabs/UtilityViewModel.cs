using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using System.Xml;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using Syn3Updater.Helper;
using Syn3Updater.Model;

namespace Syn3Updater.UI.Tabs
{
    internal class UtilityViewModel : LanguageAwareBaseViewModel
    {
        #region Constructors

        private ActionCommand _refreshUSB;
        public ActionCommand RefreshUSB => _refreshUSB ?? (_refreshUSB = new ActionCommand(RefreshUsbAction));

        private ActionCommand _logPrepareUSB;
        public ActionCommand LogPrepareUSB => _logPrepareUSB ?? (_logPrepareUSB = new ActionCommand(LogPrepareUSBAction));

        private ActionCommand _logParseXml;
        public ActionCommand LogParseXml => _logParseXml ?? (_logParseXml = new ActionCommand(LogParseXmlAction));

        private ActionCommand _gracenotesRemovalUSB;
        public ActionCommand GracenotesRemovalUSB => _gracenotesRemovalUSB ?? (_gracenotesRemovalUSB = new ActionCommand(GracenotesRemovalAction));

        private ActionCommand _smallVoiceUSB;
        public ActionCommand SmallVoiceUSB => _smallVoiceUSB ?? (_smallVoiceUSB = new ActionCommand(SmallVoiceAction));

        private ActionCommand _downgradeUSB;
        public ActionCommand DowngradeUSB => _downgradeUSB ?? (_downgradeUSB = new ActionCommand(DowngradeAction));


        private ActionCommand _troubleshootingDetails;
        public ActionCommand TroubleshootingDetails => _troubleshootingDetails ?? (_troubleshootingDetails = new ActionCommand(TroubleshootingDetailsAction));

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
            UtiltyButtonStatus = !string.IsNullOrWhiteSpace(SelectedDrive?.Name);
        }

        public void Init()
        {
            RefreshUsbAction();
        }

        private void RefreshUsbAction()
        {
            DriveList = USBHelper.refresh_devices(false);
        }

        private void UpdateDriveInfo()
        {
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

            ReloadTab();
            ApplicationManager.Logger.Info(
                $"[Utility] USB Drive selected - Name: {driveInfo.Name} - FileSystem: {driveInfo.FileSystem} - PartitionType: {driveInfo.PartitionType} - Letter: {driveInfo.Letter}");
        }

        private void LogPrepareUSBAction()
        {
            //Reset ApplicationManager variables
            ApplicationManager.Instance.Ivsus.Clear();
            ApplicationManager.Instance.DownloadOnly = false;
            ApplicationManager.Instance.DriveLetter = DriveLetter;
            ApplicationManager.Instance.Action = "logutility";
            ApplicationManager.Instance.SelectedRelease = "Interrogator Log Utility";
            ApplicationManager.Instance.Ivsus.Add(Api.InterrogatorTool);

            if (Debugger.IsAttached) ApplicationManager.Instance.Ivsus = FileHelper.DebugMode(ApplicationManager.Instance.Ivsus);

            if (SanityCheckHelper.CancelDownloadCheck(SelectedDrive, false)) return;

            ApplicationManager.Instance.DriveNumber = SelectedDrive.Path.Replace("Win32_DiskDrive.DeviceID=\"\\\\\\\\.\\\\PHYSICALDRIVE", "").Replace("\"", "");
            ApplicationManager.Instance.IsDownloading = true;
            ApplicationManager.Logger.Info($@"[App] Starting process (Logging Utility");
            ApplicationManager.Instance.FireDownloadsTabEvent();
        }

        private void LogParseXmlAction()
        {
            VistaFileDialog dialog = new VistaOpenFileDialog {Filter = "Interrogator Log XML Files|*.xml"};
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(dialog.FileName);
                    string json = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented);
                    InterrogatorModel interrogatorLog = JsonConvert.DeserializeObject<InterrogatorModel>(json, Model.Converter.Settings);
                    LogXmlDetails = $"VIN: {interrogatorLog.POtaModuleSnapShot.PVin}{Environment.NewLine}";

                    D2P1Did[] d2P1Did = interrogatorLog.POtaModuleSnapShot.PNode.D2P1EcuAcronym.D2P1State.D2P1Gateway.D2P1Did;
                    string syncappname = d2P1Did.Where(x => x.DidType == "Embedded Consumer Operating System Part Number").Select(x => x.D2P1Response).Single();
                    LogXmlDetails += $"{LanguageManager.GetValue("Utility.SyncVersion")} {syncappname}{Environment.NewLine}";
                    LogXmlDetails +=
                        $"{LanguageManager.GetValue("Utility.APIMModel")} {d2P1Did.Where(x => x.DidType == "ECU Delivery Assembly Number").Select(x => x.D2P1Response).Single()}{Environment.NewLine}";

                    string apimsize = interrogatorLog.POtaModuleSnapShot.PNode.D2P1AdditionalAttributes.D2P1PartitionHealth.Where(x => x.Type == "/fs/images/").Select(x => x.Total)
                        .Single();
                    double apimsizeint = Convert.ToDouble(apimsize.Remove(apimsize.Length - 1));
                    if (apimsizeint >= 0 && apimsizeint <= 8)
                    {
                        LogXmlDetails += $"{LanguageManager.GetValue("Utility.APIMType")} Non-Navigation {Environment.NewLine}";
                        LogXmlDetails += $"{LanguageManager.GetValue("Utility.APIMSize")} 8GB {Environment.NewLine}";
                    }
                    else if (apimsizeint >= 9 && apimsizeint <= 16)
                    {
                        LogXmlDetails += $"{LanguageManager.GetValue("Utility.APIMType")} Non-Navigation {Environment.NewLine}";
                        LogXmlDetails += $"{LanguageManager.GetValue("Utility.APIMSize")} 16GB {Environment.NewLine}";
                    }
                    else if (apimsizeint >= 17 && apimsizeint <= 32)
                    {
                        LogXmlDetails += $"{LanguageManager.GetValue("Utility.APIMType")} Navigation {Environment.NewLine}";
                        LogXmlDetails += $"{LanguageManager.GetValue("Utility.APIMSize")} 32GB {Environment.NewLine}";

                    }
                    else if (apimsizeint >= 33 && apimsizeint <= 64)
                    {
                        LogXmlDetails += $"{LanguageManager.GetValue("Utility.APIMType")} Navigation {Environment.NewLine}";
                        LogXmlDetails += $"{LanguageManager.GetValue("Utility.APIMSize")} 64GB {Environment.NewLine}";
                    }

                    string apimfree = interrogatorLog.POtaModuleSnapShot.PNode.D2P1AdditionalAttributes.D2P1PartitionHealth.Where(x => x.Type == "/fs/images/")
                        .Select(x => x.Available).Single();
                    LogXmlDetails += $"{LanguageManager.GetValue("Utility.APIMFree")} {apimfree} {Environment.NewLine}";

                    LogXmlDetails += interrogatorLog.POtaModuleSnapShot.PNode.D2P1AdditionalAttributes.LogGeneratedDateTime + Environment.NewLine;

                    LogXmlDetails += $"{Environment.NewLine}Partition Type = Free / Total";
                    foreach (D2P1PartitionHealth d2P1PartitionHealth in interrogatorLog.POtaModuleSnapShot.PNode.D2P1AdditionalAttributes.D2P1PartitionHealth)
                    {
                        LogXmlDetails += $"{Environment.NewLine}{d2P1PartitionHealth.Type} = {d2P1PartitionHealth.Available} / {d2P1PartitionHealth.Total}";
                    }

                    LogXmlDetails += $"{Environment.NewLine}{Environment.NewLine}APIM AsBuilt (Ford/UCDS)";
                    foreach (D2P1Did d2P1Didchild in d2P1Did.Where(x => x.DidType.Contains("Direct Configuraation DID DE")))
                    {
                        LogXmlDetails += $"{Environment.NewLine}{d2P1Didchild.DidValue}: {d2P1Didchild.D2P1Response.ToUpper()}";
                    }

                    ToggleLogXmlDetails = Visibility.Visible;

                    Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiSecret.Token);
                    HttpResponseMessage response = Client.GetAsync(Api.IVSUSingle + syncappname).Result;
                    Api.JsonReleases syncversion = JsonConvert.DeserializeObject<Api.JsonReleases>(response.Content.ReadAsStringAsync().Result);

                    if (syncversion.data[0].version != ApplicationManager.Instance.SyncVersion)
                    {
                        if (MessageBox.Show(string.Format(LanguageManager.GetValue("MessageBox.UpdateCurrentVersionUtility"), syncversion.data[0].version), "Syn3 Updater",
                            MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                        {
                            Properties.Settings.Default.CurrentSyncVersion = Convert.ToInt32(syncversion.data[0].version.Replace(".", ""));
                            ApplicationManager.Instance.SyncVersion = syncversion.data[0].version;
                        }
                    }

                }
                catch (NullReferenceException)
                {
                    MessageBox.Show(LanguageManager.GetValue("MessageBox.LogUtilityInvalidFile"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void GracenotesRemovalAction()
        {
            //Reset ApplicationManager variables
            ApplicationManager.Instance.Ivsus.Clear();
            ApplicationManager.Instance.DownloadOnly = false;
            ApplicationManager.Instance.DriveLetter = DriveLetter;
            ApplicationManager.Instance.Action = "gracenotesremoval";
            ApplicationManager.Instance.SelectedRelease = "Gracenotes Removal";
            ApplicationManager.Instance.Ivsus.Add(Api.GracenotesRemoval);

            if (Debugger.IsAttached) ApplicationManager.Instance.Ivsus = FileHelper.DebugMode(ApplicationManager.Instance.Ivsus);

            if (SanityCheckHelper.CancelDownloadCheck(SelectedDrive, false)) return;

            ApplicationManager.Instance.DriveNumber = SelectedDrive.Path.Replace("Win32_DiskDrive.DeviceID=\"\\\\\\\\.\\\\PHYSICALDRIVE", "").Replace("\"", "");
            ApplicationManager.Instance.IsDownloading = true;
            ApplicationManager.Logger.Info($@"[App] Starting process (Gracenotes Removal");
            ApplicationManager.Instance.FireDownloadsTabEvent();
        }

        private void SmallVoiceAction()
        {
            //Reset ApplicationManager variables
            ApplicationManager.Instance.Ivsus.Clear();
            ApplicationManager.Instance.DownloadOnly = false;
            ApplicationManager.Instance.DriveLetter = DriveLetter;
            ApplicationManager.Instance.Action = "voiceshrinker";
            ApplicationManager.Instance.SelectedRelease = "Voice Package Shrinker";
            ApplicationManager.Instance.Ivsus.Add(Api.SmallVoicePackage);

            if (Debugger.IsAttached) ApplicationManager.Instance.Ivsus = FileHelper.DebugMode(ApplicationManager.Instance.Ivsus);

            if (SanityCheckHelper.CancelDownloadCheck(SelectedDrive, false)) return;

            ApplicationManager.Instance.DriveNumber = SelectedDrive.Path.Replace("Win32_DiskDrive.DeviceID=\"\\\\\\\\.\\\\PHYSICALDRIVE", "").Replace("\"", "");
            ApplicationManager.Instance.IsDownloading = true;
            ApplicationManager.Logger.Info($@"[App] Starting process (Voice Package Shrinker");
            ApplicationManager.Instance.FireDownloadsTabEvent();
        }

        private void DowngradeAction()
        {
            //Reset ApplicationManager variables
            ApplicationManager.Instance.Ivsus.Clear();
            ApplicationManager.Instance.DownloadOnly = false;
            ApplicationManager.Instance.DriveLetter = DriveLetter;
            ApplicationManager.Instance.Action = "downgrade";
            ApplicationManager.Instance.SelectedRelease = "Enforced Downgrade";
            ApplicationManager.Instance.Ivsus.Add(Api.DowngradeApp);

            if (Debugger.IsAttached) ApplicationManager.Instance.Ivsus = FileHelper.DebugMode(ApplicationManager.Instance.Ivsus);

            if (SanityCheckHelper.CancelDownloadCheck(SelectedDrive, false)) return;

            ApplicationManager.Instance.DriveNumber = SelectedDrive.Path.Replace("Win32_DiskDrive.DeviceID=\"\\\\\\\\.\\\\PHYSICALDRIVE", "").Replace("\"", "");
            ApplicationManager.Instance.IsDownloading = true;
            ApplicationManager.Logger.Info($@"[App] Starting process (Enforced Downgrade");
            ApplicationManager.Instance.FireDownloadsTabEvent();
          
        }

        private void TroubleshootingDetailsAction()
        {
            Process.Start("https://community.cyanlabs.net/t/tutorial-sync-3-4-non-nav-apim-failure-to-update-to-newer-version-sync-3-4/1984");
        }
        #endregion
    }
}