using System.Windows;
using AutoUpdaterDotNET;
using Newtonsoft.Json;

namespace Syn3Updater.Helper
{
    public class AutoUpdaterHelper
    {
        public AutoUpdaterHelper()
        {
            AutoUpdater.ParseUpdateInfoEvent += AutoUpdaterOnParseUpdateInfoEvent;
            AutoUpdater.ApplicationExitEvent += AutoUpdater_ApplicationExitEvent;
            AutoUpdater.HttpUserAgent = "CyanLabs Application AutoUpdater v1.0";
            AutoUpdater.Mandatory = true;
            AutoUpdater.Synchronous = true;
            AutoUpdater.UpdateMode = Mode.ForcedDownload;
            AutoUpdater.Start("https://api.github.com/repos/cyanlabs/Syn3Updater/releases/latest");
        }

        private static void AutoUpdaterOnParseUpdateInfoEvent(ParseUpdateInfoEventArgs args)
        {
            {
                dynamic json = JsonConvert.DeserializeObject(args.RemoteData);
                if (json != null)
                    args.UpdateInfo = new UpdateInfoEventArgs
                    {
                        ChangelogURL = null,
                        CheckSum = null,
                        CurrentVersion = json.tag_name.ToString()
                            .Replace("v",
                                ""),
                        DownloadURL = json.assets[0]
                            .browser_download_url,
                        Error = null,
                        InstalledVersion = null,
                        InstallerArgs = null,
                        IsUpdateAvailable = false,
                        Mandatory = null
                    };
            }
        }

        private void AutoUpdater_ApplicationExitEvent()
        {
            ApplicationManager.Instance.Exit();
            Application.Current.Shutdown();
        }
    }
}