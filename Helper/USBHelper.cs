using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Management;
using System.Reflection;
using Syn3Updater.Model;

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
        public static ObservableCollection<Drive> refresh_devices()
        {
            ObservableCollection<Drive> DriveList = new ObservableCollection<Drive>
            {
                new Drive {Path = "", Name = LanguageManager.GetValue("Home.NoUSB")}
            };
            ManagementObjectSearcher driveQuery = new ManagementObjectSearcher("select * from Win32_DiskDrive WHERE InterfaceType='USB'");
            foreach (ManagementBaseObject o in driveQuery.Get())
            {
                ManagementObject d = (ManagementObject) o;
                string diskName = Convert.ToString(d.Properties["Caption"].Value);
                string friendlySize = MathHelper.BytesToString(Convert.ToInt64(d.Properties["Size"].Value));
                if (friendlySize != "0B")
                    // Add to array of drives
                    DriveList.Add(new Drive { Path = d.Path.RelativePath, Name = $"{diskName} {friendlySize}"});
            }

            // Return a list of drives
            return DriveList;
        }

        public static DriveInfo UpdateDriveInfo(Drive SelectedDrive)
        {
            DriveInfo drive_info = new DriveInfo();
            if (SelectedDrive == null || SelectedDrive.Name == LanguageManager.GetValue("Home.NoUSB")) return drive_info;

            string partitionQueryText = $@"associators of {{{SelectedDrive.Path}}} where AssocClass = Win32_DiskDriveToDiskPartition";
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
                        drive_info.Letter = Convert.ToString(ld.Properties["DeviceId"].Value);
                        drive_info.PartitionType = p.Properties["Type"].Value.ToString().Contains("GPT:") ? "GPT" : "MBR";
                        drive_info.FileSystem += Convert.ToString(ld.Properties["FileSystem"].Value);
                        drive_info.Name = ld.Properties["VolumeName"].Value.ToString();

                        if (drive_info.FileSystem == "exFAT" && drive_info.PartitionType == "MBR" && drive_info.Name == "CYANLABS") drive_info.SkipFormat = true;
                    }
                }
            }
            catch (ManagementException)
            {
                //TODO Implement Catch
            }

            return drive_info;
        }

        public static void GenerateLog(string log)
        {
            string data = $@"CYANLABS - SYN3 UPDATER - V{Assembly.GetExecutingAssembly().GetName().Version}{Environment.NewLine}";
            data += $@"Operating System: {SystemHelper.GetOsFriendlyName()}{Environment.NewLine}";
            data += Environment.NewLine;
            data += $@"PREVIOUS CONFIGURATION{Environment.NewLine}";
            data += $@"Version: {ApplicationManager.Instance.SyncVersion}{Environment.NewLine}";
            data += $@"Region: {Properties.Settings.Default.CurrentSyncRegion}{Environment.NewLine}";
            data += $@"Navigation: {Properties.Settings.Default.CurrentSyncNav}{Environment.NewLine}";
            data +=
                $@"Mode: {(Properties.Settings.Default.CurrentInstallMode == @"autodetect" ? ApplicationManager.Instance.InstallMode : $"{Properties.Settings.Default.CurrentInstallMode} FORCED")}{Environment.NewLine}";
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