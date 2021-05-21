//using DiscordRPC;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Windows;
using Cyanlabs.Syn3Updater.Helper;
using Cyanlabs.Syn3Updater.Model;
using Cyanlabs.Syn3Updater.UI;
using Newtonsoft.Json;
using Cyanlabs.Updater.Common;

namespace Cyanlabs.Syn3Updater
{
    public class AppMan
    {
        #region Constructors

        private AppMan()
        {
            LauncherPrefs = new LauncherPrefs();
            MainSettings = new JsonMainSettings();
        }

        public static readonly SimpleLogger Logger = new SimpleLogger();
        public ObservableCollection<SModel.Ivsu> Ivsus = new ObservableCollection<SModel.Ivsu>();
        public ObservableCollection<SModel.Ivsu> ExtraIvsus = new ObservableCollection<SModel.Ivsu>();
        public static AppMan App { get; } = new AppMan();
        public LauncherPrefs LauncherPrefs { get; set; }
        public JsonMainSettings MainSettings { get; set; }
        public JsonSettings Settings { get; set; }

        public readonly HttpClient Client = new HttpClient();

        #endregion

        #region Events

        public void FireLanguageChangedEvent()
        {
            LanguageChangedEvent?.Invoke(this, EventArgs.Empty);
        }

        public void FireDownloadsTabEvent()
        {
            ShowDownloadsTab?.Invoke(this, EventArgs.Empty);
        }

        public void FireHomeTabEvent()
        {
            ShowHomeTab?.Invoke(this, EventArgs.Empty);
        }

        public void FireUtilityTabEvent()
        {
            ShowUtilityTab?.Invoke(this, EventArgs.Empty);
        }

        public void FireSettingsTabEvent()
        {
            ShowSettingsTab?.Invoke(this, EventArgs.Empty);
        }

