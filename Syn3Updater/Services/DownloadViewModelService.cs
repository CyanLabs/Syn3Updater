using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Cyanlabs.Syn3Updater.Helper;
using Cyanlabs.Syn3Updater.Model;

namespace Cyanlabs.Syn3Updater.Services
{
    public static class DownloadViewModelService
    {
        public static StringBuilder CreateAutoInstallFile(string selectedRelease, string selectedRegion)
        {
            var autoinstalllst = new StringBuilder($@"; CyanLabs Syn3Updater {Assembly.GetEntryAssembly()?.GetName().Version} {AppMan.App.LauncherPrefs.ReleaseTypeInstalled} - Autoinstall {(AppMan.App.ModeForced ? "FORCED " : "")}Mode - {selectedRelease} {selectedRegion}{Environment.NewLine}{Environment.NewLine}");
            //naviextras not handled here 
            var ivsuList = AppMan.App.Ivsus.Where(item => item.Source != "naviextras").ToList();

            if (ivsuList.Any(i => i.Type == "MAP"))
            {
                autoinstalllst.Append($@"[SYNCGen3.0_3.0.1_PRODUCT]{Environment.NewLine}");
                var mapLicense = ivsuList.Find(i => i.Type == "MAP_LICENSE");
                autoinstalllst.Append($@"Item1 = {mapLicense.Type} - {mapLicense.FileName}\rOpen1 = SyncMyRide\{mapLicense.FileName}\r").Replace(@"\r", Environment.NewLine);
                ivsuList.Remove(mapLicense);

                List<uint> vals = ivsuList.ConvertAll(ivsu => (uint)ivsu.FileSize);
                //splits the ivsus into 3 evenly distibuted buckets 
                var buckets = new effPartition(vals, 3);
                for (ushort i = 0; i < buckets.SubsetCount; i++)
                {
                    for(int j =0; j< buckets[i].NumbIDs.Count; j++ )
                    {
                        var subIndex = (int)buckets[i].numbIDs[j];
                        //indexes for "Items" start at 1
                        var partIndex = j+1;
                        //indexes returned by the partition code start at 1 
                        SModel.Ivsu item = ivsuList[subIndex - 1];
                        if (i == 0) {
                            //since we added the MAP_LICENCE package above 
                            partIndex++;
                        }
                        autoinstalllst.Append($@"Item{partIndex} = {item.Type} - {item.FileName}\rOpen{partIndex} = SyncMyRide\{item.FileName}\r").Replace(@"\r", Environment.NewLine);
                    }
                    if (i == 0)
                    {
                        autoinstalllst.Append("Options = AutoInstall").Append(Environment.NewLine).Append(Environment.NewLine)
                            .Append($@"[SYNCGen3.0_3.0.1]{Environment.NewLine}");
                    }
                    if (i == 1)
                    {
                        autoinstalllst.Append("Options = AutoInstall, Include, Transaction").Append(Environment.NewLine).Append(Environment.NewLine)
                            .Append($@"[SYNCGen3.0_ALL]{Environment.NewLine}");
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
                    autoinstalllst.Append($@"Item{i + 1} = {item.Type} - {item.FileName}\rOpen{i + 1} = SyncMyRide\{item.FileName}\r").Replace(@"\r", Environment.NewLine);
                }
                autoinstalllst.Append("Options = AutoInstall").Append(Environment.NewLine);
            }
            return autoinstalllst;
        }
    }
}
