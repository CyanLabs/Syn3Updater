using System.Windows;
using Syn3Updater.Model;

namespace Syn3Updater.Helper
{
    public class SanityCheckHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectedDrive"></param>
        /// <param name="allowDownloadonly"></param>
        /// <returns></returns>
        public static bool CancelDownloadCheck(USBHelper.Drive selectedDrive, bool allowDownloadonly = true)
        {
            string driveLetter = ApplicationManager.Instance.DriveLetter;
            string downloadPath = ApplicationManager.Instance.DownloadPath;

            //No USB drive selected, download only?

                if ((string.IsNullOrWhiteSpace(selectedDrive.Name) || selectedDrive.Name == LanguageManager.GetValue("Home.NoUSB")))
                {
                    if (allowDownloadonly)
                    {
                        if (ModernWpf.MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelNoUSB"), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        {
                            ApplicationManager.Logger.Info("[App] No usb has been selected, download only mode activated");
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

                //Ensure drive letter is not used as download path
            if (!string.IsNullOrEmpty(driveLetter))
                if (downloadPath.Contains(driveLetter))
                    ModernWpf.MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelDownloadIsDrive"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            //Optional Format
            if (!string.IsNullOrWhiteSpace(selectedDrive.Name) && selectedDrive.Name != LanguageManager.GetValue("Home.NoUSB") && ApplicationManager.Instance.DownloadOnly == false)
            {
                if (selectedDrive != null && ModernWpf.MessageBox.Show(string.Format(LanguageManager.GetValue("MessageBox.OptionalFormatUSB"), selectedDrive.Name, driveLetter),
                    "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                    ApplicationManager.Instance.SkipFormat = false;
                else
                    ApplicationManager.Logger.Info("[App] USB Drive not formatted, using existing filesystem and files");
            }

            //Format USB Drive
            if (!string.IsNullOrWhiteSpace(selectedDrive.Name) && ApplicationManager.Instance.DownloadOnly == false && ApplicationManager.Instance.SkipFormat == false)
                if (ModernWpf.MessageBox.Show(string.Format(LanguageManager.GetValue("MessageBox.CancelFormatUSB"), selectedDrive.Name, driveLetter), "Syn3 Updater",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    return true;
            

            return false;
        }
    }
}
