using System;
using System.Collections.ObjectModel;
using System.Management;
using Syn3Updater.Model;
using Syn3Updater.UI.Tabs;

namespace Syn3Updater.Helper
{
    public class USBHelper
    {
        public static ObservableCollection<HomeViewModel.Drive> refresh_devices()
        {
            ObservableCollection<HomeViewModel.Drive> DriveList = new ObservableCollection<HomeViewModel.Drive>
            {
                new HomeViewModel.Drive {Path = "", Name = LanguageManager.GetValue("Home.NoUSB")}
            };
            ManagementObjectSearcher driveQuery = new ManagementObjectSearcher("select * from Win32_DiskDrive WHERE InterfaceType='USB'");
            foreach (ManagementBaseObject o in driveQuery.Get())
            {
                ManagementObject d = (ManagementObject) o;
                string diskName = Convert.ToString(d.Properties["Caption"].Value);
                string friendlySize = MathHelper.BytesToString(Convert.ToInt64(d.Properties["Size"].Value));
                if (friendlySize != "0B")
                    // Add to array of drives
                    DriveList.Add(new HomeViewModel.Drive {Path = d.Path.RelativePath, Name = $"{diskName} {friendlySize}"});
            }

            // Return a list of drives
            return DriveList;
        }

        public static DriveInfo UpdateDriveInfo(HomeViewModel.Drive SelectedDrive)
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

        public struct DriveInfo
        {
            public string Letter;
            public string Name;
            public string FileSystem;
            public string PartitionType;
            public bool SkipFormat;
        }
    }
}