using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Syn3Updater.Models;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using GraphQL;
using Newtonsoft.Json;
using Syn3Updater.Converters;
using Syn3Updater.Helpers.Windows;
using Syn3Updater.Models.Mac;
using Formatting = Newtonsoft.Json.Formatting;


namespace Syn3Updater.Helpers
{
    public class USBHelper
    {
        public struct ApimDetails
        {
            public int Size;
            public bool Nav;
            public string PartNumber;
            public string Vin;
        }

        /// <summary>
        ///     Refreshes device list using WMI queries
        /// </summary>
        /// <param name="fakeusb">Set to true to add 'Download Only' option to the list</param>
        /// <returns>ObservableCollection of all USB Drives as type Drive</returns>
        [SupportedOSPlatform("windows")]
        public static ObservableCollection<USBDriveModel.Drive> RefreshDevicesWindows()
        {
            try
            {
                ObservableCollection<USBDriveModel.Drive> driveList = new();
                ManagementObjectSearcher driveQuery = new("select * from Win32_DiskDrive Where InterfaceType = \"USB\" OR MediaType = \"External hard disk media\"");
                foreach (ManagementBaseObject n in driveQuery.Get())
                {
                    ManagementObject d = (ManagementObject)n;
                    string friendlySize = MathHelper.BytesToString(Convert.ToInt64(d.Properties["Size"].Value));

                    if (friendlySize == "0B") continue;

                    USBDriveModel.Drive drive = new();
                    string partitionQueryText = $@"associators of {{{d.Path.RelativePath}}} where AssocClass = Win32_DiskDriveToDiskPartition";
                    ManagementObjectSearcher partitionQuery = new(partitionQueryText);
                    foreach (ManagementBaseObject o in partitionQuery.Get())
                    {
                        ManagementObject p = (ManagementObject)o;
                        string logicalDriveQueryText = $@"associators of {{{p.Path.RelativePath}}} where AssocClass = Win32_LogicalDiskToPartition";
                        ManagementObjectSearcher logicalDriveQuery = new(logicalDriveQueryText);
                        foreach (ManagementBaseObject managementBaseObject in logicalDriveQuery.Get())
                        {
                            ManagementObject ld = (ManagementObject)managementBaseObject;
                            ManagementObjectSearcher encryptedDriveQuery = new("\\\\.\\ROOT\\CIMV2\\Security\\MicrosoftVolumeEncryption",$"select * from Win32_EncryptableVolume Where DriveLetter = \"{ld.Properties["DeviceId"].Value}\"");
                            foreach (ManagementBaseObject encryptedDriveObject in encryptedDriveQuery.Get())
                            {
                                uint encryptionStatus = (uint)encryptedDriveObject.GetPropertyValue("ProtectionStatus");
                                drive.Encrypted = encryptionStatus != 0;
                                if (drive.Encrypted) drive.EncryptionStatus = "Encrypted";
                            }

                            if (!drive.Encrypted)
                            {
                                drive.FileSystem += ld.GetPropertyValue("FileSystem");
                                drive.VolumeName = string.IsNullOrWhiteSpace((string)ld.GetPropertyValue("VolumeName")) ? "" : ld.GetPropertyValue("VolumeName").ToString();
                            }

                            drive.Path = ld.GetPropertyValue("DeviceId").ToString();
                            drive.PartitionType = p.GetPropertyValue("Type").ToString()!.Contains("GPT:") ? "GPT" : "MBR";
                            
                            drive.Name = d.GetPropertyValue("Caption").ToString();
                            
                            drive.RelativePath = d.Path.RelativePath;
                            drive.FreeSpace = MathHelper.BytesToString(Convert.ToInt64(ld.GetPropertyValue("FreeSpace")));
                            drive.Model = d.GetPropertyValue("Model").ToString();
                            drive.Size = friendlySize;
                            drive.Fake = false;
                            if (drive.FileSystem == "exFAT" && drive.PartitionType == "MBR" && drive.VolumeName == "CYANLABS")
                                drive.SkipFormat = true;
                            else
                                drive.SkipFormat = false;
                        }
                    }

                    // Add to array of drives
                    driveList.Add(drive);
                }

                //TODO Reimplement folder function?

                // Return a list of drives
                return driveList;
            }
            catch (ManagementException)
            {
                //TODO Exception Handling
                return new ObservableCollection<USBDriveModel.Drive>();
            }
        }

