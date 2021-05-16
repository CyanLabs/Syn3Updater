using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Cyanlabs.Syn3Updater.Helper;
using Cyanlabs.Syn3Updater.Model;

namespace Cyanlabs.Syn3Updater.Services
{
    public static class HomeViewModelService
    {
        public static async Task Download(string installMode, ObservableCollection<SModel.Ivsu> ivsuList,
            SModel.SRegion selectedRegion, string selectedRelease, string selectedMapVersion, string driveLetter, USBHelper.Drive selectedDrive)
        {
            await SetIvsuList(installMode, ivsuList, selectedRegion, selectedRelease, selectedMapVersion, driveLetter);

            bool canceldownload = false;
            //Install Mode is reformat or downgrade My20 warning
            if (installMode == "reformat" || installMode == "downgrade")
            {
                if (ModernWpf.MessageBox.Show(LM.GetValue("MessageBox.My20Check"), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    await USBHelper.LogPrepareUSBAction(selectedDrive, driveLetter, "logutilitymy20");
                    return;
                }
            }

            //Warn is users region is different to new selection
            if (selectedRegion.Code != AppMan.App.Settings.CurrentRegion)
            {
                if (ModernWpf.MessageBox.Show(LM.GetValue("MessageBox.CancelRegionMismatch"), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning) !=
                    MessageBoxResult.Yes)
                {
                    canceldownload = true;
                }
            }

            //Cancel no apps package selected
            if (!AppMan.App.AppsSelected && (installMode == "reformat" || installMode == "downgrade"))
            {
                ModernWpf.MessageBox.Show(LM.GetValue("MessageBox.CancelNoApps"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                canceldownload = true;
            }

            if (!canceldownload && !SanityCheckHelper.CancelDownloadCheck(selectedDrive))
            {
                if (!AppMan.App.DownloadOnly)
                {
                    AppMan.App.DriveNumber = selectedDrive.Path.Replace("Win32_DiskDrive.DeviceID=\"\\\\\\\\.\\\\PHYSICALDRIVE", "").Replace("\"", "");
                    AppMan.App.DriveLetter = driveLetter;
                }
                AppMan.App.IsDownloading = true;
                AppMan.Logger.Info($"Starting process ({selectedRelease} - {selectedRegion.Code} - {selectedMapVersion})");
                AppMan.App.FireDownloadsTabEvent();
            }
        }

        public async static Task SetIvsuList(string installMode, ObservableCollection<SModel.Ivsu> ivsuList, SModel.SRegion selectedRegion, string selectedRelease, string selectedMapVersion, string driveLetter)
        {
            AppMan.Logger.Info(
                $"USB Drive selected - Name: {AppMan.App.DriveName} - FileSystem: {AppMan.App.DriveFileSystem} - PartitionType: {AppMan.App.DrivePartitionType} - Letter: {driveLetter}");
            AppMan.App.Ivsus.Clear();

            if (installMode == "downgrade")
            {
                Api.DowngradeApp = await ApiHelper.GetSpecialIvsu(Api.GetDowngradeApp);
                AppMan.App.Ivsus.Add(Api.DowngradeApp);

                Api.DowngradeTool = await ApiHelper.GetSpecialIvsu(Api.GetDowngradeTool);
                AppMan.App.Ivsus.Add(Api.DowngradeTool);
            }

            if (installMode == "reformat" || installMode == "downgrade")
            {
                Api.ReformatTool = await ApiHelper.GetSpecialIvsu(Api.GetReformat);
                AppMan.App.Ivsus.Add(Api.ReformatTool);
            }

            AppMan.App.DownloadOnly = false;
            AppMan.App.DriveLetter = driveLetter;
            foreach (SModel.Ivsu item in ivsuList)
            {
                if (item.Selected)
                {
                    if (item.Type == "APPS")
                        AppMan.App.AppsSelected = true;

                    AppMan.App.Ivsus.Add(item);
                }
            }

            AppMan.App.SelectedRegion = selectedRegion.Code;
            AppMan.App.SelectedRelease = selectedRelease;
            AppMan.App.SelectedMapVersion = selectedMapVersion;
        }
    }
}
