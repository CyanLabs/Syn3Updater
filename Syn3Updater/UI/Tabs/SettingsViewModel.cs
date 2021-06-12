using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using AsyncAwaitBestPractices.MVVM;
using Cyanlabs.Syn3Updater.Helper;
using Cyanlabs.Syn3Updater.Model;
using Cyanlabs.Updater.Common;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using ModernWpf;
using ModernWpf.Controls;
using Ookii.Dialogs.Wpf;
using ElementTheme = SourceChord.FluentWPF.ElementTheme;
using ResourceDictionaryEx = SourceChord.FluentWPF.ResourceDictionaryEx;

namespace Cyanlabs.Syn3Updater.UI.Tabs
{
    internal class SettingsViewModel : LanguageAwareBaseViewModel
    {
        #region Constructors

        private AsyncCommand<string> _pathSelector;
        private ActionCommand _applySettings;
        public AsyncCommand<string> PathSelector => _pathSelector ??= new AsyncCommand<string>(SelectPathAction);
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
            new(LM.Languages.Select(x => new LanguageOption {Name = x.EnglishName, Code = x.Code, Emoji = x.Emoji}));

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
                if (value != null && value.Any(char.IsDigit))
                {
                    SetProperty(ref _currentVersion, value);
                    AppMan.App.SVersion = value;
                    AppMan.App.Settings.CurrentVersion = int.Parse(new string(value.Where(char.IsDigit).ToArray()));
                }
            }
        }

        private double _downloadConnectionsValue;

        public double DownloadConnectionsValue
        {
            get => _downloadConnectionsValue;
            set
            {
                if (double.IsNaN(value)) value = 8;
                SetProperty(ref _downloadConnectionsValue, Convert.ToInt16(value));
                AppMan.App.Settings.DownloadConnections = Convert.ToInt16(value);
            }
        }

        private int _downloadConnectionsMin;

        public int DownloadConnectionsMin
        {
            get => _downloadConnectionsMin;
            set => SetProperty(ref _downloadConnectionsMin, value);
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
                    AppMan.App.MainSettings.DownloadPath = value;
                }
            }
        }

        private string _logLocation;

        public string LogLocation
        {
            get => _logLocation;
            set
            {
                if (value != null)
                {
                    SetProperty(ref _logLocation, value);
                    AppMan.App.MainSettings.LogPath = value;
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
                AppMan.App.Settings.InstallMode = value;
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
                    AppMan.App.MainSettings.Theme = value;
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

        
        private string _my20ModeText;

        public string My20ModeText
        {
            get => _my20ModeText;
            set => SetProperty(ref _my20ModeText, value);
        }
        
        private bool? _my20Mode;

        public bool? My20Mode
        {
            get => _my20Mode;
            set
            {
                SetProperty(ref _my20Mode, value);
                AppMan.App.Settings.My20v2 = value;
                InstallModesEnabled = AppMan.App.Settings.My20v2 != true && AdvancedModeToggle;
                My20ModeText = value switch
                {
                    null => "Autodetect",
                    true => LM.GetValue("String.Enabled"),
                    false => LM.GetValue("String.Disabled"),
                };
            }
        }

        private bool _advancedModeToggle;

        public bool AdvancedModeToggle
        {
            get => _advancedModeToggle;
            set
            {
                SetProperty(ref _advancedModeToggle, value);
                InstallModesEnabled = AppMan.App.Settings.My20v2 != true && AdvancedModeToggle;
            }
        }

        private bool _installModesEnabled;

        public bool InstallModesEnabled
        {
            get => _installModesEnabled;
            set => SetProperty(ref _installModesEnabled, value);
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
                    AppMan.App.MainSettings.LicenseKey = value;
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

                    if (value != AppMan.App.MainSettings.Lang)
                        AppMan.App.LanguageChanged = true;

                    AppMan.App.MainSettings.Lang = value;
                    AppMan.App.FireLanguageChangedEvent();
                    OnPropertyChanged();
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

        private Api.Ivsus2 _syncVersions;

        public Api.Ivsus2 SyncVersions
        {
            get => _syncVersions;
            set => SetProperty(ref _syncVersions, value);
        }

        private LauncherPrefs.ReleaseType _currentReleaseType = AppMan.App.LauncherPrefs.ReleaseBranch;

        #endregion

        #region Methods

        public void Init()
        {
            DownloadConnectionsValue = AppMan.App.Settings.DownloadConnections;
            DownloadConnectionsMin = 1;
            if (AppMan.App.MainSettings.Lang != null)
                CurrentLanguage = AppMan.App.MainSettings.Lang;
            else if (Languages.Any(x => x.Code == CultureInfo.CurrentCulture.ToString()))
                CurrentLanguage = CultureInfo.CurrentCulture.ToString();
            else if (Languages.Any(x => x.Code == CultureInfo.CurrentCulture.TwoLetterISOLanguageName))
                CurrentLanguage = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

            AppMan.App.FireHomeTabEvent();
            string currentRegionTemp = AppMan.App.Settings.CurrentRegion;
            SRegions = new ObservableCollection<SModel.SRegion>
            {
                new() {Code = "EU", Name = "Europe"},
                new() {Code = "NA", Name = "United States, Canada & Mexico"},
                new() {Code = "CN", Name = "China"},
                new() {Code = "ANZ", Name = "Australia, New Zealand, South America, Turkey & Taiwan"},
                new() {Code = "ROW", Name = "Middle East, Africa, India, Sri Lanka, Israel, South East Asia, Caribbean and Central America"}
            };
            CurrentRegion = currentRegionTemp;
            string currentInstallModeTemp = AppMan.App.Settings.InstallMode;
            InstallModes = new ObservableCollection<string>
            {
                "autodetect", "autoinstall", "reformat", "downgrade"
            };
            CurrentInstallMode = currentInstallModeTemp;
            Themes = new ObservableCollection<string>
            {
                "Dark", "Light"
            };
            if (!string.IsNullOrEmpty(AppMan.App.MainSettings.Theme))
                CurrentTheme = AppMan.App.MainSettings.Theme;
            else
                try
                {
                    object v = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", "1");
                    if (v != null && v.ToString() == "0")
                        CurrentTheme = "Dark";
                    else
                        CurrentTheme = "Light";
                }
                catch
                {
                    // ignored
                }

            ReleaseTypes = new ObservableCollection<LauncherPrefs.ReleaseType>
            {
                LauncherPrefs.ReleaseType.Stable,
                LauncherPrefs.ReleaseType.Beta,
                LauncherPrefs.ReleaseType.Alpha
            };

            LogLocation = AppMan.App.MainSettings.LogPath;
            LicenseKey = AppMan.App.MainSettings.LicenseKey;
            DownloadLocation = AppMan.App.DownloadPath;
            CurrentNav = AppMan.App.Settings.CurrentNav;
            ReleaseType = AppMan.App.LauncherPrefs.ReleaseBranch;
            AdvancedModeToggle = CurrentInstallMode != "autodetect";
        }

        public void ReloadSettings()
        {
            CurrentVersion = AppMan.App.SVersion;
            My20Mode = AppMan.App.Settings.My20v2;
        }

        private async void ApplySettingsAction()
        {
            if (ReleaseType != _currentReleaseType)
            {
                if (await UIHelper.ShowDialog(LM.GetValue("MessageBox.ChangeApplicationReleaseBranch"), LM.GetValue("String.Notice"), LM.GetValue("String.No"),
                    LM.GetValue("String.Yes")).ShowAsync() == ContentDialogResult.Primary)
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
                        await UIHelper.ShowErrorDialog(e.GetFullMessage()).ShowAsync();
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

        private async Task SelectPathAction(string type)
        {
            string oldPath = type == "downloads" ? AppMan.App.MainSettings.DownloadPath : AppMan.App.MainSettings.LogPath;

            VistaFolderBrowserDialog dialog = new();
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                if (Directory.Exists(oldPath) && oldPath != dialog.SelectedPath)
                    if (await UIHelper.ShowDialog(string.Format(LM.GetValue("MessageBox.DownloadPathChangeCopy"),
                            Environment.NewLine + oldPath + Environment.NewLine,
                            Environment.NewLine + dialog.SelectedPath + Environment.NewLine), "Syn3 Updater", LM.GetValue("String.No"), LM.GetValue("String.Yes")).ShowAsync() ==
                        ContentDialogResult.Primary)
                        try
                        {
                            if (type == "downloads")
                                foreach (string file in Directory.GetFiles(oldPath, "*.TAR.GZ"))
                                    FileSystem.MoveFile(file, Path.Combine(dialog.SelectedPath, Path.GetFileName(file)), UIOption.AllDialogs);
                            else
                                foreach (string file in Directory.GetFiles(oldPath, "*.txt"))
                                    FileSystem.MoveFile(file, Path.Combine(dialog.SelectedPath, Path.GetFileName(file)), UIOption.AllDialogs);
                        }
                        catch (OperationCanceledException)
                        {
                            //TODO Catch better
                        }

                if (type == "downloads")
                    DownloadLocation = dialog.SelectedPath + "\\";
                else
                    LogLocation = dialog.SelectedPath + "\\";
            }
        }

        public async Task UpdateMy20Toggle(bool? ison)
        {
            if (AdvancedModeToggle)
                if(await UIHelper.ShowDialog(LM.GetValue("MessageBox.My20Detected"), LM.GetValue("String.Warning") + "!", LM.GetValue("Download.CancelButton"),
                    LM.GetValue("String.Yes"), null, ContentDialogButton.None, Brushes.DarkRed).ShowAsync() == ContentDialogResult.Primary)
                    My20Mode = false;

            CurrentInstallMode = "autodetect";
        }

        public async Task UpdateAdvancedModeToggle(bool ison)
        {
            if (ison)
            {
                if (CurrentInstallMode != "autodetect" || await UIHelper.ShowWarningDialog(LM.GetValue("MessageBox.AdvancedSettings"), LM.GetValue("String.Warning") + "!", LM.GetValue("Download.CancelButton"),
                    LM.GetValue("String.Yes")).ShowAsync() == ContentDialogResult.Primary)
                {
                    AdvancedModeToggle = true;
                }
                else
                {
                    AdvancedModeToggle = false;
                }
            }
        }

        #endregion
    }
}