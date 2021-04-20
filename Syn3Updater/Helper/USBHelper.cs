using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using Cyanlabs.Syn3Updater.Model;
using Cyanlabs.Updater.Common;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using Formatting = Newtonsoft.Json.Formatting;

namespace Cyanlabs.Syn3Updater.Helper
{
    /// <summary>
    ///     Helper class for USB related methods such as reading disk information via WMI
    /// </summary>
    public class USBHelper
    {
        #region Properties & Fields

        public struct DriveInfo
        {
            public string Letter;
            public string Name;
            public string FileSystem;
            public string PartitionType;
            public bool SkipFormat;
        }

        public class Drive
        {
            public string Path { get; set; }
            public string Name { get; set; }
        }

        #endregion

        #region Methods
        /// <summary>
        ///     Refreshes device list using WMI queries
        /// </summary>
        /// <param name="fakeusb">Set to true to add 'Download Only' option to the list</param>
        /// <returns>ObservableCollection of all USB Drives as type Drive</returns>
        public static ObservableCollection<Drive> RefreshDevices(bool fakeusb)
        {
            ObservableCollection<Drive> driveList = new ObservableCollection<Drive>
            {
                fakeusb ? new Drive { Path = "", Name = LM.GetValue("Home.NoUSB") } : new Drive { Path = "", Name = "" }
            };
            ManagementObjectSearcher driveQuery = new ManagementObjectSearcher("select * from Win32_DiskDrive Where InterfaceType = \"USB\" OR MediaType = \"External hard disk media\"");
            foreach (ManagementBaseObject o in driveQuery.Get())
            {
                ManagementObject d = (ManagementObject)o;
                string diskName = Convert.ToString(d.Properties["Caption"].Value);
                string friendlySize = MathHelper.BytesToString(Convert.ToInt64(d.Properties["Size"].Value));
                if (friendlySize != "0B")
                    // Add to array of drives
                    driveList.Add(new Drive { Path = d.Path.RelativePath, Name = $"{diskName} {friendlySize}" });
            }
            if (fakeusb)
                driveList.Add(new Drive { Path = "", Name = LM.GetValue("Home.NoUSBDir") });

            // Return a list of drives
            return driveList;
        }

        /// <summary>
        ///     Get detailed information of selectedDrive
        /// </summary>
        /// <param name="selectedDrive"></param>
        /// <returns>Details of the selectedDrive as type DriveInfo</returns>
        public static DriveInfo UpdateDriveInfo(Drive selectedDrive)
        {
            DriveInfo driveInfo = new DriveInfo();
            if (selectedDrive == null || selectedDrive.Name == LM.GetValue("Home.NoUSB") || selectedDrive.Name == LM.GetValue("Home.NoUSBDir") || selectedDrive.Path?.Length == 0) return driveInfo;

            string partitionQueryText = $@"associators of {{{selectedDrive.Path}}} where AssocClass = Win32_DiskDriveToDiskPartition";
            ManagementObjectSearcher partitionQuery = new ManagementObjectSearcher(partitionQueryText);
            foreach (ManagementBaseObject o in partitionQuery.Get())
            {
                ManagementObject p = (ManagementObject)o;
                string logicalDriveQueryText = $@"associators of {{{p.Path.RelativePath}}} where AssocClass = Win32_LogicalDiskToPartition";
                ManagementObjectSearcher logicalDriveQuery = new ManagementObjectSearcher(logicalDriveQueryText);
                foreach (ManagementBaseObject managementBaseObject in logicalDriveQuery.Get())
                {
                    ManagementObject ld = (ManagementObject)managementBaseObject;
                    driveInfo.Letter = Convert.ToString(ld.Properties["DeviceId"].Value);
                    driveInfo.PartitionType = p.Properties["Type"].Value.ToString().Contains("GPT:") ? "GPT" : "MBR";
                    driveInfo.FileSystem += Convert.ToString(ld.Properties["FileSystem"].Value);
                    driveInfo.Name = ld.Properties["VolumeName"].Value.ToString();

                    if (driveInfo.FileSystem == "exFAT" && driveInfo.PartitionType == "MBR" && driveInfo.Name == "CYANLABS")
                        driveInfo.SkipFormat = true;
                }
            }

            return driveInfo;
        }

