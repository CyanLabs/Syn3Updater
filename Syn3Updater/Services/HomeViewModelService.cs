using Cyanlabs.Syn3Updater;
using Cyanlabs.Syn3Updater.Helper;
using Cyanlabs.Syn3Updater.Model;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace Cyanlabs.Updater.Common
{
    public static class HomeViewModelService
    {
        public async static Task Download(string InstallMode, ObservableCollection<SModel.Ivsu> IvsuList,
            SModel.SRegion SelectedRegion, string SelectedRelease, string SelectedMapVersion, string DriveLetter, USBHelper.Drive SelectedDrive)
        {
            await SetIvsuList(InstallMode, IvsuList, SelectedRegion, SelectedRelease, SelectedMapVersion, DriveLetter);

            bool canceldownload = false;
            //Install Mode is reformat or downgrade My20 warning
            if (InstallMode == "reformat" || InstallMode == "downgrade")
            {
                if (ModernWpf.MessageBox.Show(string.Format(LanguageManager.GetValue("MessageBox.CancelMy20"), InstallMode), "Syn3 Updater", MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    if (ModernWpf.MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelMy20Final"), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning) !=
                        MessageBoxResult.Yes)
                    {
                        canceldownload = true;
                    }
                }
                else
                {
                    canceldownload = true;
                }
            }

            //Warn is users region is different to new selection
            if (SelectedRegion.Code != ApplicationManager.Instance.Settings.CurrentRegion)
            {
                if (ModernWpf.MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelRegionMismatch"), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning) !=
                    MessageBoxResult.Yes)
                {
                    canceldownload = true;
                }
            }

            //Cancel no apps package selected
            if (!ApplicationManager.Instance.AppsSelected && (InstallMode == "reformat" || InstallMode == "downgrade"))
            {
                ModernWpf.MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelNoApps"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                canceldownload = true;
            }

            if (!canceldownload && !SanityCheckHelper.CancelDownloadCheck(SelectedDrive))
            {
                if (!ApplicationManager.Instance.DownloadOnly)
                {
                    ApplicationManager.Instance.DriveNumber = SelectedDrive.Path.Replace("Win32_DiskDrive.DeviceID=\"\\\\\\\\.\\\\PHYSICALDRIVE", "").Replace("\"", "");
                    ApplicationManager.Instance.DriveLetter = DriveLetter;
                }
                ApplicationManager.Instance.IsDownloading = true;
                ApplicationManager.Logger.Info($"Starting process ({SelectedRelease} - {SelectedRegion.Code} - {SelectedMapVersion})");
                ApplicationManager.Instance.FireDownloadsTabEvent();
            }
        }

        public async static Task SetIvsuList(string InstallMode, ObservableCollection<SModel.Ivsu> IvsuList, SModel.SRegion SelectedRegion, string SelectedRelease, string SelectedMapVersion, string DriveLetter)
        {
            ApplicationManager.Logger.Info(
                $"USB Drive selected - Name: {ApplicationManager.Instance.DriveName} - FileSystem: {ApplicationManager.Instance.DriveFileSystem} - PartitionType: {ApplicationManager.Instance.DrivePartitionType} - Letter: {DriveLetter}");
            ApplicationManager.Instance.Ivsus.Clear();

            if (InstallMode == "downgrade")
            {
                Api.DowngradeApp = await ApiHelper.GetSpecialIvsu(Api.GetDowngradeApp);
                ApplicationManager.Instance.Ivsus.Add(Api.DowngradeApp);

                Api.DowngradeTool = await ApiHelper.GetSpecialIvsu(Api.GetDowngradeTool);
                ApplicationManager.Instance.Ivsus.Add(Api.DowngradeTool);
            }

            if (InstallMode == "reformat" || InstallMode == "downgrade")
            {
                Api.ReformatTool = await ApiHelper.GetSpecialIvsu(Api.GetReformat);
                ApplicationManager.Instance.Ivsus.Add(Api.ReformatTool);
            }

            ApplicationManager.Instance.DownloadOnly = false;
            ApplicationManager.Instance.DriveLetter = DriveLetter;
            foreach (SModel.Ivsu item in IvsuList)
            {
                if (item.Selected)
                {
                    if (item.Type == "APPS")
                        ApplicationManager.Instance.AppsSelected = true;

                    ApplicationManager.Instance.Ivsus.Add(item);
                }
            }

            ApplicationManager.Instance.SelectedRegion = SelectedRegion.Code;
            ApplicationManager.Instance.SelectedRelease = SelectedRelease;
            ApplicationManager.Instance.SelectedMapVersion = SelectedMapVersion;
        }
    }
}
