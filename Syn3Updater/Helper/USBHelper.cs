using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Cyanlabs.Syn3Updater.Model;
using Newtonsoft.Json;

namespace Cyanlabs.Syn3Updater.Helper
{
    /// <summary>
    ///     Helper class for USB related methods such as reading disk information via WMI
    /// </summary>
    public static class USBHelper
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
        #endregion
    }
}