using System.Windows;
using AutoUpdaterDotNET;
using Newtonsoft.Json;
using Syn3Updater.Model;

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
            AutoUpdater.Start(Api.UpdaterReleaseURL);
        }

        private static void AutoUpdaterOnParseUpdateInfoEvent(ParseUpdateInfoEventArgs args)
        {
            {
                string asseturl = "";
                dynamic json = JsonConvert.DeserializeObject(args.RemoteData);
                foreach (var asset in json.assets)
                {
                    
                    string browser_download_url = asset.browser_download_url.ToString();
                    if (browser_download_url.Contains(".zip"))
                    {
                        asseturl = asset.browser_download_url;
                        break;
                    }
                    
                }
                if (json != null)
                {
                    args.UpdateInfo = new UpdateInfoEventArgs
                    {
                        ChangelogURL = Api.UpdaterChangelogURL,
                        CheckSum = null,
                        CurrentVersion = json.tag_name.ToString()
                            .Replace("v",
                                ""),
                        DownloadURL = asseturl,
                        Error = null,
                        InstalledVersion = null,
                        InstallerArgs = null,
                        IsUpdateAvailable = false,
                        Mandatory = null
                    };
                }
                    
            }
        }

        private void AutoUpdater_ApplicationExitEvent()
        {
            ApplicationManager.Instance.Exit();
            Application.Current.Shutdown();
        }
    }
}