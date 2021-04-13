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
using Cyanlabs.Updater.Common;
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

        private ObservableCollection<SModel.SRegion> _sRegions;

        public ObservableCollection<SModel.SRegion> SRegions
        {
            get => _sRegions;
            set => SetProperty(ref _sRegions, value);
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
            new ObservableCollection<LanguageOption>(LM.Languages.Select(x => new LanguageOption { Name = x.EnglishName, Code = x.Code, Emoji = x.Emoji }));

        private string _currentRegion;

        public string CurrentRegion
        {
            get => _currentRegion;
            set
            {
                if (value != null)
                {
                    SetProperty(ref _currentRegion, value);
                    AppMan.App.Settings.CurrentRegion = value;
                }
            }
        }

        private string _currentVersion;

        public string CurrentVersion
        {
            get => _currentVersion;
            set
            {
                string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                if (value != null && value != $"_{decimalSeparator}_{decimalSeparator}_____" && value.Any(char.IsDigit))
                {
                    SetProperty(ref _currentVersion, value);
                    AppMan.App.SVersion = value;
                    AppMan.App.Settings.CurrentVersion = int.Parse(new string(value.Where(char.IsDigit).ToArray()));
                }
            }
        }

        private bool _currentNav;

        public bool CurrentNav
        {
            get => _currentNav;
            set
            {
                SetProperty(ref _currentNav, value);
                AppMan.App.Settings.CurrentNav = value;
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
                    AppMan.App.DownloadPath = value;
                    AppMan.App.Settings.DownloadPath = value;
                }
            }
        }

        private string _currentInstallMode;

        public string CurrentInstallMode
        {
            get => _currentInstallMode;
            set
            {
                if (value == null) return;
                AppMan.App.InstallMode = value;
                SetProperty(ref _currentInstallMode, value);
                AppMan.App.ModeForced = value != "autodetect";
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
                    AppMan.App.Settings.Theme = value;
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

        private bool _My20Mode;

        public bool My20Mode
        {
            get => _My20Mode;
            set
            {
                if (_My20Mode == true && value == false)
                {
                    if (ModernWpf.MessageBox.Show(LM.GetValue("MessageBox.My20Detected"), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        SetProperty(ref _My20Mode, value);
                    AppMan.App.Settings.My20 = value;
                }
                else
                {
                    SetProperty(ref _My20Mode, value);
                    AppMan.App.Settings.My20 = value;
                    CurrentInstallMode = "autodetect";
                }
                InstallModesEnabled = !value;
            }
        }

        private bool _installModesEnabled;

        public bool InstallModesEnabled
        {
            get => _installModesEnabled;
            set
            {
                SetProperty(ref _installModesEnabled, value);
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
                    AppMan.App.Settings.LicenseKey = value;
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
                    AppMan.App.Settings.Lang = value;
                    AppMan.App.FireLanguageChangedEvent();
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
                AppMan.App.LauncherPrefs.ReleaseBranch = value;
                AppMan.App.UpdateLauncherSettings();
            }
        }

        public class LanguageOption
        {
            public string Emoji { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
        }

        private LauncherPrefs.ReleaseType _currentReleaseType = AppMan.App.LauncherPrefs.ReleaseBranch;

        #endregion

        #region Methods

        public void Init()
        {
            AppMan.App.FireHomeTabEvent();
            //TODO Fix need for temp string
            string currentRegionTemp = AppMan.App.Settings.CurrentRegion;
            SRegions = new ObservableCollection<SModel.SRegion>
            {
                new SModel.SRegion {Code = "EU", Name = "Europe"},
                new SModel.SRegion {Code = "NA", Name = "United States, Canada & Mexico"},
                new SModel.SRegion {Code = "CN", Name = "China"},
                new SModel.SRegion {Code = "ANZ", Name = "Australia, New Zealand, South America, Turkey & Taiwan"},
                new SModel.SRegion {Code = "ROW", Name = "Middle East, Africa, India, Sri Lanka, Israel, South East Asia, Caribbean and Central America"}
            };
            CurrentRegion = currentRegionTemp;

            //TODO Fix need for temp string
            string currentInstallModeTemp = AppMan.App.InstallMode;
            InstallModes = new ObservableCollection<string>
            {
                "autodetect", "autoinstall", "reformat", "downgrade"
            };
            CurrentInstallMode = currentInstallModeTemp;

            Themes = new ObservableCollection<string>
            {
                "Dark", "Light"
            };
            CurrentTheme = AppMan.App.Settings.Theme;

            ReleaseTypes = new ObservableCollection<LauncherPrefs.ReleaseType>
            {
                LauncherPrefs.ReleaseType.Release,
                LauncherPrefs.ReleaseType.Beta,
                LauncherPrefs.ReleaseType.Ci
            };

            ReleaseType = AppMan.App.LauncherPrefs.ReleaseBranch;
            CurrentNav = AppMan.App.Settings.CurrentNav;
            My20Mode = AppMan.App.Settings.My20;
            DownloadLocation = AppMan.App.DownloadPath;
            LicenseKey = AppMan.App.Settings.LicenseKey;
            CurrentLanguage = AppMan.App.Settings.Lang;
        }

        public void ReloadSettings()
        {
            CurrentVersion = AppMan.App.SVersion;
            CurrentTheme = AppMan.App.Settings.Theme;
            My20Mode = AppMan.App.Settings.My20;
        }

        private void ApplySettingsAction()
        {
            if (ReleaseType != _currentReleaseType)
            {
                if (ModernWpf.MessageBox.Show(LM.GetValue("MessageBox.ChangeApplicationReleaseBranch"), "Syn3 Updater", MessageBoxButton.YesNo,
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
                        AppMan.Logger.Debug(e.GetFullMessage());
                        ModernWpf.MessageBox.Show(e.GetFullMessage(), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    ReleaseType = _currentReleaseType;
                }
            }

            if (LicenseKey?.Length < 10) LicenseKey = "";
            AppMan.App.FireHomeTabEvent();
            AppMan.App.SaveSettings();
        }

        private void DownloadPathAction()
        {
            string oldPath = AppMan.App.Settings.DownloadPath;
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                if (Directory.Exists(oldPath) && oldPath != dialog.SelectedPath)
                    if (ModernWpf.MessageBox.Show(string.Format(LM.GetValue("MessageBox.DownloadPathChangeCopy"),
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
                DownloadLocation = dialog.SelectedPath + "\\";
            }
        }
        #endregion
    }
}