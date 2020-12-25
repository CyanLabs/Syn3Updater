using System.Collections.ObjectModel;
using System.Windows;
using Syn3Updater.Helper;
using Syn3Updater.Model;

namespace Syn3Updater.UI.Tabs
{
    internal class UtilityViewModel : LanguageAwareBaseViewModel
    {
        #region Constructors

        private ActionCommand _refreshUSB;
        public ActionCommand RefreshUSB => _refreshUSB ?? (_refreshUSB = new ActionCommand(RefreshUsbAction));
        
        private ActionCommand _logPrepareUSB;
        public ActionCommand LogPrepareUSB => _logPrepareUSB ?? (_logPrepareUSB = new ActionCommand(LogPrepareUSBAction));

        private ActionCommand _logParseXml;
        public ActionCommand LogParseXml => _logParseXml ?? (_logParseXml = new ActionCommand(LogParseXmlAction));

        #endregion

        #region Properties & Fields
        private string _driveLetter;

        public string DriveLetter
        {
            get => _driveLetter;
            set => SetProperty(ref _driveLetter, value);
        }

        private string _driveName;

        public string DriveName
        {
            get => _driveName;
            set => SetProperty(ref _driveName, value);
        }

        private string _driveFileSystem;

        public string DriveFileSystem
        {
            get => _driveFileSystem;
            set => SetProperty(ref _driveFileSystem, value);
        }       
        
        private string _logXMLDetails;

        public string LogXMLDetails
        {
            get => _logXMLDetails;
            set => SetProperty(ref _logXMLDetails, value);
        }

        private Visibility _toggleLogXMLDetails;

        public Visibility ToggleLogXMLDetails
        {
            get => _toggleLogXMLDetails;
            set => SetProperty(ref _toggleLogXMLDetails, value);
        }

        private USBHelper.Drive _selectedDrive;

        public USBHelper.Drive SelectedDrive
        {
            get => _selectedDrive;
            set
            {
                SetProperty(ref _selectedDrive, value);
                UpdateDriveInfo();
            }
        }

        private ObservableCollection<USBHelper.Drive> _driveList;

        public ObservableCollection<USBHelper.Drive> DriveList
        {
            get => _driveList;
            set => SetProperty(ref _driveList, value);
        }

        #endregion

        #region Methods

        public void ReloadSettings()
        {
        }

        public void Init()
        {
            RefreshUsbAction();
        }

        private void RefreshUsbAction()
        {
            DriveList = USBHelper.refresh_devices(false);
        }

        private void UpdateDriveInfo()
        {
            USBHelper.DriveInfo driveInfo = USBHelper.UpdateDriveInfo(SelectedDrive);

            // Update app level vars
            ApplicationManager.Instance.DriveFileSystem = driveInfo.FileSystem;
            ApplicationManager.Instance.DrivePartitionType = driveInfo.PartitionType;
            ApplicationManager.Instance.DriveName = driveInfo.Name;
            ApplicationManager.Instance.SkipFormat = driveInfo.SkipFormat;

            // Update local level vars
            DriveLetter = driveInfo.Letter;
            DriveFileSystem = driveInfo.PartitionType + " " + driveInfo.FileSystem;
            DriveName = driveInfo.Name;

            ApplicationManager.Logger.Info(
                $"[Utility] USB Drive selected - Name: {driveInfo.Name} - FileSystem: {driveInfo.FileSystem} - PartitionType: {driveInfo.PartitionType} - Letter: {driveInfo.Letter}");
        }
        private void LogParseXmlAction()
        {
            throw new System.NotImplementedException();
        }

        private void LogPrepareUSBAction()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}