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
            Settings = new JsonSettings();
        }

        public static readonly SimpleLogger Logger = new SimpleLogger();
        public ObservableCollection<SModel.Ivsu> Ivsus = new ObservableCollection<SModel.Ivsu>();
        public ObservableCollection<SModel.Ivsu> ExtraIvsus = new ObservableCollection<SModel.Ivsu>();
        public static AppMan App { get; } = new AppMan();
        public LauncherPrefs LauncherPrefs { get; set; }
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
            ConfigFolderPath,
            Header,
            LauncherConfigFile;

        public bool DownloadOnly, SkipFormat, IsDownloading, UtilityCreateLogStep1Complete, AppsSelected, DownloadToFolder, ModeForced;

        public int AppUpdated = 0;
        #endregion

        #region Methods

        public void SaveSettings()
        {
            string json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
            File.WriteAllText(ConfigFile, json);
        }

        public void ResetSettings()
        {
            try
            {
                if (File.Exists(ConfigFile)) File.Delete(ConfigFile);
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
            LauncherConfigFile = ConfigFolderPath + "\\launcherPrefs.json";
            if (!Directory.Exists(ConfigFolderPath)) Directory.CreateDirectory(ConfigFolderPath);
            try
            {
                File.WriteAllText(ConfigFolderPath + "\\launcherPrefs.json", json);
            }
            catch (IOException e)
            {
                Logger.Debug(e.GetFullMessage());
                ModernWpf.MessageBox.Show(e.GetFullMessage(), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Initialize()
        {
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

            if (File.Exists(ConfigFile))
            {
                try
                {
                    Settings = JsonConvert.DeserializeObject<JsonSettings>(File.ReadAllText(ConfigFile));
                }
                catch (JsonReaderException)
                {
                    File.Delete(ConfigFile);
                    Settings = new JsonSettings();
                }
            }
            else
            {
                Logger.Debug("No settings file found, initializing JSON settings");
                Settings = new JsonSettings();
            }

            if (File.Exists(ConfigFolderPath + "\\launcherPrefs.json"))
            {
                try
                {
                    LauncherPrefs = JsonConvert.DeserializeObject<LauncherPrefs>(File.ReadAllText(ConfigFolderPath + "\\launcherPrefs.json"));
                }
                catch (JsonReaderException)
                {
                    File.Delete(ConfigFolderPath + "\\launcherPrefs.json");
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
            DownloadPath = App.Settings.DownloadPath;

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
                DownloadPath = App.Settings.DownloadPath;
            }
            catch (IOException)
            {
                string downloads = SystemHelper.GetPath(SystemHelper.KnownFolder.Downloads);
                Logger.Debug($"Download location was invalid, defaulting to {downloads}\\Syn3Updater\\");
                Settings.DownloadPath = $@"{downloads}\Syn3Updater\";
                DownloadPath = App.Settings.DownloadPath;
            }

            string version = App.Settings.CurrentVersion.ToString();
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

            Randomize();

            Logger.Debug("Launching main window");
            if (_mainWindow == null) _mainWindow = new MainWindow();
            if (!_mainWindow.IsVisible) _mainWindow.Show();
            if (_mainWindow.WindowState == WindowState.Minimized) _mainWindow.WindowState = WindowState.Normal;
        }

        public void Randomize()
        {
            List<string> header = new List<string>();
            var rand = new Random();
            string result = Client.GetStringAsync(Api.HeaderURL).Result;
            Api.Headers UAs = JsonConvert.DeserializeObject<Api.Headers>(result);

            foreach (Api.Header ua in UAs.Header)
                header.Add(ua.Ua.Replace("[PLACEHOLDER]", rand.Next(ua.Min, ua.Max).ToString()));

            int index = rand.Next(header.Count);
            Header = header[index];
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