        /// <summary>
        ///     Generates a log.txt file on the root of the USB Drive
        /// </summary>
        /// <param name="log">Additional log to append, usually the log textbox</param>
        /// <param name="upload">Set to true to upload log file <see cref="UploadLog"/>, else false to only save it to USB drive</param>
        public static void GenerateLog(string log, bool upload)
        {
            StringBuilder data = new StringBuilder($@"CYANLABS - SYN3 UPDATER - V{Assembly.GetExecutingAssembly().GetName().Version}{Environment.NewLine}");
            data.Append(@"Branch: ").Append(AppMan.App.LauncherPrefs.ReleaseTypeInstalled).Append(Environment.NewLine)
                .Append(@"Operating System: ").Append(SystemHelper.GetOsFriendlyName()).Append(Environment.NewLine)
                .Append(Environment.NewLine)
                .Append($@"PREVIOUS CONFIGURATION{Environment.NewLine}")
                .Append($@"Version: {AppMan.App.SVersion}{Environment.NewLine}")
                .Append($@"Region: {AppMan.App.Settings.CurrentRegion}{Environment.NewLine}")
                .Append($@"Navigation: {AppMan.App.Settings.CurrentNav}{Environment.NewLine}")
                .Append($@"Mode: {AppMan.App.InstallMode}{Environment.NewLine}")
                .Append($@"Install Mode Overridden: {AppMan.App.ModeForced}{Environment.NewLine}")
                .Append($@"My20 Protection Enabled: {AppMan.App.Settings.My20}{Environment.NewLine}")
                .Append(Environment.NewLine).Append("DESTINATION DETAILS").Append(Environment.NewLine);
            if (AppMan.App.DownloadToFolder)
            {
                data.Append("Mode: Directory").Append(Environment.NewLine)
                    .Append(@"Path: ").Append(AppMan.App.DriveLetter).Append(Environment.NewLine);
            }
            else
            {
                data.Append("Mode: Drive").Append(Environment.NewLine)
                    .Append("Model: ").Append(AppMan.App.DriveName).Append(Environment.NewLine)
                    .Append("FileSystem: ").Append(AppMan.App.DriveFileSystem).Append(Environment.NewLine)
                    .Append("Partition Type: ").Append(AppMan.App.DrivePartitionType).Append(Environment.NewLine);
            }

            string driveletter = AppMan.App.DriveLetter;
            if (File.Exists($@"{driveletter}\reformat.lst"))
            {
                data.Append(Environment.NewLine)
                    .Append("REFORMAT.LST").Append(Environment.NewLine)
                    .Append(File.ReadAllText($@"{driveletter}\reformat.lst")).Append(Environment.NewLine);
            }

            if (File.Exists($@"{driveletter}\autoinstall.lst"))
            {
                data.Append(Environment.NewLine)
                    .Append("AUTOINSTALL.LST").Append(Environment.NewLine)
                    .Append(File.ReadAllText($@"{driveletter}\autoinstall.lst")).Append(Environment.NewLine);
            }

            if (Directory.Exists($@"{driveletter}\SyncMyRide"))
            {
                data.Append(Environment.NewLine);
                DirectoryInfo di = new DirectoryInfo($@"{driveletter}\SyncMyRide");
                FileInfo[] allFiles = di.GetFiles("*", SearchOption.AllDirectories);
                data.Append($"FILES ({allFiles.Length}){Environment.NewLine}");
                foreach (FileInfo file in allFiles)
                    data.Append($"{file.Name} ({MathHelper.BytesToString(file.Length)}){Environment.NewLine}");
                data.Append(Environment.NewLine);
            }

            data.Append("LOG").Append(Environment.NewLine)
                .Append(log);
            string complete = data.ToString();
            File.WriteAllText($@"{driveletter}\log.txt", complete);

            if (upload)
                UploadLog(complete);
        }

        /// <summary>
        ///     Uploads log to our API server for easy diagnostics
        /// </summary>
        /// <param name="log">Contents of log file</param>
        public static void UploadLog(string log)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
            {
                {"contents", log}
            };
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.PostAsync(Api.LogPost, new FormUrlEncodedContent(values)).GetAwaiter().GetResult();
            string responseString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var output = JsonConvert.DeserializeAnonymousType(responseString, new { uuid = "", status = "" });
            Process.Start(Api.LogUrl + output.uuid);
        }

        //TODO: Fma965: refactor/move this
        public async Task UploadFile()
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
                HttpResponseMessage response = await AppMan.App.Client.PostAsync(Api.AsBuiltPost, formContent);
                var output = JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new { filename = "", status = "" });
                Process.Start(Api.AsBuiltOutput + output.filename);
            }
        }

        //TODO: Fma965: refactor/move this
        public struct ApimDetails
        {
            public int Size;
            public bool Nav;
            public string PartNumber;
            public string VIN;
        }

        private ApimDetails _apimDetails;
        private XDocument _node;

        public async Task<string> LogParseXmlAction()
        {
            string LogXmlDetails = "";
            VistaFileDialog dialog = new VistaOpenFileDialog { Filter = "Interrogator Log XML Files|*.xml" };
            if (!dialog.ShowDialog().GetValueOrDefault()) return LogXmlDetails;
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

                string result = AppMan.App.Client.GetStringAsync(Api.My20URL).Result;
                Api.My20Models output = JsonConvert.DeserializeObject<Api.My20Models>(result);
                foreach (string my20 in output.My20Model)
                {
                    if (apimmodel.Contains(my20))
                        AppMan.App.Settings.My20 = true;
                }

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

                    AppMan.App.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiSecret.Token);
                    try
                    {
                        HttpResponseMessage response = await AppMan.App.Client.GetAsync(Api.IvsuSingle + sappname);
                        Api.JsonReleases sversion = JsonHelpers.Deserialize<Api.JsonReleases>(await response.Content.ReadAsStreamAsync());
                        string convertedsversion = sversion.Releases[0].Version.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                        if (AppMan.App.Action == "logutilitymy20")
                        {
                            AppMan.App.Settings.CurrentVersion = Convert.ToInt32(sversion.Releases[0].Version.Replace(".", ""));
                            AppMan.App.SVersion = convertedsversion;
                        }
                        else if (convertedsversion != AppMan.App.SVersion)
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
            return LogXmlDetails;
        }

        public static async Task LogPrepareUSBAction(USBHelper.Drive selectedDrive, string driveLetter, string action = "logutility")
        {
            //Reset ApplicationManager variables
            AppMan.App.Ivsus.Clear();
            AppMan.App.DownloadOnly = false;
            AppMan.App.DriveLetter = driveLetter;
            AppMan.App.Action = action;
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

            if (SanityCheckHelper.CancelDownloadCheck(selectedDrive, false))
                return;

            AppMan.App.IsDownloading = true;
            AppMan.Logger.Info("Starting process (Logging Utility");
            AppMan.App.FireDownloadsTabEvent();
        }
        #endregion

    }
}