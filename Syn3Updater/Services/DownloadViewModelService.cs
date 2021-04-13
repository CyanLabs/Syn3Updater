using Cyanlabs.Syn3Updater;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cyanlabs.Updater.Services
{
    public static class DownloadViewModelService
    {
        public static StringBuilder CreateAutoInstallFile(string _selectedRelease, string _selectedRegion)
        {
            var autoinstalllst = new StringBuilder($@"; CyanLabs Syn3Updater {Assembly.GetEntryAssembly()?.GetName().Version} {AppMan.App.LauncherPrefs.ReleaseTypeInstalled} - Autoinstall {(AppMan.App.ModeForced ? "FORCED " : "")}Mode - {_selectedRelease} {_selectedRegion}{Environment.NewLine}{Environment.NewLine}");
            //naviextras not handled here 
            var ivsuList = AppMan.App.Ivsus.Where(item => item.Source != "naviextras").ToList();
            if (ivsuList.Any(i => i.Type == "MAP"))
            {
                // Sort by filesize, smallest first.
                ivsuList = ivsuList.OrderBy(item => item.FileSize).ToList();
                autoinstalllst.Append($@"[SYNCGen3.0_3.0.1_PRODUCT]{Environment.NewLine}");
                //split into 3 buckets 
                for (int i = 0; i < 3; i++)
                {
                    // 4.3 million bytes aka 4.3 GB, the size of one map partition
                    long binRemain = 4361305600;
                    for (var counter = 1; ivsuList.Count > 0; counter++)
                    {
                        var item = ivsuList[0];
                        //if adding the next file would made the size go over...
                        if (item.FileSize > binRemain)
                        {
                            break;
                        }
                        autoinstalllst.Append($@"Item{counter} = {item.Type} - {item.FileName}\rOpen{counter} = SyncMyRide\{item.FileName}\r").Replace(@"\r", Environment.NewLine);
                        binRemain -= item.FileSize;
                        ivsuList.RemoveAt(0);
                    }
                    if (i == 0)
                    {
                        autoinstalllst.Append("Options = AutoInstall").Append(Environment.NewLine).Append(Environment.NewLine);
                        autoinstalllst.Append($@"[SYNCGen3.0_3.0.1]{Environment.NewLine}");
                    }
                    if (i == 1)
                    {
                        autoinstalllst.Append("Options = AutoInstall, Include, Transaction").Append(Environment.NewLine).Append(Environment.NewLine);
                        autoinstalllst.Append($@"[SYNCGen3.0_ALL]{Environment.NewLine}");
                    }
                    if (i == 2)
                    {
                        autoinstalllst.Append("Options = Delay, Include, Transaction").Append(Environment.NewLine);
                    }
                }
            }
            else
            {
                autoinstalllst.Append($@"[SYNCGen3.0_ALL_PRODUCT]{Environment.NewLine}");
                for (int i = 0; i < ivsuList.Count; i++)
                {
                    var item = ivsuList[i];
                    autoinstalllst.Append($@"Item{i+1} = {item.Type} - {item.FileName}\rOpen{i+1} = SyncMyRide\{item.FileName}\r").Replace(@"\r", Environment.NewLine);
                }
                autoinstalllst.Append("Options = AutoInstall").Append(Environment.NewLine);
            }
            return autoinstalllst;
        }
    }
}
