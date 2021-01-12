//using DiscordRPC;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using SharedCode;
using Syn3Updater.Helper;
using Syn3Updater.Model;
using Syn3Updater.UI;

namespace Syn3Updater
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
            configFolderPath;

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
           if(File.Exists(ConfigFile)) File.Delete(ConfigFile);
        }

        public void UpdateLaucherSettings()
        {
            string json = JsonConvert.SerializeObject(LauncherPrefs);
            if (!Directory.Exists(configFolderPath)) Directory.CreateDirectory(configFolderPath);
            try
            {
                File.WriteAllText(configFolderPath + "\\launcherPrefs.json", json);
            }
            catch (IOException e)
            {
                Logger.Debug(e.GetFullMessage());
                UI.MessageBox.MessageBox.Show(e.GetFullMessage(), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void Initialize()
        {
            if (!Environment.GetCommandLineArgs().Contains("/launcher") && !Debugger.IsAttached)
            {
                try
                {
                    Process.Start("Launcher.exe");
                    Application.Current.Shutdown();
                }
                catch (System.ComponentModel.Win32Exception e)
                {
                    Logger.Debug("Something went wrong launching 'Launcher.exe', skipping launcher!");
                    Logger.Debug(e.GetFullMessage());
                }
            }
            Logger.Debug($"Syn3 Updater {Assembly.GetEntryAssembly()?.GetName().Version} is Starting");
            foreach (string arg in Environment.GetCommandLineArgs())
                switch (arg)
                {
                    case "/updated":
                        Logger.Debug("/updated - flag no longer used but noted for debug purposes");
                        break;
                }
            

            configFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\CyanLabs\\Syn3Updater";
            ConfigFile = configFolderPath + "\\settings.json";
            if (!Directory.Exists(configFolderPath))
            {
                Directory.CreateDirectory(configFolderPath);
            }

            if (!File.Exists(ConfigFile))
            {
                Logger.Debug("No settings file found, initializing JSON settings");
                Settings = new JsonSettings();
            }
            else
            {
                Settings = JsonConvert.DeserializeObject<JsonSettings>(File.ReadAllText(ConfigFile));
            }

            LauncherPrefs = File.Exists(configFolderPath + "\\launcherPrefs.json") ? JsonConvert.DeserializeObject<LauncherPrefs>(File.ReadAllText(configFolderPath + "\\launcherPrefs.json")) : new LauncherPrefs();

            // ReSharper disable once IdentifierTypo
            // ReSharper disable once UnusedVariable
            Logger.Debug($"Determining language to use for the application");
            List<LanguageModel> langs = LanguageManager.Languages;
            CultureInfo ci = CultureInfo.InstalledUICulture;
            if (string.IsNullOrWhiteSpace(Settings.Lang))
            {
                Logger.Debug($"Language is not set, inferring language from system culture. Lang={ci.TwoLetterISOLanguageName}");
                Settings.Lang = ci.TwoLetterISOLanguageName;
            }

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Lang);
            Logger.Debug($"Language is set to {Settings.Lang}");
            //client = new DiscordRpcClient("");
            //client.Initialize();

            if (string.IsNullOrWhiteSpace(Settings.DownloadPath))
            {
                string downloads = SystemHelper.GetPath(SystemHelper.KnownFolder.Downloads);
                Logger.Debug($"Download location is not set, defaulting to {downloads}\\Syn3Updater\\");
                Settings.DownloadPath = $@"{downloads}\Syn3Updater\";
            }

            Logger.Debug($"Determining download path to use for the application");
            DownloadPath = ApplicationManager.Instance.Settings.DownloadPath;

            try
            {
                if (!Directory.Exists(DownloadPath))
                {
                    Logger.Debug("Download location does not exist");
                    Directory.CreateDirectory(DownloadPath);
                }
            
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
                DownloadPath = ApplicationManager.Instance.Settings.DownloadPath;
            }
            catch (IOException)
            {
                string downloads = SystemHelper.GetPath(SystemHelper.KnownFolder.Downloads);
                Logger.Debug($"Download location was invalid, defaulting to {downloads}\\Syn3Updater\\");
                Settings.DownloadPath = $@"{downloads}\Syn3Updater\";
                DownloadPath = ApplicationManager.Instance.Settings.DownloadPath;
            }

            string version = ApplicationManager.Instance.Settings.CurrentSyncVersion.ToString();
            string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            Logger.Debug($"Current Sync Version set to {version}, Decimal seperator set to {decimalSeparator}");
            try
            {
                if (version.Length == 7)
                {
                    SyncVersion = $"{version[0]}{decimalSeparator}{version[1]}{decimalSeparator}{version.Substring(2, version.Length - 2)}";
                }
                else
                {
                    SyncVersion = $"0{decimalSeparator}0{decimalSeparator}00000";
                }
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
                ApplicationManager.Instance.SaveSettings();
            }
            catch (Exception e)
            {
                Logger.Debug(e.GetFullMessage());
            }
            Logger.Debug("Writing log to disk before shutdown");
            try
            {
                File.WriteAllText(configFolderPath + "\\log.txt", JsonConvert.SerializeObject(Logger.Log));
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