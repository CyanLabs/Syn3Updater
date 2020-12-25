using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Management;
using System.Reflection;
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
            driveList.Add(new Drive { Path = "", Name = fakeusb ? LanguageManager.GetValue("Home.NoUSB") : ""});
            ManagementObjectSearcher driveQuery = new ManagementObjectSearcher("select * from Win32_DiskDrive WHERE InterfaceType='USB'");
            foreach (ManagementBaseObject o in driveQuery.Get())
            {
                ManagementObject d = (ManagementObject) o;
                string diskName = Convert.ToString(d.Properties["Caption"].Value);
                string friendlySize = MathHelper.BytesToString(Convert.ToInt64(d.Properties["Size"].Value));
                if (friendlySize != "0B")
                    // Add to array of drives
                    driveList.Add(new Drive {Path = d.Path.RelativePath, Name = $"{diskName} {friendlySize}"});
            }

            // Return a list of drives
            return driveList;
        }

        public static DriveInfo UpdateDriveInfo(Drive selectedDrive)
        {
            DriveInfo driveInfo = new DriveInfo();
            if (selectedDrive == null || selectedDrive.Name == LanguageManager.GetValue("Home.NoUSB")) return driveInfo;

            string partitionQueryText = $@"associators of {{{selectedDrive.Path}}} where AssocClass = Win32_DiskDriveToDiskPartition";
            ManagementObjectSearcher partitionQuery = new ManagementObjectSearcher(partitionQueryText);
            try
            {
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
            }
            catch (ManagementException)
            {
                //TODO Implement Catch
            }

            return driveInfo;
        }

        public static void GenerateLog(string log)
        {
            string data = $@"CYANLABS - SYN3 UPDATER - V{Assembly.GetExecutingAssembly().GetName().Version}{Environment.NewLine}";
            data += $@"Operating System: {SystemHelper.GetOsFriendlyName()}{Environment.NewLine}";
            data += Environment.NewLine;
            data += $@"PREVIOUS CONFIGURATION{Environment.NewLine}";
            data += $@"Version: {ApplicationManager.Instance.SyncVersion}{Environment.NewLine}";
            data += $@"Region: {Settings.Default.CurrentSyncRegion}{Environment.NewLine}";
            data += $@"Navigation: {Settings.Default.CurrentSyncNav}{Environment.NewLine}";
            data +=
                $@"Mode: {(Settings.Default.CurrentInstallMode == @"autodetect" ? ApplicationManager.Instance.InstallMode : $"{Settings.Default.CurrentInstallMode} FORCED")}{Environment.NewLine}";
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
        }

        #endregion
    }
}