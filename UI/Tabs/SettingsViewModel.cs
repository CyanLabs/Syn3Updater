using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.VisualBasic.FileIO;
using Syn3Updater.Helper;
using Syn3Updater.Model;

namespace Syn3Updater.UI.Tabs
{

    internal class SettingsViewModel : LanguageAwareBaseViewModel
    {
        public void Init()
        {
            //TODO Fix need for temp string
            string CurrentSyncRegionTemp = Properties.Settings.Default.CurrentSyncRegion;
            SyncRegions = new ObservableCollection<SyncRegion>
            {
                new SyncRegion {Code = "EU", Name = "Europe"},
                new SyncRegion {Code = "NA", Name = "North America & Canada"},
                new SyncRegion {Code = "CN", Name = "China"},
                new SyncRegion {Code = "ANZ", Name = "Australia & New Zealand"},
                new SyncRegion {Code = "ROW", Name = "Rest Of World"}
            };
            OnPropertyChanged("SyncRegions");
            CurrentSyncRegion = CurrentSyncRegionTemp;

            //TODO Fix need for temp string
            string CurrentInstallModeTemp = Properties.Settings.Default.CurrentInstallMode;
            InstallModes = new ObservableCollection<string>
            {
                "autodetect","autoinstall","reformat","downgrade"
            };
            OnPropertyChanged("InstallModes");
            CurrentInstallMode = CurrentInstallModeTemp;

            CurrentSyncVersion = Properties.Settings.Default.CurrentSyncVersion.ToString();
            CurrentSyncNav = Properties.Settings.Default.CurrentSyncNav;

            DownloadLocation = Properties.Settings.Default.DownloadLocation == ""
                ? KnownFolders.GetPath(KnownFolder.Downloads) + @"\Syn3Updater\"
                : Properties.Settings.Default.DownloadLocation;

            ShowAllReleases = Properties.Settings.Default.ShowAllReleases;
            LicenseKey = Properties.Settings.Default.LicenseKey;
            CurrentLanguage = Properties.Settings.Default.Lang;
        }

        public ObservableCollection<SyncRegion> SyncRegions { get; set; }
        public ObservableCollection<string> InstallModes { get; set; }

        public ObservableCollection<LanguageOption> Languages { get; set; } =
            new ObservableCollection<LanguageOption>(
                LanguageManager.Languages.Select(x => new LanguageOption { Name = x.NativeName, Code = x.Code, Emoji = x.Emoji }));

        private string _currentSyncRegion;

        public string CurrentSyncRegion
        {
            get => _currentSyncRegion;
            set
            {
                if (value != null)
                {
                    SetProperty(ref _currentSyncRegion, value);
                    Properties.Settings.Default.CurrentSyncRegion = value;
                }
            }
        }

        private string _currentSyncVersion;

        public string CurrentSyncVersion
        {
            get => _currentSyncVersion;
            set
            {
                SetProperty(ref _currentSyncVersion, value);
                Properties.Settings.Default.CurrentSyncVersion = Int32.Parse(new string(value.Where(c => char.IsDigit(c)).ToArray()));
            }
        }

        private bool _currentSyncNav;

        public bool CurrentSyncNav
        {
            get => _currentSyncNav;
            set
            {
                SetProperty(ref _currentSyncNav, value);
                Properties.Settings.Default.CurrentSyncNav = value;
            }
        }

        private string _downloadLocation;

        public string DownloadLocation
        {
            get => _downloadLocation;
            set
            {
                if (value != null)
                {
                    SetProperty(ref _downloadLocation, value);
                    Properties.Settings.Default.DownloadLocation = value;
                }
            }
        }

        private string _currentInstallMode;

        public string CurrentInstallMode
        {
            get => _currentInstallMode;
            set
            {
                if (value != null)
                {
                    SetProperty(ref _currentInstallMode, value);
                    Properties.Settings.Default.CurrentInstallMode = value;
                }
            }
        }

        private bool _showAllReleases;

        public bool ShowAllReleases
        {
            get => _showAllReleases;
            set
            {
                SetProperty(ref _showAllReleases, value);
                Properties.Settings.Default.ShowAllReleases = value;
            }
        }


        private string _licenseKey;

        public string LicenseKey
        {
            get => _licenseKey;
            set
            {
                if (value != null)
                {
                    SetProperty(ref _licenseKey, value);
                    Properties.Settings.Default.LicenseKey = value;
                }
            }
        }

        private string _currentLanguage;

        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                _currentLanguage = value;
                if (value != null)
                {
                    SetProperty(ref _currentLanguage, value);
                    Properties.Settings.Default.Lang = value;
                    ApplicationManager.Instance.FireLanguageChangedEvent();
                }

            }
        }
        
        private ActionCommand _downloadPathSelector;
        public ActionCommand DownloadPathSelector => _downloadPathSelector ?? (_downloadPathSelector = new ActionCommand(DownloadPathAction));

        private void DownloadPathAction()
        {
            string oldpath = Properties.Settings.Default.DownloadLocation;
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                if (Directory.Exists(oldpath))
                {
                    if (MessageBox.Show(string.Format(LanguageManager.GetValue("MessageBox.DownloadPathChangeCopy"), Environment.NewLine + oldpath + Environment.NewLine, Environment.NewLine + dialog.SelectedPath + Environment.NewLine), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                    {
                        if (oldpath != dialog.SelectedPath)
                        {
                            try
                            {
                                FileSystem.MoveDirectory(oldpath, dialog.SelectedPath,
                                UIOption.AllDialogs);
                            }
                            catch (OperationCanceledException)
                            {
                                //TODO Catch better
                            }
                        }
                    }
                    Properties.Settings.Default.DownloadLocation = dialog.SelectedPath;
                    Properties.Settings.Default.Save();
                }
            }
        }

        public class SyncRegion
        {
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class LanguageOption
        {
            public string Emoji { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
        }
    }
}

