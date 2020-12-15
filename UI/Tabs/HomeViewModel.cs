using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using Microsoft.VisualBasic.FileIO;
using Syn3Updater.Helper;
using Syn3Updater.Model;
using System.Management;
using Syn3Updater.Helpers;

namespace Syn3Updater.UI.Tabs
{
    public class HomeViewModel : LanguageAwareBaseViewModel
    {
        public void Init()
        {
            RefreshUsb();
        }
        private ActionCommand _regionInfo;
        public ActionCommand RegionInfo => _regionInfo ?? (_regionInfo = new ActionCommand(RegionInfoAction));

        private void RegionInfoAction()
        {
            if (Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "en")
            {
                Process.Start("https://cyanlabs.net/api/Syn3Updater/region.php");
            }
            else
            {
                Process.Start(
                    "https://translate.google.co.uk/translate?hl=&sl=en&tl={Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName}&u=https%3A%2F%2Fcyanlabs.net%2Fapi%2FSyn3Updater%2Fregion.php");
            }
        }

        private void RefreshUsb()
        {
            DriveList = new ObservableCollection<Drive>();
            DriveList.Add(new Drive { Path = "", Name = ""});
            //DriveList?.Clear();
            ManagementObjectSearcher driveQuery =
                new ManagementObjectSearcher("select * from Win32_DiskDrive WHERE InterfaceType='USB'");
            foreach (ManagementBaseObject o in driveQuery.Get())
            {
                ManagementObject d = (ManagementObject)o;
                string diskName = Convert.ToString(d.Properties["Caption"].Value);
                string friendlySize = Functions.BytesToString(Convert.ToInt64(d.Properties["Size"].Value));
                if (friendlySize != "0B")
                {
                    DriveList.Add(new Drive { Path = d.Path.RelativePath, Name = diskName + " " + friendlySize});
                }
            }
            OnPropertyChanged("DriveList");
        }

        private Drive _selectedDrive;

        public Drive SelectedDrive
        {
            get => _selectedDrive;
            set
            {
                if (value.Path != null)
                {
                    SetProperty(ref _selectedDrive, value);
                    UpdateDriveInfo();
                }
            }
        }

        private void UpdateDriveInfo()
        {
            string partitionQueryText = $@"associators of {{{SelectedDrive.Path}}} where AssocClass = Win32_DiskDriveToDiskPartition";
            ManagementObjectSearcher partitionQuery = new ManagementObjectSearcher(partitionQueryText);
            try
            {
                foreach (ManagementBaseObject o in partitionQuery.Get())
                {
                    ManagementObject p = (ManagementObject)o;
                    string logicalDriveQueryText = $@"associators of {{{p.Path.RelativePath}}} where AssocClass = Win32_LogicalDiskToPartition";
                    ManagementObjectSearcher logicalDriveQuery = new ManagementObjectSearcher(logicalDriveQueryText);
                    foreach (ManagementBaseObject managementBaseObject in logicalDriveQuery.Get())
                    {
                        ManagementObject ld = (ManagementObject)managementBaseObject;
                        DriveLetter = Convert.ToString(ld.Properties["DeviceId"].Value);
                        DriveFileSystem = p.Properties["Type"].Value.ToString().Contains("GPT:") ? "GPT" : "MBR";
                        DriveFileSystem += " " + Convert.ToString(ld.Properties["FileSystem"].Value);
                        DriveName = Convert.ToString(ld.Properties["VolumeName"].Value);
                    }
                }
            }
            catch (ManagementException)
            {
                //TODO Implement Catch
            }
            OnPropertyChanged("DriveLetter");
            OnPropertyChanged("DriveName");
            OnPropertyChanged("DriveFileSystem");
        }


        public string DriveLetter { get; set; }

        public string DriveName { get; set; }
        public string DriveFileSystem { get; set; }

        public ObservableCollection<Drive> DriveList { get; set; }
        public class Drive
        {
            public string Path { get; set; }
            public string Name { get; set; }
        }
    }
}