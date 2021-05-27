using System.IO;
using System.Linq;
using System.Windows;
using Cyanlabs.Syn3Updater.Model;

namespace Cyanlabs.Syn3Updater.Helper
{
    /// <summary>
    ///     Helper class to "Sanity Check" before running the main methods to prevent user error etc
    /// </summary>
    public static class SanityCheckHelper
    {
        /// <summary>
        ///     A multi-section check to ensure nothing prevents the download from beginning
        /// </summary>
        /// <param name="selectedDrive">USB Drive</param>
        /// <returns>true/false as Boolean depending on if Download is cancelled or not</returns>
        public static bool CancelDownloadCheck(USBHelper.Drive selectedDrive)
        {
            // Set local variables to the values of application level variables
            string driveLetter = AppMan.App.DriveLetter;
            string downloadPath = AppMan.App.DownloadPath;

            // Ensure drive letter is not used as download path
            if (!string.IsNullOrEmpty(driveLetter))
                if (downloadPath.Contains(driveLetter))
                {
                    ModernWpf.MessageBox.Show(LM.GetValue("MessageBox.CancelDownloadIsDrive"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return true;
                }

            // Optional Format
            if (!string.IsNullOrWhiteSpace(selectedDrive.Path))
            {
                if (ModernWpf.MessageBox.Show(string.Format(LM.GetValue("MessageBox.OptionalFormatUSB"), selectedDrive.Name, driveLetter),
                    "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                    AppMan.App.SkipFormat = false;
                else
                {
                    AppMan.Logger.Info("USB Drive not formatted, using existing filesystem and files");
                    AppMan.App.SkipFormat = true;
                }
            }

            // Format USB Drive
            if (!string.IsNullOrWhiteSpace(selectedDrive.Path) && !AppMan.App.SkipFormat)
                if (ModernWpf.MessageBox.Show(string.Format(LM.GetValue("MessageBox.CancelFormatUSB"), selectedDrive.Name, driveLetter), "Syn3 Updater",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                {
                    return true;
                }

            if (selectedDrive.Name == LM.GetValue("Home.NoUSBDir"))
            {
                AppMan.Logger.Info("Using 'Select a Directory' instead of a USB Drive");
                AppMan.App.DownloadToFolder = true;

                if (Directory.EnumerateFiles(driveLetter).Any())
                {
                    if (ModernWpf.MessageBox.Show(string.Format(LM.GetValue("MessageBox.CancelDeleteFiles"), driveLetter), "Syn3 Updater",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    {
                        return true;
                    }
                }
            }

            // If nothing above has returned true then download has not been cancelled and method will return false;
            return false;
        }
    }
}