        public static ObservableCollection<USBDriveModel.Drive> RefreshDevicesMac()
        {
            try
            {
                ObservableCollection<USBDriveModel.Drive> driveList = new();
                IEnumerable<DriveInfo> allDrives = DriveInfo.GetDrives().Where(x => x.Name != "/" && !x.Name.Contains("/System/Volumes") && !x.Name.Contains("/dev"));
                foreach (var driveInfo in allDrives)
                {
                    USBDriveModel.Drive drive = new();
                    DiskUtilModel.DiskUtilInfo diskUtilInfo = new();
                    string output;
                    if (OperatingSystem.IsMacOS())
                    {
                        using (Process p = new())
                        {
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.RedirectStandardOutput = true;
                            p.StartInfo.FileName = "diskutil";
                            p.StartInfo.Arguments = $"info {driveInfo.Name}";
                            p.StartInfo.CreateNoWindow = true;
                            p.Start();
                            output = p.StandardOutput.ReadToEnd();
                            p.WaitForExit();
                        }

                        drive.Encrypted = false;
                        drive.Fake = false;
                        drive.FileSystem = driveInfo.DriveFormat;
                        drive.Size = MathHelper.BytesToString(Convert.ToInt64(driveInfo.TotalSize));
                        drive.FreeSpace = MathHelper.BytesToString(Convert.ToInt64(driveInfo.AvailableFreeSpace));
                        drive.VolumeName = driveInfo.Name.Replace("/Volumes/","");
                        drive.IsMac = true;
                    }
                    else
                    {
                        //TODO Remove Debug
                        output = File.ReadAllText("D:\\diskutil.txt");
                    }
                    output = Regex.Replace(output, @"(\s)\s+", "$1");

                    foreach (string line in output.Split(new[] { "\r\n", "\r", "\n" },StringSplitOptions.None))
                    {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;
                            string[] namevalue = line.Split(": ");
                            if (namevalue[0].Contains("Part of Whole")) diskUtilInfo.PartOfWhole = namevalue[1];
                            if (namevalue[0].Contains("Removable Media")) diskUtilInfo.RemovableMedia = namevalue[1];
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                    
                    drive.Path = $"/dev/{diskUtilInfo.PartOfWhole}";
                    //TODO remove uneeded check later
                    if (OperatingSystem.IsMacOS())
                    {
                        if (diskUtilInfo.RemovableMedia == "Removable")
                        {
                            using (Process p = new())
                            {
                                p.StartInfo.UseShellExecute = false;
                                p.StartInfo.RedirectStandardOutput = true;
                                p.StartInfo.FileName = "diskutil";
                                p.StartInfo.Arguments = $"info {drive.Path}";
                                p.StartInfo.CreateNoWindow = true;
                                p.Start();
                                output = p.StandardOutput.ReadToEnd();
                                p.WaitForExit();
                            }
                            
                            output = Regex.Replace(output, @"(\s)\s+", "$1");

                            foreach (string line in output.Split(new[] { "\r\n", "\r", "\n" },StringSplitOptions.None))
                            {
                                try
                                {
                                    if (string.IsNullOrWhiteSpace(line)) continue;
                                    string[] namevalue = line.Split(": ");
                                    if (namevalue[0].Contains("Content (IOContent)")) diskUtilInfo.Content = namevalue[1];
                                    if (namevalue[0].Contains("Device / Media")) diskUtilInfo.DeviceMediaName = namevalue[1];
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            }

                            drive.PartitionType = diskUtilInfo.Content switch
                            {
                                "FDisk_partition_scheme" => "MBR",
                                "GUID_partition_scheme" => "GPT",
                                _ => "???"
                            };

                            drive.SkipFormat = drive.FileSystem switch
                            {
                                "exfat" when drive.PartitionType == "MBR" && drive.VolumeName == "CYANLABS" => true,
                                _ => false
                            };
                            drive.Name = diskUtilInfo.DeviceMediaName;
                            drive.Model = diskUtilInfo.DeviceMediaName; //TODO Get manufacturer somehow? 
                        }
                    }
                    driveList.Add(drive);
                }
                return driveList;
            }
            catch (Exception)
            {
                //TODO Exception Handling
                return new ObservableCollection<USBDriveModel.Drive>();
            }
        }
        
        public static async Task LogPrepareUSBAction(USBDriveModel.Drive? selectedDrive, string driveLetter, string currentversion, string action = "logutility")
        {
            //Reset ApplicationManager variables
            
            AppMan.App.Ivsus.Clear();
            AppMan.App.DrivePath = driveLetter;
            AppMan.App.Action = action;
            AppMan.App.SelectedRelease = "Interrogator Log Utility";
            
            if (currentversion.StartsWith("3.4"))
                Api.InterrogatorTool = await ApiHelper.GetSpecialIvsu(Api.SpecialPackages.LogTool34);
            else if ((currentversion.StartsWith("3.2") || currentversion.StartsWith("3.3")))
                Api.InterrogatorTool = await ApiHelper.GetSpecialIvsu(Api.SpecialPackages.LogTool32);
            else if ((currentversion.StartsWith("3.0") || currentversion.StartsWith("2.") || currentversion.StartsWith("1.")))
                Api.InterrogatorTool = await ApiHelper.GetSpecialIvsu(Api.SpecialPackages.LogTool30);
            else
                Api.InterrogatorTool = await ApiHelper.GetSpecialIvsu(Api.SpecialPackages.LogTool34);

            AppMan.App.Ivsus.Add(Api.InterrogatorTool);
            AppMan.App.InstallMode = "autoinstall";

            //TODO Redo sanity checks
            //if (await SanityCheckHelper.CancelDownloadCheck(selectedDrive) || Api.InterrogatorTool == null)
            //     return;

            AppMan.App.IsDownloading = true;
            AppMan.App.FireDownloadsStartEvent();
        }
        
        /// <summary>
        ///     Generates a log.txt file on the root of the USB Drive and a log-date.txt file in LogPath
        /// </summary>
        /// <param name="log">Additional log to append, usually the log textbox</param>
        /// <param name="upload">Set to true to upload log file <see cref="UploadLog" />, else false to only save it to USB drive</param>
        public static async Task GenerateLog(string log, bool upload)
        {
            StringBuilder data = new($@"CYANLABS - SYN3 UPDATER - V{Assembly.GetExecutingAssembly().GetName().Version}{Environment.NewLine}");
            if (OperatingSystem.IsWindows())
            {
                data.Append(@"Operating System: ").Append(WindowsSystemHelper.GetOsFriendlyName()).Append(Environment.NewLine);
            }
            else if (OperatingSystem.IsMacOS())
            {
                //TODO Add Operating System Check 
            }
            data.Append(Environment.NewLine)
                .Append($@"PREVIOUS CONFIGURATION{Environment.NewLine}")
                .Append($@"Version: {AppMan.App.SVersion}{Environment.NewLine}")
                .Append($@"Region: {AppMan.App.Settings.CurrentRegion}{Environment.NewLine}")
                .Append($@"Navigation: {AppMan.App.Settings.CurrentNav}{Environment.NewLine}")
                .Append($@"Install Mode: {AppMan.App.Settings.InstallMode} ({AppMan.App.InstallMode}){Environment.NewLine}")
                .Append($@"Install Mode Overridden: {AppMan.App.ModeForced}{Environment.NewLine}");

            if (AppMan.App.Settings.My20V2 == null)
                data.Append($@"My20 Protection Enabled: AutoDetect{Environment.NewLine}");
            else if (AppMan.App.Settings.My20V2 == true)
                data.Append($@"My20 Protection Enabled: Enabled{Environment.NewLine}");
            else if (AppMan.App.Settings.My20V2 == false)
                data.Append($@"My20 Protection Enabled: Disabled{Environment.NewLine}");

            data.Append(Environment.NewLine).Append("DESTINATION DETAILS").Append(Environment.NewLine);
            if (AppMan.App.DownloadToFolder)
                data.Append("Mode: Directory").Append(Environment.NewLine)
                    .Append(@"Path: ").Append(AppMan.App.DrivePath).Append(Environment.NewLine);
            else
                data.Append("Mode: Drive").Append(Environment.NewLine)
                    .Append("Model: ").Append(AppMan.App.DriveName).Append(Environment.NewLine)
                    .Append("FileSystem: ").Append(AppMan.App.DriveFileSystem).Append(Environment.NewLine)
                    .Append("Partition Type: ").Append(AppMan.App.DrivePartitionType).Append(Environment.NewLine);

            string driveletter = AppMan.App.DrivePath;
            if (File.Exists($@"{driveletter}\reformat.lst"))
                data.Append(Environment.NewLine)
                    .Append("REFORMAT.LST").Append(Environment.NewLine)
                    .Append(File.ReadAllText($@"{driveletter}\reformat.lst")).Append(Environment.NewLine);

            if (File.Exists($@"{driveletter}\autoinstall.lst"))
                data.Append(Environment.NewLine)
                    .Append("AUTOINSTALL.LST").Append(Environment.NewLine)
                    .Append(File.ReadAllText($@"{driveletter}\autoinstall.lst")).Append(Environment.NewLine);

            if (Directory.Exists($@"{driveletter}\SyncMyRide"))
            {
                data.Append(Environment.NewLine);
                DirectoryInfo di = new($@"{driveletter}\SyncMyRide");
                FileInfo[] allFiles = di.GetFiles("*", SearchOption.AllDirectories);
                data.Append($"FILES ({allFiles.Length}){Environment.NewLine}");
                foreach (FileInfo file in allFiles)
                    data.Append($"{file.Name} ({MathHelper.BytesToString(file.Length)}){Environment.NewLine}");
                data.Append(Environment.NewLine);
            }

            data.Append("LOG").Append(Environment.NewLine)
                .Append(log);
            string complete = data.ToString();
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            try
            {
                File.WriteAllText($@"{driveletter}\log.txt", complete);
                File.WriteAllText($@"{AppMan.App.MainSettings.LogPath}log-{currentDate}.txt", complete);
            }
            catch (DirectoryNotFoundException)
            {
                //await UIHelper.ShowErrorDialog(e.GetFullMessage()));
            }
            catch (UnauthorizedAccessException)
            {
                //await UIHelper.ShowErrorDialog(e.GetFullMessage()));
            }

            if (upload)
                await UploadLog(complete);
        }

        //TODO GraphQL instead of routing via php script
        private static async Task UploadLog(string log)
        {
            try
            {
                Dictionary<string, string> values = new()
                {
                    { "computername", Environment.MachineName },
                    { "contents", log }
                };
                HttpRequestMessage httpRequestMessage = new()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(Api.LogPost),
                    Headers = {
                        { nameof(HttpRequestHeader.Authorization), $"Bearer {ApiSecret.Token}" },
                    },
                    Content = new FormUrlEncodedContent(values)
                };
                HttpResponseMessage response = await AppMan.Client.SendAsync(httpRequestMessage);
                string responseString = await response.Content.ReadAsStringAsync();           
                var output = JsonConvert.DeserializeAnonymousType(responseString, new { uuid = "", status = "" });
                if (output != null) Process.Start(Api.LogUrl + output.uuid);
            }
            catch (Exception)
            {
                //await UIHelper.ShowErrorDialog(e.GetFullMessage()));
            }
        }

         public static async Task<Interrogator.LogResult> LogParseXmlAction(string? letter)
         { 
             ApimDetails apimDetails = new();
             string path;
             Interrogator.LogResult logResult = new();
            try
            {
                const string pattern = "*.xml";
                DirectoryInfo dirInfo = new(letter + @"\SyncMyRide\");
                FileInfo file = (from f in dirInfo.GetFiles(pattern) orderby f.LastWriteTime descending select f).First();
                path = file.FullName;
            }
            catch (Exception)
            {
                OpenFileDialog dialog = new();
                dialog.Filters.Add(new FileDialogFilter() { Name = "Interrogator Log XML Files", Extensions = { "xml" } });
                IClassicDesktopStyleApplicationLifetime desktop = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
                string[]? result = await dialog.ShowAsync(desktop.MainWindow);
                if (result == null)
                {
                    AppMan.App.Cancelled = true;
                    return new Interrogator.LogResult();
                }
                path = result.FirstOrDefault() ?? string.Empty;
            }

            try
            {
                if (path == string.Empty) return new Interrogator.LogResult();
                AppMan.App.Cancelled = false;
                XmlDocument doc = new();
                //TODO: swtich to Async once code moves to dotnet 5+ 
                doc.Load(path);
                string json = JsonConvert.SerializeXmlNode(doc, Formatting.Indented);
                Interrogator.InterrogatorModel interrogatorLog = JsonConvert.DeserializeObject<Interrogator.InterrogatorModel>(json) ?? throw new InvalidOperationException();
                
                apimDetails.Vin = interrogatorLog.POtaModuleSnapShot?.PVin ?? string.Empty;
                logResult.Vin = apimDetails.Vin;

                Interrogator.D2P1Did[] d2P1Did = interrogatorLog.POtaModuleSnapShot?.PNode?.D2P1EcuAcronym?.D2P1State?.D2P1Gateway?.D2P1Did ?? Array.Empty<Interrogator.D2P1Did>();
                string sappname = d2P1Did.Where(x => x.DidType == "Embedded Consumer Operating System Part Number").Select(x => x.D2P1Response).Single() ?? "Unknown";
                logResult.Version += sappname;

                string apimmodel = d2P1Did.Where(x => x.DidType == "ECU Delivery Assembly Number").Select(x => x.D2P1Response).Single() ?? "Unknown";
                logResult.Model += apimmodel;

                GraphQLResponse<My20ModelsRoot> graphQlResponse = await AppMan.App.GraphQlClient.SendQueryAsync<My20ModelsRoot>(GraphQlHelper.GetMy20Models());
                My20ModelsRoot output = graphQlResponse.Data;
                
                foreach (My20Models unused in output.My20Models.Where(my20 => apimmodel.Contains(my20.Model)))
                {
                    AppMan.App.Settings.My20V2 = true;
                }

                if (AppMan.App.Settings.My20V2 != true) AppMan.App.Settings.My20V2 = false;
                Interrogator.D2P1PartitionHealth[]? apimPartions = interrogatorLog.POtaModuleSnapShot?.PNode?.D2P1AdditionalAttributes?.D2P1PartitionHealth;
                string apimsize = apimPartions?.Where(x => x.Type == "/fs/images/").Select(x => x.Total).Single() ?? string.Empty;
                string apimfree = apimPartions?.Where(x => x.Type == "/fs/images/").Select(x => x.Available).Single() ?? string.Empty;
                bool? apimnavpartition = apimPartions?.Any(x => x.Type == "/fs/sd/MAP");
                apimDetails.PartNumber = apimmodel;
                if (double.TryParse(apimsize.Remove(apimsize.Length - 1), NumberStyles.Any, CultureInfo.InvariantCulture, out double apimsizeint))
                {
                    switch (apimsizeint)
                    {
                        case >= 0 and <= 8:
                            apimDetails.Nav = false;
                            apimDetails.Size = 8;
                            break;
                        case >= 9 and <= 16:
                            if(apimnavpartition == true) 
                                apimDetails.Nav = true;
                            apimDetails.Size = 16;
                            break;
                        case >= 17 and <= 32:
                            apimDetails.Nav = true;
                            apimDetails.Size = 32;
                            break;
                        case >= 33 and <= 64:
                            apimDetails.Nav = true;
                            apimDetails.Size = 64;
                            break;
                    }
                }
                
                
                logResult.Navigation = apimDetails.Nav;
                logResult.Storage = apimfree + " / " + apimDetails.Size + "G";
                logResult.Time += interrogatorLog.POtaModuleSnapShot?.PNode?.D2P1AdditionalAttributes?.LogGeneratedDateTime;

                if (interrogatorLog.POtaModuleSnapShot?.PNode?.D2P1AdditionalAttributes?.D2P1PartitionHealth != null)
                    foreach (Interrogator.D2P1PartitionHealth d2P1PartitionHealth in interrogatorLog.POtaModuleSnapShot.PNode.D2P1AdditionalAttributes.D2P1PartitionHealth)
                        logResult.Partitions += $"{d2P1PartitionHealth.Type} = {d2P1PartitionHealth.Available} / {d2P1PartitionHealth.Total}{Environment.NewLine}";

                List<string> packages = SyncHexToAscii.ConvertPackages(d2P1Did.Where(x => x.DidValue == "8060").Select(x => x.D2P1Response).Single() ?? string.Empty);
                List<string> packagescont = SyncHexToAscii.ConvertPackages(d2P1Did.Where(x => x.DidValue == "8061").Select(x => x.D2P1Response).Single() ?? string.Empty); 
                packages.AddRange(packagescont);
                
                logResult.Packages = packages;

                if (logResult.Packages.Any(x => x.Contains("14G421-A")) && logResult.Packages.Any(x => x.Contains("14G422-A")))
                    logResult.Region = "CN";
                else if (logResult.Packages.Any(x => x.Contains("14G421-B")) && logResult.Packages.Any(x => x.Contains("14G422-B")))
                    logResult.Region = "EU";
                else if (logResult.Packages.Any(x => x.Contains("14G421-C")) && logResult.Packages.Any(x => x.Contains("14G422-C")))
                    logResult.Region = "NA";
                else if (logResult.Packages.Any(x => x.Contains("14G421-D")) && logResult.Packages.Any(x => x.Contains("14G422-D")))
                    logResult.Region = "ANZ";
                else if (logResult.Packages.Any(x => x.Contains("14G421-F")) && logResult.Packages.Any(x => x.Contains("14G422-F")))
                    logResult.Region = "ROW";
                else 
                    logResult.Region = "???";

                List<AsBuilt.DID> asBuiltValues = new();

                logResult.AsBuilt += "APIM AsBuilt";
                foreach (Interrogator.D2P1Did d2P1Didchild in d2P1Did.Where(x => x.DidType != null && x.DidType.Contains("Direct Configuraation DID DE")))
                {
                    if (d2P1Didchild.D2P1Response == null || d2P1Didchild.DidValue == null) continue;
                    if (d2P1Didchild.DidValue == "DE01" && logResult.Region is "" or "???")
                    {
                        logResult.AsBuiltRegion = SyncHexToAscii.ConvertPackages(d2P1Didchild.D2P1Response[..4]).FirstOrDefault() ?? "???";
                        GraphQLResponse<CountriesRoot> graphQlResponse2 = await AppMan.App.GraphQlClient.SendQueryAsync<CountriesRoot>(GraphQlHelper.GetRegionFromCountry(logResult.AsBuiltRegion));
                        logResult.Region = graphQlResponse2.Data.Countries.FirstOrDefault().Region;
                    }
                    
                    logResult.AsBuilt += $"{Environment.NewLine}{d2P1Didchild.DidValue}: {d2P1Didchild.D2P1Response.ToUpper()}";
                    asBuiltValues.Add(new AsBuilt.DID { ID = d2P1Didchild.DidValue, Text = d2P1Didchild.D2P1Response.ToUpper() });
                }

                
                try
                {
                    if (sappname != "Unknown")
                    {
                        var graphQlResponse2 = await AppMan.App.GraphQlClient.SendQueryAsync<IvsuRoot>(GraphQlHelper.IvsuVersionLookup(sappname));
                        IvsuRoot sversion = graphQlResponse2.Data;

                        if (sversion.Ivsus != null)
                        {
                            string convertedsversion = sversion.Ivsus[0].Version;
                            AppMan.App.Settings.CurrentVersion = Convert.ToInt32(sversion.Ivsus[0].Version.Replace(".", ""));
                            logResult.Version = convertedsversion;
                        }
                    }
                }
                catch (Exception)
                {
                    //likely no internet connection ignore
                }

                //TODO AsBuilt again!
                
            }
            catch (NullReferenceException)
            {
                //await UIHelper.ShowErrorDialog(LM.GetValue("MessageBox.LogUtilityInvalidFile")));
            }
            catch (XmlException)
            {
                //await UIHelper.ShowErrorDialog(LM.GetValue("MessageBox.LogUtilityInvalidFile")));
            }
            catch (InvalidOperationException)
            {
                //await UIHelper.ShowErrorDialog(LM.GetValue("MessageBox.LogUtilityInvalidFile")));
            }
            return logResult;
         }
    }
}