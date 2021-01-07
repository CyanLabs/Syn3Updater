using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net.Http;
using System.Reflection;
using Newtonsoft.Json;
using Syn3Updater.Model;
using Syn3Updater.Properties;

namespace Syn3Updater.Helper
{
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

        public static ObservableCollection<Drive> refresh_devices(bool fakeusb)
        {
            ObservableCollection<Drive> driveList = new ObservableCollection<Drive>();
            if (fakeusb)
            {
                driveList.Add(new Drive { Path = "", Name = LanguageManager.GetValue("Home.NoUSB")});
            }
            else
            {
                driveList.Add(new Drive { Path = "", Name = "" });
            }
            ManagementObjectSearcher driveQuery = new ManagementObjectSearcher("select * from Win32_DiskDrive");
            foreach (ManagementBaseObject o in driveQuery.Get())
            {
                if (o.Properties["InterfaceType"].Value?.ToString() == "USB" || o.Properties["MediaType"].Value?.ToString() == "External hard disk media") {
                    ManagementObject d = (ManagementObject)o;
                    string diskName = Convert.ToString(d.Properties["Caption"].Value);
                    string friendlySize = MathHelper.BytesToString(Convert.ToInt64(d.Properties["Size"].Value));
                    if (friendlySize != "0B")
                        // Add to array of drives
                        driveList.Add(new Drive {Path = d.Path.RelativePath, Name = $"{diskName} {friendlySize}"});
                }
            }

            // Return a list of drives
            return driveList;
        }

        public static DriveInfo UpdateDriveInfo(Drive selectedDrive)
        {
            DriveInfo driveInfo = new DriveInfo();
            if (selectedDrive == null || selectedDrive.Name == LanguageManager.GetValue("Home.NoUSB") || selectedDrive.Path == "") return driveInfo;

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

        public static void GenerateLog(string log,bool upload)
        {
            string data = $@"CYANLABS - SYN3 UPDATER - V{Assembly.GetExecutingAssembly().GetName().Version}{Environment.NewLine}";
            data += $@"Operating System: {SystemHelper.GetOsFriendlyName()}{Environment.NewLine}";
            data += Environment.NewLine;
            data += $@"PREVIOUS CONFIGURATION{Environment.NewLine}";
            data += $@"Version: {ApplicationManager.Instance.SyncVersion}{Environment.NewLine}";
            data += $@"Region: {ApplicationManager.Instance.Settings.CurrentSyncRegion}{Environment.NewLine}";
            data += $@"Navigation: {ApplicationManager.Instance.Settings.CurrentSyncNav}{Environment.NewLine}";
            data +=
                $@"Mode: {(ApplicationManager.Instance.Settings.CurrentInstallMode == @"autodetect" ? ApplicationManager.Instance.InstallMode : $"{ApplicationManager.Instance.Settings.CurrentInstallMode} FORCED")}{Environment.NewLine}";
            data += Environment.NewLine;
            data += $@"USB DETAILS{Environment.NewLine}";
            data += $@"Model: {ApplicationManager.Instance.DriveName}{Environment.NewLine}";
            data += $@"FileSystem: {ApplicationManager.Instance.DriveFileSystem}{Environment.NewLine}";
            data += $@"Partition Type: {ApplicationManager.Instance.DrivePartitionType}{Environment.NewLine}";

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
                data += $@"SYNCMYRIDE FILES ({allFiles.Length}){Environment.NewLine}";
                foreach (FileInfo file in allFiles)
                    data += $"{file.Name} ({MathHelper.BytesToString(file.Length)}){Environment.NewLine}";
                data += Environment.NewLine;
            }

            data += $@"LOG{Environment.NewLine}";
            data += log;
            File.WriteAllText($@"{driveletter}\log.txt", data);

            if (upload) UploadLog(data);
        }

        public static void UploadLog(string log)
        {
            var values = new Dictionary<string, string>
            {
                {"contents", log}
            };

            var content = new FormUrlEncodedContent(values);
            HttpClient client = new HttpClient();
            var response = client.PostAsync(Api.LogPost, content).Result;

            var responseString = response.Content.ReadAsStringAsync().Result;
            var definition = new { uuid = "", status = "" };
            var output = JsonConvert.DeserializeAnonymousType(responseString, definition);
            Process.Start(Api.LogURL + output.uuid);
        }
        
        #endregion
    }
}