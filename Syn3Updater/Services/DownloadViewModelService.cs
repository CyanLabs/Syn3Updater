using Cyanlabs.Syn3Updater;
using Cyanlabs.Syn3Updater.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using Cyanlabs.Syn3Updater.Helper;

namespace Cyanlabs.Updater.Services
{
    public static class DownloadViewModelService
    {
        public static StringBuilder CreateAutoInstallFile(string _selectedRelease, string _selectedRegion)
        {
            int baseint = 1, part3int = 1, part2int = 1, part1int = 1;
            var autoinstalllst = new StringBuilder($@"; CyanLabs Syn3Updater {Assembly.GetEntryAssembly()?.GetName().Version} {ApplicationManager.Instance.LauncherPrefs.ReleaseTypeInstalled} - Autoinstall Mode - {_selectedRelease} {_selectedRegion}{Environment.NewLine}{Environment.NewLine}");

            // Sort by filesize, smallest first.
            ApplicationManager.Instance.Ivsus = new ObservableCollection<SModel.Ivsu>(from i in ApplicationManager.Instance.Ivsus orderby i.FileSize select i);

            if (ApplicationManager.Instance.Ivsus.Any(i => i.Type == "MAP"))
            {
                var autoinstalllstPart1 = new StringBuilder($@"[SYNCGen3.0_3.0.1_PRODUCT]{Environment.NewLine}");
                var autoinstalllstPart2 = new StringBuilder($@"[SYNCGen3.0_3.0.1]{Environment.NewLine}");
                var autoinstalllstPart3 = new StringBuilder($@"[SYNCGen3.0_ALL]{Environment.NewLine}");
                foreach (SModel.Ivsu item in ApplicationManager.Instance.Ivsus)
                {
                    if (item.Source == "naviextras") continue;
                    switch (baseint)
                    {
                        case 1:
                            autoinstalllstPart1.Append($@"Item{part1int} = {item.Type} - {item.FileName}\rOpen{part1int} = SyncMyRide\{item.FileName}\r").Replace(@"\r", Environment.NewLine);
                            part1int++;
                            baseint++;
                            break;
                        case 2:
                            autoinstalllstPart2.Append($@"Item{part2int} = {item.Type} - {item.FileName}\rOpen{part2int} = SyncMyRide\{item.FileName}\r").Replace(@"\r", Environment.NewLine);
                            part2int++;
                            baseint++;
                            break;
                        case 3:
                            autoinstalllstPart3.Append($@"Item{part3int} = {item.Type} - {item.FileName}\rOpen{part3int} = SyncMyRide\{item.FileName}\r").Replace(@"\r", Environment.NewLine);
                            part3int++;
                            baseint = 1;
                            break;
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
                foreach (SModel.Ivsu item in ApplicationManager.Instance.Ivsus)
                {
                    if (item.Source == "naviextras") continue;
                    autoinstalllst.Append($@"Item{baseint} = {item.Type} - {item.FileName}\rOpen{baseint} = SyncMyRide\{item.FileName}\r").Replace(@"\r", Environment.NewLine);
                    baseint++;
                }
                autoinstalllst.Append("Options = AutoInstall").Append(Environment.NewLine);
            }
            return autoinstalllst;
        }
    }
}
