using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Cyanlabs.Syn3Updater.Helper;
using Cyanlabs.Syn3Updater.Model;

namespace Cyanlabs.Syn3Updater.Services
{
    using ss_id = System.UInt16;
    using numb_id = Int32;
    public static class DownloadViewModelService
    {
        public static StringBuilder CreateAutoInstallFile(string _selectedRelease, string _selectedRegion)
        {
            var autoinstalllst = new StringBuilder($@"; CyanLabs Syn3Updater {Assembly.GetEntryAssembly()?.GetName().Version} {AppMan.App.LauncherPrefs.ReleaseTypeInstalled} - Autoinstall {(AppMan.App.ModeForced ? "FORCED " : "")}Mode - {_selectedRelease} {_selectedRegion}{Environment.NewLine}{Environment.NewLine}");
            //naviextras not handled here 
            var ivsuList = AppMan.App.Ivsus.Where(item => item.Source != "naviextras").ToList();

            //split into 3 buckets 
            if (ivsuList.Any(i => i.Type == "MAP"))
            {
                int baseint = 1, part3int = 1, part2int = 1, part1int = 2;
                
                var autoinstalllstPart1 = new StringBuilder($@"[SYNCGen3.0_3.0.1_PRODUCT]{Environment.NewLine}");
                var autoinstalllstPart2 = new StringBuilder($@"[SYNCGen3.0_3.0.1]{Environment.NewLine}");
                var autoinstalllstPart3 = new StringBuilder($@"[SYNCGen3.0_ALL]{Environment.NewLine}");
                
                var mapLicense = ivsuList.FirstOrDefault(i => i.Type == "MAP_LICENSE");
                autoinstalllstPart1.Append($@"Item1 = {mapLicense.Type} - {mapLicense.FileName}\rOpen1 = SyncMyRide\{mapLicense.FileName}\r").Replace(@"\r", Environment.NewLine);
                ivsuList.Remove(mapLicense);

                List<UInt32> vals = ivsuList.Select(ivsu => (uint) ivsu.FileSize).ToList();
                effPartition p = new effPartition(vals, 3);
                for (ss_id i = 0; i < p.SubsetCount; i++)
                {
                    foreach (int iId in p[i].NumbIDs)
                    {
                        SModel.Ivsu item = ivsuList[iId - 1];
                        switch (i)
                        {
                            case 0:
                                autoinstalllstPart1.Append($@"Item{part1int} = {item.Type} - {item.FileName}\rOpen{part1int} = SyncMyRide\{item.FileName}\r").Replace(@"\r", Environment.NewLine);
                                part1int++;
                                break;
                            case 1:
                                autoinstalllstPart2.Append($@"Item{part2int} = {item.Type} - {item.FileName}\rOpen{part2int} = SyncMyRide\{item.FileName}\r").Replace(@"\r", Environment.NewLine);
                                part2int++;
                                break;
                            case 2:
                                autoinstalllstPart3.Append($@"Item{part3int} = {item.Type} - {item.FileName}\rOpen{part3int} = SyncMyRide\{item.FileName}\r").Replace(@"\r", Environment.NewLine);
                                part3int++;
                                break;
                        }
                    }
                }
                autoinstalllstPart1.Append("Options = AutoInstall").Append(Environment.NewLine).Append(Environment.NewLine);
                autoinstalllstPart2.Append("Options = AutoInstall, Include, Transaction").Append(Environment.NewLine).Append(Environment.NewLine);
                autoinstalllstPart3.Append("Options = Delay, Include, Transaction").Append(Environment.NewLine);
                autoinstalllst.Append(autoinstalllstPart1).Append(autoinstalllstPart2).Append(autoinstalllstPart3);
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
