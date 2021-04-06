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
            int baseint = 0, part2int = 1, part1int = 1;
            var autoinstalllst = new StringBuilder();

            // Sort by filesize, smallest first.
            ApplicationManager.Instance.Ivsus = new ObservableCollection<SModel.Ivsu>(from i in ApplicationManager.Instance.Ivsus orderby i.FileSize select i);

            if (ApplicationManager.Instance.Ivsus.Any(i => i.Type == "MAP"))
            {
                var autoinstalllstPart1 = new StringBuilder($@"; CyanLabs Syn3Updater {Assembly.GetEntryAssembly()?.GetName().Version} ({ApplicationManager.Instance.LauncherPrefs.ReleaseTypeInstalled} - Autoinstall Mode - {_selectedRelease} {_selectedRegion}{Environment.NewLine}{Environment.NewLine}[SYNCGen3.0_ALL_PRODUCT]{Environment.NewLine}");
                var autoinstalllstPart2 = new StringBuilder($@"[SYNCGen3.0_ALL]{Environment.NewLine}");
                foreach (SModel.Ivsu item in ApplicationManager.Instance.Ivsus)
                {
                    if (item.Source == "naviextras") continue;
                    if (MathHelper.IsOdd(baseint))
                    {
                        autoinstalllstPart2.Append($@"Item{part2int} = {item.Type} - {item.FileName}\rOpen{part2int} = SyncMyRide\{item.FileName}\r").Replace(@"\r", Environment.NewLine);
                        part2int++;
                    }
                    else
                    {
                        autoinstalllstPart1.Append($@"Item{part1int} = {item.Type} - {item.FileName}\rOpen{part1int} = SyncMyRide\{item.FileName}\r").Replace(@"\r", Environment.NewLine);
                        part1int++;
                    }

                    baseint++;
                }
                autoinstalllstPart1.Append("Options = AutoInstall").Append(Environment.NewLine).Append(Environment.NewLine);
                autoinstalllstPart2.Append("Options = Delay,Include,Transaction").Append(Environment.NewLine);
                autoinstalllst.Append(autoinstalllstPart1).Append(autoinstalllstPart2);
            }
            else
            {
                autoinstalllst.Append($@"; CyanLabs Syn3Updater 2.x - Autoinstall Mode - {_selectedRelease} {_selectedRegion}{Environment.NewLine}{Environment.NewLine}[SYNCGen3.0_ALL_PRODUCT]{Environment.NewLine}");
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