        public void FireNewsTabEvent()
        {
            ShowNewsTab?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler LanguageChangedEvent;

        public event EventHandler ShowDownloadsTab;

        public event EventHandler ShowHomeTab;

        public event EventHandler ShowUtilityTab;

        public event EventHandler ShowSettingsTab;

        public event EventHandler ShowNewsTab;
        #endregion

        #region Properties & Fields

        private MainWindow _mainWindow;

        public string DownloadPath,
            DriveName,
            DrivePartitionType,
            DriveFileSystem,
            DriveNumber,
            DriveLetter,
            SelectedMapVersion,
            SelectedRelease,
            SelectedRegion,
            InstallMode,
            SVersion,
            Action,
            ConfigFile,
            ProfileFile,
            CommonConfigFolderPath,
            UserConfigFolderPath,
            ProfileConfigFolderPath,
            Header,
            LauncherConfigFile,
            Magnet;

        public bool DownloadOnly, SkipFormat, IsDownloading, UtilityCreateLogStep1Complete, AppsSelected, DownloadToFolder, ModeForced, Cancelled;

        public int AppUpdated = 0;
        #endregion

        #region Methods

        public void LoadProfile()
        {
            if (string.IsNullOrEmpty(MainSettings.Profile)) MainSettings.Profile = "default";
            ProfileFile = ProfileConfigFolderPath + $"{MainSettings.Profile}.json";
            if (File.Exists(ProfileFile))
            {
                try
                {
                    Settings = JsonConvert.DeserializeObject<JsonSettings>(File.ReadAllText(ProfileFile));
                }
                catch (JsonReaderException)
                {
                    File.Delete(ProfileFile);
                    Settings = new JsonSettings();
                }
                Logger.Debug($"Loaded Profile: {MainSettings.Profile}");
            }
            else
            {
                Logger.Debug("No Profile Settings file found, initializing JSON settings");
                Settings = new JsonSettings();
                Settings.CurrentNav = MainSettings.CurrentNav;
                Settings.CurrentRegion = MainSettings.CurrentRegion;
                Settings.CurrentVersion = MainSettings.CurrentVersion;
                Settings.My20 = MainSettings.My20;
                SaveSettings();
            }
            
            string version = App.Settings.CurrentVersion.ToString() ;
            string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            Logger.Debug($"Current Version set to {version}, Decimal seperator set to {decimalSeparator}");
            try
            {
                if (version.Length == 7)
                    SVersion = $"{version[0]}{decimalSeparator}{version[1]}{decimalSeparator}{version.Substring(2, version.Length - 2)}";
                else
                    SVersion = $"0{decimalSeparator}0{decimalSeparator}00000";
            }
            catch (IndexOutOfRangeException e)
            {
                Logger.Debug(e.GetFullMessage());
            }
        }

        public void SaveSettings()
        {
            string mainJson = JsonConvert.SerializeObject(MainSettings, Formatting.Indented);
            File.WriteAllText(ConfigFile, mainJson);
            string profileJson = JsonConvert.SerializeObject(Settings, Formatting.Indented);
            File.WriteAllText(ProfileFile, profileJson);
        }

        public void ResetSettings()
        {
            try
            {
                if (File.Exists(ConfigFile)) File.Delete(ConfigFile);
                if (File.Exists(ProfileFile)) File.Delete(ProfileFile);
                if (File.Exists(LauncherConfigFile)) File.Delete(LauncherConfigFile);
            }
            catch (Exception e)
            {
                Logger.Debug(e.GetFullMessage());
            }
        }

        public void UpdateLauncherSettings()
        {
            string json = JsonConvert.SerializeObject(LauncherPrefs);
            LauncherConfigFile = CommonConfigFolderPath + "\\launcherPrefs.json";
            if (!Directory.Exists(CommonConfigFolderPath)) Directory.CreateDirectory(CommonConfigFolderPath);
            try
            {
                File.WriteAllText(CommonConfigFolderPath + "\\launcherPrefs.json", json);
            }
            catch (IOException e)
            {
                Logger.Debug(e.GetFullMessage());
                ModernWpf.MessageBox.Show(e.GetFullMessage(), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Initialize()
        {
            SystemHelper.WriteRegistryHandler();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Logger.Debug($"Syn3 Updater {Assembly.GetEntryAssembly()?.GetName().Version} ({LauncherPrefs.ReleaseTypeInstalled}) is Starting");
            string[] args = Environment.GetCommandLineArgs();
            if (!args.Contains("/launcher"))
            {
                try
                {
                    Process.Start("Launcher.exe");
                    Application.Current.Shutdown();
                }
                catch (Win32Exception e)
                {
                    Logger.Debug("Something went wrong launching 'Launcher.exe', skipping launcher!");
                    Logger.Debug(e.GetFullMessage());
                }
            }

            foreach (string value in args)
            {
                if (value.Contains("syn3updater://")) Magnet = value.Replace("syn3updater://", "").TrimEnd('/');
            }

            CommonConfigFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\CyanLabs\\Syn3Updater";
            UserConfigFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\CyanLabs\\Syn3Updater";
            ProfileConfigFolderPath = UserConfigFolderPath + "\\Profiles\\";
            ConfigFile = UserConfigFolderPath + "\\settings.json";
            
            if (!Directory.Exists(CommonConfigFolderPath)) Directory.CreateDirectory(CommonConfigFolderPath);
            if (!Directory.Exists(UserConfigFolderPath)) Directory.CreateDirectory(UserConfigFolderPath);
            if (!Directory.Exists(ProfileConfigFolderPath)) Directory.CreateDirectory(ProfileConfigFolderPath);
            
            if (File.Exists(ConfigFile))
            {
                try
                {
                    MainSettings = JsonConvert.DeserializeObject<JsonMainSettings>(File.ReadAllText(ConfigFile));
                }
                catch (JsonReaderException)
                {
                    File.Delete(ConfigFile);
                    MainSettings = new JsonMainSettings();
                }
            }
            else if (File.Exists(CommonConfigFolderPath + "\\settings.json"))
            {
                File.Move(CommonConfigFolderPath + "\\settings.json",ConfigFile);
                try
                {
                    MainSettings = JsonConvert.DeserializeObject<JsonMainSettings>(File.ReadAllText(ConfigFile));
                }
                catch (JsonReaderException)
                {
                    File.Delete(ConfigFile);
                    MainSettings = new JsonMainSettings();
                }
            }
            else
            {
                Logger.Debug("No Main Settings file found, initializing JSON settings");
                MainSettings = new JsonMainSettings();
            }

            LoadProfile();

            if (string.IsNullOrEmpty(MainSettings.LogPath))
            {
                MainSettings.LogPath = UserConfigFolderPath + "\\Logs\\";
                if (!Directory.Exists(MainSettings.LogPath)) Directory.CreateDirectory(MainSettings.LogPath);
            }
            
            if (File.Exists(CommonConfigFolderPath + "\\launcherPrefs.json"))
            {
                try
                {
                    LauncherPrefs = JsonConvert.DeserializeObject<LauncherPrefs>(File.ReadAllText(CommonConfigFolderPath + "\\launcherPrefs.json"));
                }
                catch (JsonReaderException)
                {
                    File.Delete(CommonConfigFolderPath + "\\launcherPrefs.json");
                    LauncherPrefs = new LauncherPrefs();
                }
            }
            else
            {
                LauncherPrefs = new LauncherPrefs();
            }

            // ReSharper disable once IdentifierTypo
            // ReSharper disable once UnusedVariable
            Logger.Debug("Determining language to use for the application");
            List<LanguageModel> langs = LM.Languages;
            CultureInfo ci = CultureInfo.InstalledUICulture;
            if (string.IsNullOrWhiteSpace(MainSettings.Lang))
            {
                Logger.Debug($"Language is not set, inferring language from system culture. Lang={ci.TwoLetterISOLanguageName}");
                MainSettings.Lang = ci.TwoLetterISOLanguageName;
            }

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(MainSettings.Lang);
            Logger.Debug($"Language is set to {MainSettings.Lang}");

            if (string.IsNullOrWhiteSpace(MainSettings.DownloadPath))
            {
                string downloads = SystemHelper.GetPath(SystemHelper.KnownFolder.Downloads);
                Logger.Debug($"Download location is not set, defaulting to {downloads}\\Syn3Updater\\");
                MainSettings.DownloadPath = $@"{downloads}\Syn3Updater\";
            }

            Logger.Debug("Determining download path to use for the application");
            DownloadPath = App.MainSettings.DownloadPath;

            try
            {
                if (!Directory.Exists(DownloadPath))
                {
                    Logger.Debug("Download location does not exist");
                    Directory.CreateDirectory(DownloadPath);
                }

                Logger.Debug($"Download path is set to {DownloadPath}");
            }
            catch (DirectoryNotFoundException)
            {
                string downloads = SystemHelper.GetPath(SystemHelper.KnownFolder.Downloads);
                Logger.Debug($"Download location was invalid, defaulting to {downloads}\\Syn3Updater\\");
                MainSettings.DownloadPath = $@"{downloads}\Syn3Updater\";
                DownloadPath = App.MainSettings.DownloadPath;
            }
            catch (IOException)
            {
                string downloads = SystemHelper.GetPath(SystemHelper.KnownFolder.Downloads);
                Logger.Debug($"Download location was invalid, defaulting to {downloads}\\Syn3Updater\\");
                MainSettings.DownloadPath = $@"{downloads}\Syn3Updater\";
                DownloadPath = App.MainSettings.DownloadPath;
            }

            Randomize();
            App.SaveSettings();
            Logger.Debug("Launching main window");
            if (_mainWindow == null) _mainWindow = new MainWindow();
            if (!_mainWindow.IsVisible) _mainWindow.Show();
            if (_mainWindow.WindowState == WindowState.Minimized) _mainWindow.WindowState = WindowState.Normal;
        }

        public void Randomize()
        {
            try
            {
                Random rand = new();
                string result = Client.GetStringAsync(Api.HeaderURL).Result;
                Api.Headers UAs = JsonConvert.DeserializeObject<Api.Headers>(result);

                List<string> header = UAs.Header.Select(ua => ua.Ua.Replace("[PLACEHOLDER]", rand.Next(ua.Min, ua.Max).ToString())).ToList();

                int index = rand.Next(header.Count);
                Header = header[index];
            }
            catch (Exception e)
            {
                Logger.Debug($"Unable to access CyanLabs API, defaulting UserAgent");
                Header = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:70.0) Gecko/20100101 Firefox/86.0";
            }

        }

        public void RestartApp()
        {
            Logger.Debug($"Syn3 Updater {Assembly.GetEntryAssembly()?.GetName().Version} is Restarting");
            Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        public void Exit()
        {
            Logger.Debug("Saving settings before shutdown");
            try
            {
                App.SaveSettings();
            }
            catch (Exception e)
            {
                Logger.Debug(e.GetFullMessage());
            }

            Logger.Debug("Writing log to disk before shutdown");
            try
            {
                File.WriteAllText(UserConfigFolderPath + "\\applog.txt", JsonConvert.SerializeObject(Logger.Log));
            }
            catch (UnauthorizedAccessException e)
            {
                Logger.Debug(e.GetFullMessage());
            }
            catch (IOException e)
            {
                Logger.Debug(e.GetFullMessage());
            }

            Logger.Debug("Syn3 Updater is shutting down");
            Application.Current.Shutdown();
        }

        #endregion
    }
}