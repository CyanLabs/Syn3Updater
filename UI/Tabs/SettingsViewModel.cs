using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.VisualBasic.FileIO;
using Ookii.Dialogs.Wpf;
using Syn3Updater.Helper;
using Syn3Updater.Model;

namespace Syn3Updater.UI.Tabs
{
    internal class SettingsViewModel : LanguageAwareBaseViewModel
    {
        private ActionCommand _applySettings;

        private string _currentInstallMode;

        private string _currentLanguage;

        private bool _currentSyncNav;

        private string _currentSyncRegion;

        private string _currentSyncVersion;

        private string _downloadLocation;

        private ActionCommand _downloadPathSelector;

        private string _licenseKey;

        private bool _showAllReleases;
        private ObservableCollection<SyncRegion> _syncRegions;
        private ObservableCollection<string> _installModes;

        public ObservableCollection<SyncRegion> SyncRegions
        {
            get => _syncRegions;
            set => SetProperty(ref _syncRegions, value);
        }

        public ObservableCollection<string> InstallModes
        {
            get => _installModes;
            set => SetProperty(ref _installModes, value);
        }

        public ObservableCollection<LanguageOption> Languages { get; set; } =
            new ObservableCollection<LanguageOption>(LanguageManager.Languages.Select(x => new LanguageOption {Name = x.NativeName, Code = x.Code, Emoji = x.Emoji}));

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

        public string CurrentSyncVersion
        {
            get => _currentSyncVersion;
            set
            {
                string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                if (value != $"_{decimalSeparator}_{decimalSeparator}_____" && value.Any(char.IsDigit))
                {
                    SetProperty(ref _currentSyncVersion, value);
                    Properties.Settings.Default.CurrentSyncVersion = int.Parse(new string(value.Where(char.IsDigit).ToArray()));
                }
            }
        }

        public bool CurrentSyncNav
        {
            get => _currentSyncNav;
            set
            {
                SetProperty(ref _currentSyncNav, value);
                Properties.Settings.Default.CurrentSyncNav = value;
            }
        }

        public string DownloadLocation
        {
            get => _downloadLocation;
            set
            {
                if (value != null)
                {
                    SetProperty(ref _downloadLocation, value);
                    ApplicationManager.Instance.DownloadLocation = value;
                    Properties.Settings.Default.DownloadLocation = value;
                }
            }
        }

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

        public bool ShowAllReleases
        {
            get => _showAllReleases;
            set
            {
                SetProperty(ref _showAllReleases, value);
                Properties.Settings.Default.ShowAllReleases = value;
            }
        }

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

        public ActionCommand DownloadPathSelector => _downloadPathSelector ?? (_downloadPathSelector = new ActionCommand(DownloadPathAction));

        public ActionCommand ApplySettings => _applySettings ?? (_applySettings = new ActionCommand(ApplySettingsAction));

        public void Init()
        {
            ApplicationManager.Instance.FireHomeTabEvent();
            //TODO Fix need for temp string
            string currentSyncRegionTemp = Properties.Settings.Default.CurrentSyncRegion;
            SyncRegions = new ObservableCollection<SyncRegion>
            {
                new SyncRegion {Code = "EU", Name = "Europe"},
                new SyncRegion {Code = "NA", Name = "North America & Canada"},
                new SyncRegion {Code = "CN", Name = "China"},
                new SyncRegion {Code = "ANZ", Name = "Australia & New Zealand"},
                new SyncRegion {Code = "ROW", Name = "Rest Of World"}
            };
            //OnPropertyChanged("SyncRegions");
            CurrentSyncRegion = currentSyncRegionTemp;

            //TODO Fix need for temp string
            string currentInstallModeTemp = Properties.Settings.Default.CurrentInstallMode != "" ? Properties.Settings.Default.CurrentInstallMode : "autodetect";
            InstallModes = new ObservableCollection<string>
            {
                "autodetect", "autoinstall", "reformat", "downgrade"
            };
            //OnPropertyChanged("InstallModes");
            CurrentInstallMode = currentInstallModeTemp;

            string _version = Properties.Settings.Default.CurrentSyncVersion.ToString();
            string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            if (_version.Length >= 5) CurrentSyncVersion = $"{_version[0]}{decimalSeparator}{_version[1]}{decimalSeparator}{_version.Substring(2, _version.Length - 2)}";
            CurrentSyncNav = Properties.Settings.Default.CurrentSyncNav;

            DownloadLocation = ApplicationManager.Instance.DownloadLocation;

            ShowAllReleases = Properties.Settings.Default.ShowAllReleases;
            LicenseKey = Properties.Settings.Default.LicenseKey;
            CurrentLanguage = Properties.Settings.Default.Lang;
        }

        private void ApplySettingsAction()
        {
            if (CurrentSyncVersion != "" || CurrentSyncVersion != "0" || CurrentSyncRegion != "")
                ApplicationManager.Instance.FireHomeTabEvent();
            else
                MessageBox.Show(LanguageManager.GetValue("MessageBox.NoSyncVersionOrRegionSelected"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void DownloadPathAction()
        {
            string oldPath = Properties.Settings.Default.DownloadLocation;
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog().GetValueOrDefault())
                if (Directory.Exists(oldPath))
                {
                    if (MessageBox.Show(
                            string.Format(LanguageManager.GetValue("MessageBox.DownloadPathChangeCopy"), Environment.NewLine + oldPath + Environment.NewLine,
                                Environment.NewLine + dialog.SelectedPath + Environment.NewLine), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Information) ==
                        MessageBoxResult.Yes)
                        if (oldPath != dialog.SelectedPath && !dialog.SelectedPath.Contains(oldPath))
                            try
                            {
                                FileSystem.MoveDirectory(oldPath, dialog.SelectedPath, UIOption.AllDialogs);
                            }
                            catch (OperationCanceledException)
                            {
                                //TODO Catch better
                            }

                    DownloadLocation = dialog.SelectedPath + "\\";
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