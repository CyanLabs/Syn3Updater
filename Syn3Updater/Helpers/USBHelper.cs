using System;
using System.Collections.ObjectModel;
using Syn3Updater.Models;
using System.Management;
using System.Threading.Tasks;
using Cyanlabs.Syn3Updater.Model;


namespace Syn3Updater.Helpers
{
    public class USBHelper
    {
        /// <summary>
        ///     Refreshes device list using WMI queries
        /// </summary>
        /// <param name="fakeusb">Set to true to add 'Download Only' option to the list</param>
        /// <returns>ObservableCollection of all USB Drives as type Drive</returns>
        public static ObservableCollection<USBDriveModel.Drive> RefreshDevicesWindows(bool fakeusb)
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

                        drive.Letter = ld.GetPropertyValue("DeviceId").ToString();
                        drive.PartitionType = p.GetPropertyValue("Type").ToString()!.Contains("GPT:") ? "GPT" : "MBR";
                        
                        drive.Name = d.GetPropertyValue("Caption").ToString();
                        
                        drive.Path = d.Path.RelativePath;
                        drive.FreeSpace = MathHelper.BytesToString(Convert.ToInt64(ld.GetPropertyValue("FreeSpace")));
                        drive.Model = d.GetPropertyValue("Model").ToString();
                        drive.Size = friendlySize;
                        drive.Fake = false;
                        if (drive.FileSystem == "exFAT" && drive.PartitionType == "MBR" && drive.Name == "CYANLABS")
                            drive.SkipFormat = true;
                        else
                            drive.SkipFormat = false;
                    }
                }

                // Add to array of drives
                driveList.Add(drive);
            }

            if (fakeusb)
                driveList.Add(new USBDriveModel.Drive { Path = "", Name = "Select A Directory", Fake = true });

            // Return a list of drives
            return driveList;
        }
        
        public static async Task LogPrepareUSBAction(USBDriveModel.Drive selectedDrive, string driveLetter, string currentversion, string action = "logutility")
        {
            //Reset ApplicationManager variables
            
            //AppMan.App.Ivsus.Clear();
            //AppMan.App.DriveLetter = driveLetter;
            //AppMan.App.Action = action;
            //AppMan.App.SelectedRelease = "Interrogator Log Utility";
            
            if (currentversion.StartsWith("3.4"))
                Api.InterrogatorTool = await ApiHelper.GetSpecialIvsu(Api.SpecialPackages.LogTool34);
            else if (currentversion.StartsWith("3.2") || currentversion.StartsWith("3.3"))
                Api.InterrogatorTool = await ApiHelper.GetSpecialIvsu(Api.SpecialPackages.LogTool32);
            else if (currentversion.StartsWith("3.0") || currentversion.StartsWith("2.") || currentversion.StartsWith("1."))
                Api.InterrogatorTool = await ApiHelper.GetSpecialIvsu(Api.SpecialPackages.LogTool30);
            else
                Api.InterrogatorTool = await ApiHelper.GetSpecialIvsu(Api.SpecialPackages.LogTool34);

            //AppMan.App.Ivsus.Add(Api.InterrogatorTool);
            //AppMan.App.InstallMode = "autoinstall";

            // if (await SanityCheckHelper.CancelDownloadCheck(selectedDrive) || Api.InterrogatorTool == null)
            //     return;

            // AppMan.App.IsDownloading = true;
            // AppMan.Logger.Info("Starting process (Logging Utility");
            // AppMan.App.FireDownloadsTabEvent();
        }

    }
}