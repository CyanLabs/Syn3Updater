using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net.Http;
using System.Reflection;
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
            ObservableCollection<Drive> driveList = new ObservableCollection<Drive>();
            driveList.Add(fakeusb ? new Drive {Path = "", Name = LanguageManager.GetValue("Home.NoUSB")} : new Drive {Path = "", Name = ""});
            ManagementObjectSearcher driveQuery = new ManagementObjectSearcher("select * from Win32_DiskDrive Where InterfaceType = \"USB\" OR MediaType = \"External hard disk media\"");
            foreach (ManagementBaseObject o in driveQuery.Get())
            {
                ManagementObject d = (ManagementObject) o;
                string diskName = Convert.ToString(d.Properties["Caption"].Value);
                string friendlySize = MathHelper.BytesToString(Convert.ToInt64(d.Properties["Size"].Value));
                if (friendlySize != "0B")
                    // Add to array of drives
                    driveList.Add(new Drive {Path = d.Path.RelativePath, Name = $"{diskName} {friendlySize}"});
            }
            if (fakeusb) driveList.Add(new Drive { Path = "", Name = LanguageManager.GetValue("Home.NoUSBDir") });

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
            if (selectedDrive == null || selectedDrive.Name == LanguageManager.GetValue("Home.NoUSB") || selectedDrive.Name == LanguageManager.GetValue("Home.NoUSBDir") || selectedDrive.Path == "") return driveInfo;

            string partitionQueryText = $@"associators of {{{selectedDrive.Path}}} where AssocClass = Win32_DiskDriveToDiskPartition";
            ManagementObjectSearcher partitionQuery = new ManagementObjectSearcher(partitionQueryText);
            foreach (ManagementBaseObject o in partitionQuery.Get())
            {
                ManagementObject p = (ManagementObject) o;
                string logicalDriveQueryText = $@"associators of {{{p.Path.RelativePath}}} where AssocClass = Win32_LogicalDiskToPartition";
                ManagementObjectSearcher logicalDriveQuery = new ManagementObjectSearcher(logicalDriveQueryText);
                foreach (ManagementBaseObject managementBaseObject in logicalDriveQuery.Get())
                {
                    ManagementObject ld = (ManagementObject) managementBaseObject;
                    driveInfo.Letter = Convert.ToString(ld.Properties["DeviceId"].Value);
                    driveInfo.PartitionType = p.Properties["Type"].Value.ToString().Contains("GPT:") ? "GPT" : "MBR";
                    driveInfo.FileSystem += Convert.ToString(ld.Properties["FileSystem"].Value);
                    driveInfo.Name = ld.Properties["VolumeName"].Value.ToString();

                    if (driveInfo.FileSystem == "exFAT" && driveInfo.PartitionType == "MBR" && driveInfo.Name == "CYANLABS") driveInfo.SkipFormat = true;
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
            string data = $@"CYANLABS - SYN3 UPDATER - V{Assembly.GetExecutingAssembly().GetName().Version}{Environment.NewLine}";
            data += $@"Branch: {ApplicationManager.Instance.LauncherPrefs.ReleaseTypeInstalled}{Environment.NewLine}";
            data += $@"Operating System: {SystemHelper.GetOsFriendlyName()}{Environment.NewLine}";
            data += Environment.NewLine;
            data += $@"PREVIOUS CONFIGURATION{Environment.NewLine}";
            data += $@"Version: {ApplicationManager.Instance.SVersion}{Environment.NewLine}";
            data += $@"Region: {ApplicationManager.Instance.Settings.CurrentRegion}{Environment.NewLine}";
            data += $@"Navigation: {ApplicationManager.Instance.Settings.CurrentNav}{Environment.NewLine}";
            data +=
                $@"Mode: {(ApplicationManager.Instance.Settings.CurrentInstallMode == @"autodetect" ? ApplicationManager.Instance.InstallMode : $"{ApplicationManager.Instance.Settings.CurrentInstallMode} FORCED")}{Environment.NewLine}";
            data += Environment.NewLine;
            data += $@"DESTINATION DETAILS{Environment.NewLine}";
            if (ApplicationManager.Instance.DownloadToFolder)
            {
                data += $@"Mode: Directory{Environment.NewLine}";
                data += $@"Path: {ApplicationManager.Instance.DriveLetter}{Environment.NewLine}";
            }
            else
            {
                data += $@"Mode: Drive{Environment.NewLine}";
                data += $@"Model: {ApplicationManager.Instance.DriveName}{Environment.NewLine}";
                data += $@"FileSystem: {ApplicationManager.Instance.DriveFileSystem}{Environment.NewLine}";
                data += $@"Partition Type: {ApplicationManager.Instance.DrivePartitionType}{Environment.NewLine}";
            }


            string driveletter = ApplicationManager.Instance.DriveLetter;
            if (File.Exists($@"{driveletter}\reformat.lst"))
            {
                data += Environment.NewLine;
                data += $@"REFORMAT.LST{Environment.NewLine}";
                data += File.ReadAllText($@"{driveletter}\reformat.lst") + Environment.NewLine;
            }

            if (File.Exists($@"{driveletter}\autoinstall.lst"))
            {
                data += Environment.NewLine;
                data += $@"AUTOINSTALL.LST{Environment.NewLine}";
                data += File.ReadAllText($@"{driveletter}\autoinstall.lst") + Environment.NewLine;
            }

            if (Directory.Exists($@"{driveletter}\SyncMyRide"))
            {
                data += Environment.NewLine;
                DirectoryInfo di = new DirectoryInfo($@"{driveletter}\SyncMyRide");
                FileInfo[] allFiles = di.GetFiles("*", SearchOption.AllDirectories);
                data += $@"FILES ({allFiles.Length}){Environment.NewLine}";
                foreach (FileInfo file in allFiles)
                    data += $"{file.Name} ({MathHelper.BytesToString(file.Length)}){Environment.NewLine}";
                data += Environment.NewLine;
            }

            data += $@"LOG{Environment.NewLine}";
            data += log;
            File.WriteAllText($@"{driveletter}\log.txt", data);

            if (upload) UploadLog(data);
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

            FormUrlEncodedContent content = new FormUrlEncodedContent(values);
            HttpResponseMessage response = ApplicationManager.Instance.Client.PostAsync(Api.LogPost, content).Result;

            string responseString = response.Content.ReadAsStringAsync().Result;
            var definition = new {uuid = "", status = ""};
            var output = JsonConvert.DeserializeAnonymousType(responseString, definition);
            Process.Start(Api.LogUrl + output.uuid);
        }
        #endregion
    }
}