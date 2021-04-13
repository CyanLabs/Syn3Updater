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
                if (ModernWpf.MessageBox.Show(LM.GetValue("MessageBox.My20Check"), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    await USBHelper.LogPrepareUSBAction(SelectedDrive, DriveLetter, "logutilitymy20");
                    return;
                }
            }

            //Warn is users region is different to new selection
            if (SelectedRegion.Code != AppMan.App.Settings.CurrentRegion)
            {
                if (ModernWpf.MessageBox.Show(LM.GetValue("MessageBox.CancelRegionMismatch"), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning) !=
                    MessageBoxResult.Yes)
                {
                    canceldownload = true;
                }
            }

            //Cancel no apps package selected
            if (!AppMan.App.AppsSelected && (InstallMode == "reformat" || InstallMode == "downgrade"))
            {
                ModernWpf.MessageBox.Show(LM.GetValue("MessageBox.CancelNoApps"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                canceldownload = true;
            }

            if (!canceldownload && !SanityCheckHelper.CancelDownloadCheck(SelectedDrive))
            {
                if (!AppMan.App.DownloadOnly)
                {
                    AppMan.App.DriveNumber = SelectedDrive.Path.Replace("Win32_DiskDrive.DeviceID=\"\\\\\\\\.\\\\PHYSICALDRIVE", "").Replace("\"", "");
                    AppMan.App.DriveLetter = DriveLetter;
                }
                AppMan.App.IsDownloading = true;
                AppMan.Logger.Info($"Starting process ({SelectedRelease} - {SelectedRegion.Code} - {SelectedMapVersion})");
                AppMan.App.FireDownloadsTabEvent();
            }
        }

        public async static Task SetIvsuList(string InstallMode, ObservableCollection<SModel.Ivsu> IvsuList, SModel.SRegion SelectedRegion, string SelectedRelease, string SelectedMapVersion, string DriveLetter)
        {
            AppMan.Logger.Info(
                $"USB Drive selected - Name: {AppMan.App.DriveName} - FileSystem: {AppMan.App.DriveFileSystem} - PartitionType: {AppMan.App.DrivePartitionType} - Letter: {DriveLetter}");
            AppMan.App.Ivsus.Clear();

            if (InstallMode == "downgrade")
            {
                Api.DowngradeApp = await ApiHelper.GetSpecialIvsu(Api.GetDowngradeApp);
                AppMan.App.Ivsus.Add(Api.DowngradeApp);

                Api.DowngradeTool = await ApiHelper.GetSpecialIvsu(Api.GetDowngradeTool);
                AppMan.App.Ivsus.Add(Api.DowngradeTool);
            }

            if (InstallMode == "reformat" || InstallMode == "downgrade")
            {
                Api.ReformatTool = await ApiHelper.GetSpecialIvsu(Api.GetReformat);
                AppMan.App.Ivsus.Add(Api.ReformatTool);
            }

            AppMan.App.DownloadOnly = false;
            AppMan.App.DriveLetter = DriveLetter;
            foreach (SModel.Ivsu item in IvsuList)
            {
                if (item.Selected)
                {
                    if (item.Type == "APPS")
                        AppMan.App.AppsSelected = true;

                    AppMan.App.Ivsus.Add(item);
                }
            }

            AppMan.App.SelectedRegion = SelectedRegion.Code;
            AppMan.App.SelectedRelease = SelectedRelease;
            AppMan.App.SelectedMapVersion = SelectedMapVersion;
        }
    }
}
