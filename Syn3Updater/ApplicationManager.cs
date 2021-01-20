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
using SharedCode;
using MessageBox = Cyanlabs.Syn3Updater.UI.MessageBox.MessageBox;

namespace Cyanlabs.Syn3Updater
{
    public class ApplicationManager
    {
        #region Constructors

        private ApplicationManager()
        {
            LauncherPrefs = new LauncherPrefs();
            Settings = new JsonSettings();
        }

        public static readonly SimpleLogger Logger = new SimpleLogger();
        public ObservableCollection<SyncModel.SyncIvsu> Ivsus = new ObservableCollection<SyncModel.SyncIvsu>();
        public static ApplicationManager Instance { get; } = new ApplicationManager();
        public LauncherPrefs LauncherPrefs { get; set; }
        public JsonSettings Settings { get; set; }

        public readonly HttpClient Client = new HttpClient();

        #endregion

        #region Events

        public void FireLanguageChangedEvent()
        {
            LanguageChangedEvent?.Invoke(this, new EventArgs());
        }

        public void FireDownloadsTabEvent()
        {
            ShowDownloadsTab?.Invoke(this, new EventArgs());
        }

        public void FireHomeTabEvent()
        {
            ShowHomeTab?.Invoke(this, new EventArgs());
        }

        public void FireUtilityTabEvent()
        {
            ShowUtilityTab?.Invoke(this, new EventArgs());
        }

        public void FireSettingsTabEvent()
        {
            ShowSettingsTab?.Invoke(this, new EventArgs());
        }


        public event EventHandler LanguageChangedEvent;

        public event EventHandler ShowDownloadsTab;

        public event EventHandler ShowHomeTab;

        public event EventHandler ShowUtilityTab;

        public event EventHandler ShowSettingsTab;

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
            SyncVersion,
            Action,
            ConfigFile,
            ConfigFolderPath;

        public bool DownloadOnly, SkipFormat, IsDownloading, UtilityCreateLogStep1Complete, AppsSelected;
        #endregion

        #region Methods

        public void SaveSettings()
        {
            string json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
            File.WriteAllText(ConfigFile, json);
        }

        public void ResetSettings()
        {
            if (File.Exists(ConfigFile)) File.Delete(ConfigFile);
        }

        public void UpdateLauncherSettings()
        {
            string json = JsonConvert.SerializeObject(LauncherPrefs);
            if (!Directory.Exists(ConfigFolderPath)) Directory.CreateDirectory(ConfigFolderPath);
            try
            {
                File.WriteAllText(ConfigFolderPath + "\\launcherPrefs.json", json);
            }
            catch (IOException e)
            {
                Logger.Debug(e.GetFullMessage());
                MessageBox.Show(e.GetFullMessage(), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Initialize()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Logger.Debug($"Syn3 Updater {Assembly.GetEntryAssembly()?.GetName().Version} ({LauncherPrefs.ReleaseTypeInstalled}) is Starting");

            if (!Environment.GetCommandLineArgs().Contains("/launcher"))
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

            ConfigFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\CyanLabs\\Syn3Updater";
            ConfigFile = ConfigFolderPath + "\\settings.json";
            if (!Directory.Exists(ConfigFolderPath)) Directory.CreateDirectory(ConfigFolderPath);

            if (!File.Exists(ConfigFile))
            {
                Logger.Debug("No settings file found, initializing JSON settings");
                Settings = new JsonSettings();
            }
            else
            {
                Settings = JsonConvert.DeserializeObject<JsonSettings>(File.ReadAllText(ConfigFile));
            }

            LauncherPrefs = File.Exists(ConfigFolderPath + "\\launcherPrefs.json")
                ? JsonConvert.DeserializeObject<LauncherPrefs>(File.ReadAllText(ConfigFolderPath + "\\launcherPrefs.json"))
                : new LauncherPrefs();

            // ReSharper disable once IdentifierTypo
            // ReSharper disable once UnusedVariable
            Logger.Debug("Determining language to use for the application");
            List<LanguageModel> langs = LanguageManager.Languages;
            CultureInfo ci = CultureInfo.InstalledUICulture;
            if (string.IsNullOrWhiteSpace(Settings.Lang))
            {
                Logger.Debug($"Language is not set, inferring language from system culture. Lang={ci.TwoLetterISOLanguageName}");
                Settings.Lang = ci.TwoLetterISOLanguageName;
            }

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Lang);
            Logger.Debug($"Language is set to {Settings.Lang}");

            if (string.IsNullOrWhiteSpace(Settings.DownloadPath))
            {
                string downloads = SystemHelper.GetPath(SystemHelper.KnownFolder.Downloads);
                Logger.Debug($"Download location is not set, defaulting to {downloads}\\Syn3Updater\\");
                Settings.DownloadPath = $@"{downloads}\Syn3Updater\";
            }

            Logger.Debug("Determining download path to use for the application");
            DownloadPath = Instance.Settings.DownloadPath;

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
                Settings.DownloadPath = $@"{downloads}\Syn3Updater\";
                DownloadPath = Instance.Settings.DownloadPath;
            }
            catch (IOException)
            {
                string downloads = SystemHelper.GetPath(SystemHelper.KnownFolder.Downloads);
                Logger.Debug($"Download location was invalid, defaulting to {downloads}\\Syn3Updater\\");
                Settings.DownloadPath = $@"{downloads}\Syn3Updater\";
                DownloadPath = Instance.Settings.DownloadPath;
            }

            string version = Instance.Settings.CurrentSyncVersion.ToString();
            string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            Logger.Debug($"Current Sync Version set to {version}, Decimal seperator set to {decimalSeparator}");
            try
            {
                if (version.Length == 7)
                    SyncVersion = $"{version[0]}{decimalSeparator}{version[1]}{decimalSeparator}{version.Substring(2, version.Length - 2)}";
                else
                    SyncVersion = $"0{decimalSeparator}0{decimalSeparator}00000";
            }
            catch (IndexOutOfRangeException e)
            {
                Logger.Debug(e.GetFullMessage());
            }


            Logger.Debug("Launching main window");
            if (_mainWindow == null) _mainWindow = new MainWindow();
            if (!_mainWindow.IsVisible) _mainWindow.Show();
            if (_mainWindow.WindowState == WindowState.Minimized) _mainWindow.WindowState = WindowState.Normal;
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
                Instance.SaveSettings();
            }
            catch (Exception e)
            {
                Logger.Debug(e.GetFullMessage());
            }

            Logger.Debug("Writing log to disk before shutdown");
            try
            {
                File.WriteAllText(ConfigFolderPath + "\\log.txt", JsonConvert.SerializeObject(Logger.Log));
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