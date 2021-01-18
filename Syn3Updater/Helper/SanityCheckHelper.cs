using System.Windows;
using Cyanlabs.Syn3Updater.Model;
using MessageBox = Cyanlabs.Syn3Updater.UI.MessageBox.MessageBox;

namespace Cyanlabs.Syn3Updater.Helper
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

                if ((string.IsNullOrWhiteSpace(selectedDrive.Path) || selectedDrive.Name == LanguageManager.GetValue("Home.NoUSB")))
                {
                    if (allowDownloadonly)
                    {
                        if (MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelNoUSB"), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
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
                        MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelNoUSBForced"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return true;
                    }
                }

                //Ensure drive letter is not used as download path
            if (!string.IsNullOrEmpty(driveLetter))
                if (downloadPath.Contains(driveLetter))
                {
                    MessageBox.Show(LanguageManager.GetValue("MessageBox.CancelDownloadIsDrive"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return true;
                }
                    

            //Optional Format
            if (!string.IsNullOrWhiteSpace(selectedDrive.Path) && selectedDrive.Name != LanguageManager.GetValue("Home.NoUSB") && ApplicationManager.Instance.DownloadOnly == false)
            {
                if (MessageBox.Show(string.Format(LanguageManager.GetValue("MessageBox.OptionalFormatUSB"), selectedDrive.Name, driveLetter),
                    "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                    ApplicationManager.Instance.SkipFormat = false;
                else
                    ApplicationManager.Logger.Info("USB Drive not formatted, using existing filesystem and files");
            }

            //Format USB Drive
            if (!string.IsNullOrWhiteSpace(selectedDrive.Path) && ApplicationManager.Instance.DownloadOnly == false && ApplicationManager.Instance.SkipFormat == false)
                if (MessageBox.Show(string.Format(LanguageManager.GetValue("MessageBox.CancelFormatUSB"), selectedDrive.Name, driveLetter), "Syn3 Updater",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                {
                    ApplicationManager.Logger.Info("USB Drive will be formatted, using fresh filesystem");
                    return true;
                }
                    
            

            return false;
        }
    }
}
