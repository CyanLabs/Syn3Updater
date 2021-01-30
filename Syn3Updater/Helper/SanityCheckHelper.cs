using System.Windows;
using Cyanlabs.Syn3Updater.Model;

namespace Cyanlabs.Syn3Updater.Helper
{
    /// <summary>
    ///     Helper class to "Sanity Check" before running the main methods to prevent user error etc
    /// </summary>
    public class SanityCheckHelper
    {
        /// <summary>
        ///     A multi-section check to ensure nothing prevents the download from beginning
        /// </summary>
        /// <param name="selectedDrive">USB Drive</param>
        /// <param name="allowDownloadonly">True if download only is allowed, false if USB must be used</param>
        /// <returns>true/false as Boolean depending on if Download is cancelled or not</returns>
        public static bool CancelDownloadCheck(USBHelper.Drive selectedDrive, bool allowDownloadonly = true)
        {
            // Set local variables to the values of application level variables
            string driveLetter = ApplicationManager.Instance.DriveLetter;
            string downloadPath = ApplicationManager.Instance.DownloadPath;

            // No USB drive selected, download only?
            if (string.IsNullOrWhiteSpace(selectedDrive.Path) || selectedDrive.Name == LanguageManager.GetValue("Home.NoUSB"))
            {
                if (allowDownloadonly)
                {
                    if (ModernWpf.MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelNoUSB"), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                        MessageBoxResult.Yes)
                    {
                        ApplicationManager.Logger.Info("No usb has been selected, download only mode activated");
                        ApplicationManager.Instance.DownloadOnly = true;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    ModernWpf.MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelNoUSBForced"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return true;
                }
            }

            // Ensure drive letter is not used as download path
            if (!string.IsNullOrEmpty(driveLetter))
                if (downloadPath.Contains(driveLetter))
                {
                    ModernWpf.MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelDownloadIsDrive"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return true;
                }


            // Optional Format
            if (!string.IsNullOrWhiteSpace(selectedDrive.Path) && selectedDrive.Name != LanguageManager.GetValue("Home.NoUSB") && ApplicationManager.Instance.DownloadOnly == false)
            {
                if (ModernWpf.MessageBox.Show(string.Format(LanguageManager.GetValue("MessageBox.OptionalFormatUSB"), selectedDrive.Name, driveLetter),
                    "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                    ApplicationManager.Instance.SkipFormat = false;
                else
                {
                    ApplicationManager.Logger.Info("USB Drive not formatted, using existing filesystem and files");
                    ApplicationManager.Instance.SkipFormat = true;
                }
            }

            // Format USB Drive
            if (!string.IsNullOrWhiteSpace(selectedDrive.Path) && ApplicationManager.Instance.DownloadOnly == false && ApplicationManager.Instance.SkipFormat == false)
                if (ModernWpf.MessageBox.Show(string.Format(LanguageManager.GetValue("MessageBox.CancelFormatUSB"), selectedDrive.Name, driveLetter), "Syn3 Updater",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                {
                    ApplicationManager.Logger.Info("USB Drive will be formatted, using fresh filesystem");
                    return true;
                }

            // If nothing above has returned true then download has not been cancelled and method will return false;
            return false;
        }
    }
}