//using DiscordRPC;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using Newtonsoft.Json;
using Syn3Updater.Helper;
using Syn3Updater.Model;
using Syn3Updater.Properties;
using Syn3Updater.UI;

namespace Syn3Updater
{
    public class ApplicationManager
    {
        #region Constructors

        private ApplicationManager()
        {
        }

        public static readonly SimpleLogger Logger = new SimpleLogger();
        public ObservableCollection<SyncModel.SyncIvsu> Ivsus = new ObservableCollection<SyncModel.SyncIvsu>();
        public static ApplicationManager Instance { get; } = new ApplicationManager();

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
            Action;

        public bool SkipCheck, DownloadOnly, SkipFormat, IsDownloading, UtilityCreateLogStep1Complete, AppsSelected;

        #endregion

        #region Methods
        public void Initialize()
        {
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

            if (!Debugger.IsAttached) { AutoUpdaterHelper autoupdaterhelper = new AutoUpdaterHelper(); }
            
            Logger.Debug("[App] Syn3 Updater is Starting");
            // ReSharper disable once IdentifierTypo
            // ReSharper disable once UnusedVariable

            List<LanguageModel> langs = LanguageManager.Languages;
            CultureInfo ci = CultureInfo.InstalledUICulture;
            if (string.IsNullOrWhiteSpace(Settings.Default.Lang))
            {
                Logger.Debug($"[Settings]  Language is not set, inferring language from system culture. Lang={ci.TwoLetterISOLanguageName}");
                Settings.Default.Lang = ci.TwoLetterISOLanguageName;
            }

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.Lang);

            //client = new DiscordRpcClient("");
            //client.Initialize();

            if (string.IsNullOrWhiteSpace(Settings.Default.DownloadPath))
            {
                string downloads = SystemHelper.GetPath(SystemHelper.KnownFolder.Downloads);
                Logger.Debug($"[Settings] Download location is not set, defaulting to {downloads}\\Syn3Updater\\");
                Settings.Default.DownloadPath = $@"{downloads}\Syn3Updater\";
            }

            DownloadPath = Settings.Default.DownloadPath;

            if (!Directory.Exists(DownloadPath) && DownloadPath != "")
            {
                Logger.Debug("[App] Download location does not exist");
                Directory.CreateDirectory(DownloadPath);
            }

            foreach (string arg in Environment.GetCommandLineArgs())
                switch (arg)
                {
                    case "/updated":
                        Logger.Debug("[App] /updated detected, upgrading settings");
                        Settings.Default.Upgrade();
                        Settings.Default.Save();
                        break;
                    case "/debug":
                        Logger.Debug("[App] /debug flag detected, skipping all verification steps");
                        SkipCheck = true;
                        break;
                }



            string version = Settings.Default.CurrentSyncVersion.ToString();
            string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            SyncVersion = $"{version[0]}{decimalSeparator}{version[1]}{decimalSeparator}{version.Substring(2, version.Length - 2)}";

            if (_mainWindow == null) _mainWindow = new MainWindow();
            if (!_mainWindow.IsVisible) _mainWindow.Show();
            if (_mainWindow.WindowState == WindowState.Minimized) _mainWindow.WindowState = WindowState.Normal;
        }

        public void RestartApp()
        {
            Logger.Debug("[App] Syn3 Updater is restarting.");
            Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        public void Exit()
        {
            Logger.Debug("[Settings] Saving settings before shutdown");
            Settings.Default.Save();
            Logger.Debug("[App] Syn3 Updater is shutting down");
            File.WriteAllText("log.txt", JsonConvert.SerializeObject(Logger.Log));
            Application.Current.Shutdown();
        }

        #endregion
    }
}