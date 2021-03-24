using Cyanlabs.Syn3Updater;
using Cyanlabs.Syn3Updater.Model;
using System;
using System.Text;

 namespace Cyanlabs.Updater.Common
{
    public static class DownloadViewModelService
    {
        public static StringBuilder CreateAutoInstallFile(string _selectedRelease, string _selectedRegion)
        {
            var autoinstalllst = new StringBuilder(
                $@"; CyanLabs Syn3Updater 2.x - Autoinstall Mode - {_selectedRelease} {_selectedRegion}{Environment.NewLine}{Environment.NewLine}[SYNCGen3.0_ALL_PRODUCT]{Environment.NewLine}");

            string extrafiles = "";
            int baseint = 0, extraint = 0;
            foreach (SModel.Ivsu item in ApplicationManager.Instance.Ivsus)
            {
                if (item.Source == "naviextras") 
                    continue;
                if (item.Type == "APPS" || item.Type == "VOICE" || item.Type == "ENH_DAB" || item.Type == "MAP_LICENSE" || item.Type == "VOICE_NAV" ||
                     !ApplicationManager.Instance.AppsSelected)
                {
                    baseint++;
                    autoinstalllst.Append($@"Item{baseint} = {item.Type} - {item.FileName}\rOpen{baseint} = SyncMyRide\{item.FileName}\r").Replace(@"\r", Environment.NewLine);
                }
                else if (ApplicationManager.Instance.AppsSelected)
                {
                    if (extrafiles?.Length == 0) extrafiles = $"[SYNCGen3.0_ALL]{Environment.NewLine}";
                    if (extraint == 10)
                    {
                        extraint = 0;
                        extrafiles += $@"Options = Delay,Include,Transaction{Environment.NewLine}[SYNCGen3.0_{ApplicationManager.Instance.SVersion}]{Environment.NewLine}";
                    }

                    extraint++;
                    extrafiles += $@"Item{extraint} = {item.Type} - {item.FileName}\rOpen{extraint} = SyncMyRide\{item.FileName}\r".Replace(@"\r", Environment.NewLine);
                }
            }

            if (extrafiles != "")
                extrafiles += "Options = Delay,Include,Transaction";
            autoinstalllst.Append("Options = AutoInstall").Append(Environment.NewLine)
                .Append(extrafiles);
            return autoinstalllst;
        }
    }
}
