//using DiscordRPC;
using Newtonsoft.Json;

using Syn3Updater.Helper;
using Syn3Updater.Model;
using Syn3Updater.UI;
using Syn3Updater.UI.Tabs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;

namespace Syn3Updater
{
    public class ApplicationManager
    {
        public ObservableCollection<HomeViewModel.Ivsu> _ivsus = new ObservableCollection<HomeViewModel.Ivsu>();
        public static ApplicationManager Instance { get; } = new ApplicationManager();

        public MainWindow MainWindow;
        public bool Skipcheck, _downloadonly, SkipFormat, downloading;
        public string DownloadLocation, drivename, drivepartitiontype, drivefilesystem;
        public string drivenumber, driveletter, selectedmapversion, selectedrelease, selectedregion, InstallMode;

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

        public void FireSettingsTabEvent()
        {
            ShowSettingsTab?.Invoke(this, new EventArgs());
        }

        public event EventHandler LanguageChangedEvent;

        public event EventHandler ShowDownloadsTab;

        public event EventHandler ShowHomeTab;

        public event EventHandler ShowSettingsTab;

        #region Constructors

        private ApplicationManager()
        {
            
        }

        public static readonly SimpleLogger Logger = new SimpleLogger();
        #endregion

        #region Methods
        public void Initialize()
        {
            Logger.Debug("============ Syn3 Updater is Starting ============");

            // ReSharper disable once IdentifierTypo
            List<LanguageModel> langs = LanguageManager.Languages;
            CultureInfo ci = CultureInfo.InstalledUICulture;
            if (Properties.Settings.Default.Lang == null)
            {
                Logger.Debug("Language is not set, inferring language from system culture. Lang=" + ci.TwoLetterISOLanguageName);
                Properties.Settings.Default.Lang = ci.TwoLetterISOLanguageName;
            }

            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Syn3Updater.Properties.Settings.Default.Lang);

            //client = new DiscordRpcClient("");
            //client.Initialize();

            DownloadLocation = Properties.Settings.Default.DownloadLocation == ""
                ? KnownFolders.GetPath(KnownFolder.Downloads) + @"\Syn3Updater\"
                : Properties.Settings.Default.DownloadLocation;

            if (!Directory.Exists(DownloadLocation) && DownloadLocation != "")
            {
                Directory.CreateDirectory(DownloadLocation);
            }

            foreach (string arg in Environment.GetCommandLineArgs())
               switch (arg)
               {
                   case "/updated":
                       Properties.Settings.Default.Upgrade();
                       Properties.Settings.Default.Save();
                       break;
                   case "/debug":
                       Skipcheck = true;
                       break;
               }

            if (MainWindow == null) MainWindow = new MainWindow();
            if (!MainWindow.IsVisible)
            {
                MainWindow.Show();
            }

            if (MainWindow.WindowState == WindowState.Minimized)
            {
                MainWindow.WindowState = WindowState.Normal;
            }

            
        }

        public void RestartApp()
        {
            Logger.Debug("App is restarting.");
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        public void Exit()
        {
            Logger.Debug("============ App is Shutting Down ============");
            Application.Current.Shutdown();
        }
        #endregion
    }
}
