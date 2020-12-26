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
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
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

        private bool _utilityCreateLogStep1Status;

        public bool UtilityCreateLogStep1Status
        {
            get => _utilityCreateLogStep1Status;
            set => SetProperty(ref _utilityCreateLogStep1Status, value);
        }


        #endregion

        #region Methods
        public void ReloadTab()
        {
            ToggleLogXmlDetails = Visibility.Hidden;
            UtilityCreateLogStep1Status = !ApplicationManager.Instance.UtilityCreateLogStep1Complete;
            UtilityCreateLogStep1Status = !string.IsNullOrWhiteSpace(SelectedDrive.Name);
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

            UtilityCreateLogStep1Status = !string.IsNullOrWhiteSpace(SelectedDrive.Name);
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
            
            if(Debugger.IsAttached) ApplicationManager.Instance.Ivsus = FileHelper.DebugMode(ApplicationManager.Instance.Ivsus);

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
                    Interrogator interrogatorLog = JsonConvert.DeserializeObject<Interrogator>(json, Model.Converter.Settings);
                    LogXmlDetails = $"VIN: {interrogatorLog.POtaModuleSnapShot.PVin}{Environment.NewLine}";

                    D2P1Did[] d2P1Did = interrogatorLog.POtaModuleSnapShot.PNode.D2P1EcuAcronym.D2P1State.D2P1Gateway.D2P1Did;
                    string syncappname = d2P1Did.Where(x => x.DidType == "Embedded Consumer Operating System Part Number").Select(x => x.D2P1Response).Single();
                    LogXmlDetails += LanguageManager.GetValue("Utility.SyncVersion") + syncappname + Environment.NewLine;
                    LogXmlDetails += LanguageManager.GetValue("Utility.APIMModel") + d2P1Did.Where(x => x.DidType == "ECU Delivery Assembly Number").Select(x => x.D2P1Response).Single() + Environment.NewLine;

                    string apimfree = interrogatorLog.POtaModuleSnapShot.PNode.D2P1AdditionalAttributes.D2P1PartitionHealth.Where(x => x.Type == "/fs/images/").Select(x => x.Available).Single();
                    LogXmlDetails += $"{LanguageManager.GetValue("Utility.APIMFree")} {apimfree} {Environment.NewLine}";

                    string apimsize = interrogatorLog.POtaModuleSnapShot.PNode.D2P1AdditionalAttributes.D2P1PartitionHealth.Where(x => x.Type == "/fs/images/").Select(x => x.Total).Single();
                    int apimsizeint = int.Parse(new string(apimsize.Where(char.IsDigit).ToArray()));
                    if (apimsizeint >= 0 && apimsizeint <= 8)
                    { 
                        
                        LogXmlDetails += $"{LanguageManager.GetValue("Utility.APIMSize")} 8GB {Environment.NewLine}";
                        LogXmlDetails += $"{LanguageManager.GetValue("Utility.APIMType")} Non-Navigation {Environment.NewLine}";
                    } 
                    else if (apimsizeint >= 9 && apimsizeint <= 16)
                    {
                        LogXmlDetails += $"{LanguageManager.GetValue("Utility.APIMSize")} 16GB {Environment.NewLine}";
                        LogXmlDetails += $"{LanguageManager.GetValue("Utility.APIMType")} Non-Navigation {Environment.NewLine}";
                    } 
                    else if (apimsizeint >= 17 && apimsizeint <= 32)
                    {
                        LogXmlDetails += $"{LanguageManager.GetValue("Utility.APIMSize")} 32GB {Environment.NewLine}";
                        LogXmlDetails += $"{LanguageManager.GetValue("Utility.APIMType")} Navigation {Environment.NewLine}";
                    } 
                    else if (apimsizeint >= 33 && apimsizeint <= 64)
                    {
                        LogXmlDetails += $"{LanguageManager.GetValue("Utility.APIMSize")} 64GB {Environment.NewLine}";
                        LogXmlDetails += $"{LanguageManager.GetValue("Utility.APIMType")} Navigation {Environment.NewLine}";
                    }

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
                        if (MessageBox.Show(string.Format(LanguageManager.GetValue("MessageBox.UpdateCurrentVersionUtility"), syncversion.data[0].version), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
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
        #endregion
    }
}