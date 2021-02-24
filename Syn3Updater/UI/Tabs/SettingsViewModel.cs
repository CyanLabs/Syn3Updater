using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using Cyanlabs.Syn3Updater.Helper;
using Cyanlabs.Syn3Updater.Model;
using Microsoft.VisualBasic.FileIO;
using ModernWpf;
using Ookii.Dialogs.Wpf;
using SharedCode;
using ElementTheme = SourceChord.FluentWPF.ElementTheme;
using ResourceDictionaryEx = SourceChord.FluentWPF.ResourceDictionaryEx;

namespace Cyanlabs.Syn3Updater.UI.Tabs
{
    internal class SettingsViewModel : LanguageAwareBaseViewModel
    {
        #region Constructors

        private ActionCommand _downloadPathSelector;
        private ActionCommand _applySettings;
        public ActionCommand DownloadPathSelector => _downloadPathSelector ??= new ActionCommand(DownloadPathAction);
        public ActionCommand ApplySettings => _applySettings ??= new ActionCommand(ApplySettingsAction);

        #endregion

        #region Properties & Fields

        private ObservableCollection<SyncModel.SyncRegion> _syncRegions;

        public ObservableCollection<SyncModel.SyncRegion> SyncRegions
        {
            get => _syncRegions;
            set => SetProperty(ref _syncRegions, value);
        }

        private ObservableCollection<string> _installModes;

        public ObservableCollection<string> InstallModes
        {
            get => _installModes;
            set => SetProperty(ref _installModes, value);
        }

        private ObservableCollection<LauncherPrefs.ReleaseType> _releaseTypes;

        public ObservableCollection<LauncherPrefs.ReleaseType> ReleaseTypes
        {
            get => _releaseTypes;
            set => SetProperty(ref _releaseTypes, value);
        }

        private ObservableCollection<string> _themes;

        public ObservableCollection<string> Themes
        {
            get => _themes;
            set => SetProperty(ref _themes, value);
        }

        public ObservableCollection<LanguageOption> Languages { get; set; } =
            new ObservableCollection<LanguageOption>(LanguageManager.Languages.Select(x => new LanguageOption {Name = x.EnglishName, Code = x.Code, Emoji = x.Emoji}));

        private string _currentSyncRegion;

        public string CurrentSyncRegion
        {
            get => _currentSyncRegion;
            set
            {
                if (value != null)
                {
                    SetProperty(ref _currentSyncRegion, value);
                    ApplicationManager.Instance.Settings.CurrentSyncRegion = value;
                }
            }
        }

        private string _currentSyncVersion;

        public string CurrentSyncVersion
        {
            get => _currentSyncVersion;
            set
            {
                string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                if (value != null && value != $"_{decimalSeparator}_{decimalSeparator}_____" && value.Any(char.IsDigit))
                {
                    SetProperty(ref _currentSyncVersion, value);
                    ApplicationManager.Instance.SyncVersion = value;
                    ApplicationManager.Instance.Settings.CurrentSyncVersion = int.Parse(new string(value.Where(char.IsDigit).ToArray()));
                }
            }
        }

        private bool _currentSyncNav;

        public bool CurrentSyncNav
        {
            get => _currentSyncNav;
            set
            {
                SetProperty(ref _currentSyncNav, value);
                ApplicationManager.Instance.Settings.CurrentSyncNav = value;
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
                    ApplicationManager.Instance.DownloadPath = value;
                    ApplicationManager.Instance.Settings.DownloadPath = value;
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
                    ApplicationManager.Instance.Settings.CurrentInstallMode = value;
                }
            }
        }

        private string _currentTheme;

