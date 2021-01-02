using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using Syn3Updater.Helper;
using Syn3Updater.Model;
using Formatting = Newtonsoft.Json.Formatting;
using MessageBox = ModernWpf.MessageBox;

namespace Syn3Updater.UI.Tabs
{
    internal class UtilityViewModel : LanguageAwareBaseViewModel
    {
        #region Constructors

        private ActionCommand viewAsBuilt;
        public ActionCommand ViewAsBuilt => viewAsBuilt ?? (viewAsBuilt = new ActionCommand(UploadFile));

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
            ApplicationManager.Logger.Info(@"[App] Starting process (Logging Utility");
            ApplicationManager.Instance.FireDownloadsTabEvent();
        }

        private XDocument node;
        public struct SyncAPIMDetails
        {
            public int Size;
            public bool Nav;
            public string PartNumber;
            public string VIN;
        }
        SyncAPIMDetails syncAPIMDetails;
        private void LogParseXmlAction()
        {
            VistaFileDialog dialog = new VistaOpenFileDialog {Filter = "Interrogator Log XML Files|*.xml"};
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(dialog.FileName);
                    string json = JsonConvert.SerializeXmlNode(doc, Formatting.Indented);
                    InterrogatorModel interrogatorLog = JsonConvert.DeserializeObject<InterrogatorModel>(json, Model.Converter.Settings);
                    syncAPIMDetails.VIN = interrogatorLog.POtaModuleSnapShot.PVin;
                    LogXmlDetails = $"VIN: {interrogatorLog.POtaModuleSnapShot.PVin}{Environment.NewLine}";

                    D2P1Did[] d2P1Did = interrogatorLog.POtaModuleSnapShot.PNode.D2P1EcuAcronym.D2P1State.D2P1Gateway.D2P1Did;
                    string syncappname = d2P1Did.Where(x => x.DidType == "Embedded Consumer Operating System Part Number").Select(x => x.D2P1Response).Single();
                    LogXmlDetails += $"{LanguageManager.GetValue("Utility.SyncVersion")} {syncappname}{Environment.NewLine}";

                    string apimmodel = d2P1Did.Where(x => x.DidType == "ECU Delivery Assembly Number").Select(x => x.D2P1Response).Single();
                    LogXmlDetails +=  $"{LanguageManager.GetValue("Utility.APIMModel")} {apimmodel}{Environment.NewLine}";

                    string apimsize = interrogatorLog.POtaModuleSnapShot.PNode.D2P1AdditionalAttributes.D2P1PartitionHealth.Where(x => x.Type == "/fs/images/").Select(x => x.Total)
                        .Single();
                    double apimsizeint = Convert.ToDouble(apimsize.Remove(apimsize.Length - 1));
                    syncAPIMDetails.PartNumber = apimmodel;
                    if (apimsizeint >= 0 && apimsizeint <= 8)
                    {
                        syncAPIMDetails.Nav = false;
                        syncAPIMDetails.Size = 8;

                    }
                    else if (apimsizeint >= 9 && apimsizeint <= 16)
                    {
                        syncAPIMDetails.Nav = false;
                        syncAPIMDetails.Size = 16;
                    }
                    else if (apimsizeint >= 17 && apimsizeint <= 32)
                    {
                        syncAPIMDetails.Nav = true;
                        syncAPIMDetails.Size = 32;

                    }
                    else if (apimsizeint >= 33 && apimsizeint <= 64)
                    {
                        syncAPIMDetails.Nav = true;
                        syncAPIMDetails.Size = 64;
                    }

                    if (syncAPIMDetails.Nav)
                    {
                        LogXmlDetails += $"{LanguageManager.GetValue("Utility.APIMType")} Navigation {Environment.NewLine}";
                    } else {
                        LogXmlDetails += $"{LanguageManager.GetValue("Utility.APIMType")} Non-Navigation {Environment.NewLine}";
                    }
                    LogXmlDetails += $"{LanguageManager.GetValue("Utility.APIMSize")} {syncAPIMDetails.Size}GB {Environment.NewLine}";

                    string apimfree = interrogatorLog.POtaModuleSnapShot.PNode.D2P1AdditionalAttributes.D2P1PartitionHealth.Where(x => x.Type == "/fs/images/")
                        .Select(x => x.Available).Single();
                    LogXmlDetails += $"{LanguageManager.GetValue("Utility.APIMFree")} {apimfree} {Environment.NewLine}";

                    LogXmlDetails += interrogatorLog.POtaModuleSnapShot.PNode.D2P1AdditionalAttributes.LogGeneratedDateTime + Environment.NewLine;

                    LogXmlDetails += $"{Environment.NewLine}Partition Type = Free / Total";
                    foreach (D2P1PartitionHealth d2P1PartitionHealth in interrogatorLog.POtaModuleSnapShot.PNode.D2P1AdditionalAttributes.D2P1PartitionHealth)
                    {
                        LogXmlDetails += $"{Environment.NewLine}{d2P1PartitionHealth.Type} = {d2P1PartitionHealth.Available} / {d2P1PartitionHealth.Total}";
                    }

                    List<DID> asBuiltValues = new List<DID>();

                    LogXmlDetails += $"{Environment.NewLine}{Environment.NewLine}APIM AsBuilt (Ford/UCDS)";
                    foreach (D2P1Did d2P1Didchild in d2P1Did.Where(x => x.DidType.Contains("Direct Configuraation DID DE")))
                    {
                        LogXmlDetails += $"{Environment.NewLine}{d2P1Didchild.DidValue}: {d2P1Didchild.D2P1Response.ToUpper()}";
                        asBuiltValues.Add(new DID {ID = d2P1Didchild.DidValue, Text = d2P1Didchild.D2P1Response.ToUpper()});
                    }

                    ToggleLogXmlDetails = Visibility.Visible;

                    Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiSecret.Token);
                    HttpResponseMessage response = Client.GetAsync(Api.IVSUSingle + syncappname).Result;
                    Api.JsonReleases syncversion = JsonConvert.DeserializeObject<Api.JsonReleases>(response.Content.ReadAsStringAsync().Result);
                    string convertedsyncversion = syncversion.data[0].version.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                    if (convertedsyncversion != ApplicationManager.Instance.SyncVersion)
                    {
                        if (MessageBox.Show(string.Format(LanguageManager.GetValue("MessageBox.UpdateCurrentVersionUtility"), convertedsyncversion), "Syn3 Updater",
                            MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                        {
                            Properties.Settings.Default.CurrentSyncVersion = Convert.ToInt32(syncversion.data[0].version.Replace(".", ""));
                            ApplicationManager.Instance.SyncVersion = convertedsyncversion;
                        }
                    }

                    DirectConfiguration asbult = new DirectConfiguration
                    {
                        VEHICLE = new VEHICLE
                        {
                            MODULE = "Syn3Updater",
                            VIN = "",
                            VEHICLEID = apimmodel,
                            VEHICLEYEAR = interrogatorLog.POtaModuleSnapShot.PNode.D2P1AdditionalAttributes.LogGeneratedDateTime.ToString(),
                            DID = asBuiltValues
                        }
                    };
                    json = JsonConvert.SerializeObject(asbult, Formatting.Indented);
                    node = JsonConvert.DeserializeXNode(json, "DirectConfiguration");

                }
                catch (NullReferenceException)
                {
                    MessageBox.Show(LanguageManager.GetValue("MessageBox.LogUtilityInvalidFile"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void UploadFile()
        {
            if (node != null)
            {
                if (MessageBox.Show(LanguageManager.GetValue("MessageBox.AsBuiltVinWarning"), "Syn3 Updater", MessageBoxButton.OKCancel, MessageBoxImage.Information) ==
                    MessageBoxResult.OK)
                {
                    var formContent = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("xml", node.ToString()),
                        new KeyValuePair<string, string>("apim", syncAPIMDetails.PartNumber),
                        new KeyValuePair<string, string>("nav", syncAPIMDetails.Nav.ToString()),
                        new KeyValuePair<string, string>("size", syncAPIMDetails.Size.ToString()),
                        new KeyValuePair<string, string>("vin", syncAPIMDetails.VIN)
                    });
                    var response = await Client.PostAsync(Api.AsBuiltPost, formContent);
                    var definition = new { filename = "", status = "" };
                    var contents = await response.Content.ReadAsStringAsync();
                    var output = JsonConvert.DeserializeAnonymousType(contents, definition);
                    Process.Start(Api.AsBuiltOutput + output.filename);
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
            ApplicationManager.Logger.Info(@"[App] Starting process (Gracenotes Removal");
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
            ApplicationManager.Logger.Info(@"[App] Starting process (Voice Package Shrinker");
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
            ApplicationManager.Logger.Info(@"[App] Starting process (Enforced Downgrade");
            ApplicationManager.Instance.FireDownloadsTabEvent();
          
        }

        private void TroubleshootingDetailsAction()
        {
            Process.Start("https://community.cyanlabs.net/t/tutorial-sync-3-4-non-nav-apim-failure-to-update-to-newer-version-sync-3-4/1984");
        }
        #endregion
    }
}