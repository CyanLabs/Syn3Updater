using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Cyanlabs.Syn3Updater.Helper;
using Cyanlabs.Syn3Updater.Model;
using ModernWpf.Controls;

namespace Cyanlabs.Syn3Updater.Services
{
    public static class HomeViewModelService
    {
        public static async Task<bool> Download(string installMode, ObservableCollection<SModel.Ivsu> ivsuList,
            SModel.SRegion selectedRegion, string selectedRelease, string selectedMapVersion, string driveLetter, USBHelper.Drive selectedDrive)
        {
            if (await SetIvsuList(installMode, ivsuList, selectedRegion, selectedRelease, selectedMapVersion, driveLetter) == false)
            {
                await Application.Current.Dispatcher.BeginInvoke(() => UIHelper.ShowErrorDialog(LM.GetValue("MessageBox.NoPackagesSelected")));
                return false;
            }

            bool canceldownload = false;
            
            //Install Mode is reformat or downgrade My20 warning
            if ((installMode == "reformat" || installMode == "downgrade") && !AppMan.App.DownloadOnly && AppMan.App.Settings.My20v2 == null)
                if (await Application.Current.Dispatcher.Invoke(() => UIHelper.ShowDialog(string.Format(LM.GetValue("MessageBox.My20Check")), LM.GetValue("String.Warning") + "!", LM.GetValue("String.No"),
                    LM.GetValue("String.Yes"), null, ContentDialogButton.None, Brushes.DarkRed)) == ContentDialogResult.Primary)
                {
                    await USBHelper.LogPrepareUSBAction(selectedDrive, driveLetter, "logutilitymy20");
                    return true;
                }

            //Warn is users region is different to new selection
            if (selectedRegion.Code != AppMan.App.Settings.CurrentRegion)
                if (await Application.Current.Dispatcher.Invoke(() => UIHelper.ShowWarningDialog(string.Format(LM.GetValue("MessageBox.CancelRegionMismatch")), LM.GetValue("String.Warning") + "!", LM.GetValue("String.No"),
                    LM.GetValue("String.Yes"))) != ContentDialogResult.Primary)
                    canceldownload = true;

            //Cancel no apps package selected
            if (!AppMan.App.AppsSelected && (installMode == "reformat" || installMode == "downgrade"))
            {
                await Application.Current.Dispatcher.BeginInvoke(() => UIHelper.ShowErrorDialog(LM.GetValue("MessageBox.CancelNoApps")));
                canceldownload = true;
            }


            if (!canceldownload && (AppMan.App.DownloadOnly || !await SanityCheckHelper.CancelDownloadCheck(selectedDrive)))
            {
                if (AppMan.App.DownloadOnly)
                {
                    AppMan.Logger.Info($"Starting download only of ({selectedRelease} - {selectedRegion?.Code} - {selectedMapVersion})");
                }
                else
                {
                    AppMan.App.DriveNumber = selectedDrive.Path.Replace("Win32_DiskDrive.DeviceID=\"\\\\\\\\.\\\\PHYSICALDRIVE", "").Replace("\"", "");
                    AppMan.App.DriveLetter = driveLetter;
                    AppMan.Logger.Info($"Starting process ({selectedRelease} - {selectedRegion?.Code} - {selectedMapVersion})");
                }

                AppMan.App.IsDownloading = true;
                AppMan.App.FireDownloadsTabEvent();
                return true;
            }

            return false;
        }

        public static async Task<bool> SetIvsuList(string installMode, ObservableCollection<SModel.Ivsu> ivsuList, SModel.SRegion selectedRegion, string selectedRelease,
            string selectedMapVersion, string driveLetter)
        {
            AppMan.Logger.Info(
                $"USB Drive selected - Name: {AppMan.App.DriveName} - FileSystem: {AppMan.App.DriveFileSystem} - PartitionType: {AppMan.App.DrivePartitionType} - Letter: {driveLetter}");
            AppMan.App.Ivsus.Clear();

            if (!ivsuList.Any(i => i.Selected)) return false;

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

            AppMan.App.DriveLetter = driveLetter;
            foreach (SModel.Ivsu item in ivsuList)
                if (item.Selected)
                {
                    if (item.Type == "APPS")
                        AppMan.App.AppsSelected = true;

                    AppMan.App.Ivsus.Add(item);
                }

            AppMan.App.SelectedRegion = selectedRegion.Code;
            AppMan.App.SelectedRelease = selectedRelease;
            AppMan.App.SelectedMapVersion = selectedMapVersion;
            return true;
        }
    }
}