        public string CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (value != null)
                {
                    SetProperty(ref _currentTheme, value);
                    ApplicationManager.Instance.Settings.Theme = value;
                    if (value == "Dark")
                    {
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                        ResourceDictionaryEx.GlobalTheme = ElementTheme.Dark;
                    }
                    else if (value == "Light")
                    {
                        ResourceDictionaryEx.GlobalTheme = ElementTheme.Light;
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                    }
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
                ApplicationManager.Instance.Settings.ShowAllReleases = value;
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
                    ApplicationManager.Instance.Settings.LicenseKey = value;
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
                    ApplicationManager.Instance.Settings.Lang = value;
                    ApplicationManager.Instance.FireLanguageChangedEvent();
                }
            }
        }

        private LauncherPrefs.ReleaseType _releaseType;

        public LauncherPrefs.ReleaseType ReleaseType
        {
            get => _releaseType;
            set

            {
                SetProperty(ref _releaseType, value);
                ApplicationManager.Instance.LauncherPrefs.ReleaseBranch = value;
                ApplicationManager.Instance.UpdateLauncherSettings();
            }
        }

        public class LanguageOption
        {
            public string Emoji { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
        }

        private LauncherPrefs.ReleaseType _currentReleaseType = ApplicationManager.Instance.LauncherPrefs.ReleaseBranch;

        #endregion

        #region Methods

        public void Init()
        {
            ApplicationManager.Instance.FireHomeTabEvent();
            //TODO Fix need for temp string
            string currentSyncRegionTemp = ApplicationManager.Instance.Settings.CurrentSyncRegion;
            SyncRegions = new ObservableCollection<SyncModel.SyncRegion>
            {
                new SyncModel.SyncRegion {Code = "EU", Name = "Europe"},
                new SyncModel.SyncRegion {Code = "NA", Name = "North America & Canada"},
                new SyncModel.SyncRegion {Code = "CN", Name = "China"},
                new SyncModel.SyncRegion {Code = "ANZ", Name = "Australia, New Zealand, South America, Turkey & Taiwan"},
                new SyncModel.SyncRegion {Code = "ROW", Name = "Middle East, Africa, India, Sri Lanka, Israel, South East Asia, Caribbean and Central America"}
            };
            CurrentSyncRegion = currentSyncRegionTemp;

            //TODO Fix need for temp string
            string currentInstallModeTemp = ApplicationManager.Instance.Settings.CurrentInstallMode != "" ? ApplicationManager.Instance.Settings.CurrentInstallMode : "autodetect";
            InstallModes = new ObservableCollection<string>
            {
                "autodetect", "autoinstall", "reformat", "downgrade"
            };
            CurrentInstallMode = currentInstallModeTemp;

            Themes = new ObservableCollection<string>
            {
                "Dark", "Light"
            };
            CurrentTheme = ApplicationManager.Instance.Settings.Theme;

            ReleaseTypes = new ObservableCollection<LauncherPrefs.ReleaseType>
            {
                LauncherPrefs.ReleaseType.Release,
                LauncherPrefs.ReleaseType.Beta,
                LauncherPrefs.ReleaseType.Ci
            };
            
            ReleaseType = ApplicationManager.Instance.LauncherPrefs.ReleaseBranch;
            CurrentSyncNav = ApplicationManager.Instance.Settings.CurrentSyncNav;

            DownloadLocation = ApplicationManager.Instance.DownloadPath;
            ShowAllReleases = ApplicationManager.Instance.Settings.ShowAllReleases;
            LicenseKey = ApplicationManager.Instance.Settings.LicenseKey;
            CurrentLanguage = ApplicationManager.Instance.Settings.Lang;
        }

        public void ReloadSettings()
        {
            CurrentSyncVersion = ApplicationManager.Instance.SyncVersion;
            CurrentTheme = ApplicationManager.Instance.Settings.Theme;
        }

        private void ApplySettingsAction()
        {
            if (ReleaseType != _currentReleaseType)
            {
                if (ModernWpf.MessageBox.Show(LanguageManager.GetValue("MessageBox.ChangeApplicationReleaseBranch"), "Syn3 Updater", MessageBoxButton.YesNo,
                    MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    _currentReleaseType = ReleaseType;
                    try
                    {
                        Application.Current.Shutdown();
                        Process.Start("Launcher.exe");
                    }
                    catch (Win32Exception e)
                    {
                        ApplicationManager.Logger.Debug(e.GetFullMessage());
                        ModernWpf.MessageBox.Show(e.GetFullMessage(), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    ReleaseType = _currentReleaseType;
                }
            }
            ApplicationManager.Instance.FireHomeTabEvent();
        }

        private void DownloadPathAction()
        {
            string oldPath = ApplicationManager.Instance.Settings.DownloadPath;
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                if (Directory.Exists(oldPath) && oldPath != dialog.SelectedPath)
                    if (ModernWpf.MessageBox.Show(string.Format(LanguageManager.GetValue("MessageBox.DownloadPathChangeCopy"),
                            Environment.NewLine + oldPath + Environment.NewLine,
                            Environment.NewLine + dialog.SelectedPath + Environment.NewLine), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Information) ==
                        MessageBoxResult.Yes)
                    {
                        try
                        {
                            foreach (var file in Directory.GetFiles(oldPath, "*.TAR.GZ"))
                                FileSystem.MoveFile(file, Path.Combine(dialog.SelectedPath, Path.GetFileName(file)), UIOption.AllDialogs);
                        }
                        catch (OperationCanceledException)
                        {
                            //TODO Catch better
                        }
                    }
                DownloadLocation = dialog.SelectedPath;
            }
        }
        #endregion
    }
}