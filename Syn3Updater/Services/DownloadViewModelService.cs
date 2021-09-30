using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Syn3Updater.Models;

namespace Syn3Updater.Services
{
    public static class DownloadViewModelService
    {
        public static StringBuilder CreateAutoInstallFile(string selectedRelease, string selectedRegion)
        {
            StringBuilder autoinstalllst = new($@"; CyanLabs Syn3Updater {Assembly.GetEntryAssembly()?.GetName().Version} - Autoinstall {(AppMan.App.ModeForced ? "FORCED " : "")}Mode - {selectedRelease} {selectedRegion}{Environment.NewLine}{Environment.NewLine}");
            //naviextras not handled here 
            List<SModel.Ivsu> ivsuList = AppMan.App.Ivsus.Where(item => item.Source != "naviextras").ToList();
            if (ivsuList.Any(i => i.Type == "MAP"))
            {
                // Sort by filesize, smallest first.
                ivsuList = ivsuList.OrderBy(item => item.FileSize).ToList();
                autoinstalllst.Append($@"[SYNCGen3.0_3.0.1_PRODUCT]{Environment.NewLine}");
                //split into 3 buckets 
                for (int i = 0; i < 3; i++)
                {
                    // 4.4 million bytes aka 4.4 GB, the size of one map partition
                    long binRemain = 4724461607;
                    for (int counter = 1; ivsuList.Count > 0; counter++)
                    {
                        SModel.Ivsu item = ivsuList[0];
                        //if adding the next file would made the size go over or bucket has more than 10 items...
                        if (item.FileSize > binRemain || counter >10)
                        {
                            break;
                        }
                        autoinstalllst.Append($@"Item{counter} = {item.Type} - {item.FileName}\rOpen{counter} = SyncMyRide\{item.FileName}\r").Replace(@"\r", Environment.NewLine);
                        binRemain -= item.FileSize;
                        ivsuList.RemoveAt(0);
                    }
                    switch (i)
                    {
                        case 0:
                            autoinstalllst.Append("Options = AutoInstall").Append(Environment.NewLine).Append(Environment.NewLine);
                            autoinstalllst.Append($@"[SYNCGen3.0_3.0.1]{Environment.NewLine}");
                            break;
                        case 1:
                            autoinstalllst.Append("Options = AutoInstall, Include, Transaction").Append(Environment.NewLine).Append(Environment.NewLine);
                            autoinstalllst.Append($@"[SYNCGen3.0_ALL]{Environment.NewLine}");
                            break;
                        case 2:
                            autoinstalllst.Append("Options = Delay, Include, Transaction").Append(Environment.NewLine);
                            break;
                    }
                }
            }
            else
            {
                autoinstalllst.Append($@"[SYNCGen3.0_ALL_PRODUCT]{Environment.NewLine}");
                for (int i = 0; i < ivsuList.Count; i++)
                {
                    SModel.Ivsu item = ivsuList[i];
                    autoinstalllst.Append($@"Item{i+1} = {item.Type} - {item.FileName}\rOpen{i+1} = SyncMyRide\{item.FileName}\r").Replace(@"\r", Environment.NewLine);
                }
                autoinstalllst.Append("Options = AutoInstall").Append(Environment.NewLine);
            }
            return autoinstalllst;
        }
    }
}