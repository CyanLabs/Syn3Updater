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
            //Reset ApplicationManager variables
            AppMan.App.Ivsus.Clear();
            AppMan.App.DownloadOnly = false;
            AppMan.App.DriveLetter = DriveLetter;
            AppMan.App.Action = "logutility";
            AppMan.App.SelectedRelease = "Interrogator Log Utility";

            string currentversion = AppMan.App.SVersion;
            if (currentversion.StartsWith($"3{CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator}4"))
                Api.InterrogatorTool = await ApiHelper.GetSpecialIvsu(Api.GetLogTool34);
            else if (currentversion.StartsWith($"3{CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator}2") || currentversion.StartsWith($"3{CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator}3"))
                Api.InterrogatorTool = await ApiHelper.GetSpecialIvsu(Api.GetLogTool32);
            else if (currentversion.StartsWith($"3{CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator}"))
                Api.InterrogatorTool = await ApiHelper.GetSpecialIvsu(Api.GetLogTool34);
            else
                Api.InterrogatorTool = await ApiHelper.GetSpecialIvsu(Api.GetLogTool30);

            AppMan.App.Ivsus.Add(Api.InterrogatorTool);
            AppMan.App.InstallMode = "autoinstall";
            
            if (SanityCheckHelper.CancelDownloadCheck(SelectedDrive, false)) 
                return;

            //ApplicationManager.Instance.DriveNumber = SelectedDrive.Path.Replace("Win32_DiskDrive.DeviceID=\"\\\\\\\\.\\\\PHYSICALDRIVE", "").Replace("\"", "");
            AppMan.App.IsDownloading = true;
            AppMan.Logger.Info("Starting process (Logging Utility");
            AppMan.App.FireDownloadsTabEvent();
        }

        private XDocument _node;

        public struct ApimDetails
        {
            public int Size;
            public bool Nav;
            public string PartNumber;

            // ReSharper disable once InconsistentNaming
            public string VIN;
        }

        private ApimDetails _apimDetails;

        private async Task LogParseXmlAction()
        {
            VistaFileDialog dialog = new VistaOpenFileDialog { Filter = "Interrogator Log XML Files|*.xml" };
            if (dialog.ShowDialog().GetValueOrDefault())
                try
                {
                    XmlDocument doc = new XmlDocument();
                    //TODO: swtich to Async once code moves to dotnet 5+ 
                    doc.Load(dialog.FileName);
                    string json = JsonConvert.SerializeXmlNode(doc, Formatting.Indented);
                    Interrogator.InterrogatorModel interrogatorLog = JsonConvert.DeserializeObject<Interrogator.InterrogatorModel>(json);
                    _apimDetails.VIN = interrogatorLog?.POtaModuleSnapShot.PVin;
                    LogXmlDetails = $"VIN: {interrogatorLog?.POtaModuleSnapShot.PVin}{Environment.NewLine}";

                    Interrogator.D2P1Did[] d2P1Did = interrogatorLog?.POtaModuleSnapShot.PNode.D2P1EcuAcronym.D2P1State.D2P1Gateway.D2P1Did;
                    string sappname = d2P1Did!.Where(x => x.DidType == "Embedded Consumer Operating System Part Number").Select(x => x.D2P1Response).Single();
                    LogXmlDetails += $"{LM.GetValue("Home.Version")} {sappname}{Environment.NewLine}";

                    string apimmodel = d2P1Did.Where(x => x.DidType == "ECU Delivery Assembly Number").Select(x => x.D2P1Response).Single();
                    LogXmlDetails += $"{LM.GetValue("Utility.APIMModel")} {apimmodel}{Environment.NewLine}";

                    string apimsize = interrogatorLog?.POtaModuleSnapShot.PNode.D2P1AdditionalAttributes.D2P1PartitionHealth.Where(x => x.Type == "/fs/images/")
                        .Select(x => x.Total)
                        .Single();
                    double apimsizeint = Convert.ToDouble(apimsize?.Remove(apimsize.Length - 1));
                    _apimDetails.PartNumber = apimmodel;
                    if (apimsizeint >= 0 && apimsizeint <= 8)
                    {
                        _apimDetails.Nav = false;
                        _apimDetails.Size = 8;
                    }
                    else if (apimsizeint >= 9 && apimsizeint <= 16)
                    {
                        _apimDetails.Nav = false;
                        _apimDetails.Size = 16;
                    }
                    else if (apimsizeint >= 17 && apimsizeint <= 32)
                    {
                        _apimDetails.Nav = true;
                        _apimDetails.Size = 32;
                    }
                    else if (apimsizeint >= 33 && apimsizeint <= 64)
                    {
                        _apimDetails.Nav = true;
                        _apimDetails.Size = 64;
                    }

                    if (_apimDetails.Nav)
                        LogXmlDetails += $"{LM.GetValue("Utility.APIMType")} Navigation {Environment.NewLine}";
                    else
                        LogXmlDetails += $"{LM.GetValue("Utility.APIMType")} Non-Navigation {Environment.NewLine}";

                    LogXmlDetails += $"{LM.GetValue("Utility.APIMSize")} {_apimDetails.Size}GB {Environment.NewLine}";

                    string apimfree = interrogatorLog?.POtaModuleSnapShot.PNode.D2P1AdditionalAttributes.D2P1PartitionHealth.Where(x => x.Type == "/fs/images/")
                        .Select(x => x.Available).Single();
                    LogXmlDetails += $"{LM.GetValue("Utility.APIMFree")} {apimfree} {Environment.NewLine}";

                    LogXmlDetails += interrogatorLog?.POtaModuleSnapShot.PNode.D2P1AdditionalAttributes.LogGeneratedDateTime + Environment.NewLine;

                    LogXmlDetails += $"{Environment.NewLine}Partition Type = Free / Total";
                    if (interrogatorLog != null)
                    {
                        foreach (Interrogator.D2P1PartitionHealth d2P1PartitionHealth in interrogatorLog.POtaModuleSnapShot.PNode.D2P1AdditionalAttributes.D2P1PartitionHealth)
                            LogXmlDetails += $"{Environment.NewLine}{d2P1PartitionHealth.Type} = {d2P1PartitionHealth.Available} / {d2P1PartitionHealth.Total}";

                        List<AsBuilt.DID> asBuiltValues = new List<AsBuilt.DID>();

                        LogXmlDetails += $"{Environment.NewLine}{Environment.NewLine}APIM AsBuilt";
                        foreach (Interrogator.D2P1Did d2P1Didchild in d2P1Did.Where(x => x.DidType.Contains("Direct Configuraation DID DE")))
                        {
                            LogXmlDetails += $"{Environment.NewLine}{d2P1Didchild.DidValue}: {d2P1Didchild.D2P1Response.ToUpper()}";
                            asBuiltValues.Add(new AsBuilt.DID { ID = d2P1Didchild.DidValue, Text = d2P1Didchild.D2P1Response.ToUpper() });
                        }

                        ToggleLogXmlDetails = Visibility.Visible;

                        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiSecret.Token);
                        try
                        {
                            HttpResponseMessage response = await Client.GetAsync(Api.IvsuSingle + sappname);
                            Api.JsonReleases sversion = JsonHelpers.Deserialize<Api.JsonReleases>(await response.Content.ReadAsStreamAsync());
                            string convertedsversion = sversion.Releases[0].Version.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                            if (convertedsversion != AppMan.App.SVersion)
                                if (ModernWpf.MessageBox.Show(string.Format(LM.GetValue("MessageBox.UpdateCurrentVersionUtility"), AppMan.App.SVersion, convertedsversion),
                                    "Syn3 Updater",
                                    MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                                {
                                    AppMan.App.Settings.CurrentVersion = Convert.ToInt32(sversion.Releases[0].Version.Replace(".", ""));
                                    AppMan.App.SVersion = convertedsversion;
                                }
                        }
                        catch (Exception)
                        {
                            //likely no internet connection ignore
                        }

                        AsBuilt.DirectConfiguration asbult = new AsBuilt.DirectConfiguration
                        {
                            VEHICLE = new AsBuilt.VEHICLE
                            {
                                MODULE = "Syn3Updater",
                                VIN = "",
                                VEHICLEID = apimmodel,
                                VEHICLEYEAR = interrogatorLog.POtaModuleSnapShot.PNode.D2P1AdditionalAttributes.LogGeneratedDateTime.ToString(),
                                DID = asBuiltValues
                            }
                        };
                        json = JsonConvert.SerializeObject(asbult, Formatting.Indented);
                    }

                    _node = JsonConvert.DeserializeXNode(json, "DirectConfiguration");
                }
                catch (NullReferenceException)
                {
                    ModernWpf.MessageBox.Show(LM.GetValue("MessageBox.LogUtilityInvalidFile"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (XmlException)
                {
                    ModernWpf.MessageBox.Show(LM.GetValue("MessageBox.LogUtilityInvalidFile"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Error);
                }
        }

        private async Task UploadFile()
        {
            if (_node != null && ModernWpf.MessageBox.Show(LM.GetValue("MessageBox.AsBuiltVinWarning"), "Syn3 Updater", MessageBoxButton.OKCancel, MessageBoxImage.Information) ==
                    MessageBoxResult.OK)
            {
                FormUrlEncodedContent formContent = new FormUrlEncodedContent(new[]
                {
                        new KeyValuePair<string, string>("xml", _node.ToString()),
                        new KeyValuePair<string, string>("apim", _apimDetails.PartNumber),
                        new KeyValuePair<string, string>("nav", _apimDetails.Nav.ToString()),
                        new KeyValuePair<string, string>("size", _apimDetails.Size.ToString()),
                        new KeyValuePair<string, string>("vin", _apimDetails.VIN)
                    });
                HttpResponseMessage response = await Client.PostAsync(Api.AsBuiltPost, formContent);               
                var output = JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new { filename = "", status = "" });
                Process.Start(Api.AsBuiltOutput + output.filename);
            }
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

        #endregion
    }
}