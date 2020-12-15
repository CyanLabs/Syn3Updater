//using DiscordRPC;
using Newtonsoft.Json;

using Syn3Updater.Helper;
using Syn3Updater.Model;
using Syn3Updater.UI;
using Syn3Updater.UI.Tabs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Syn3Updater
{
    public class ApplicationManager
    {
        public static ApplicationManager Instance { get; } = new ApplicationManager();

        public MainWindow MainWindow;
        public bool Skipcheck;

        public void FireLanguageChangedEvent()
        {
            LanguageChangedEvent?.Invoke(this, new EventArgs());
        }

        public event EventHandler LanguageChangedEvent;

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

            if (!Directory.Exists(Properties.Settings.Default.DownloadLocation) && Properties.Settings.Default.DownloadLocation != "")
            {
                Directory.CreateDirectory(Properties.Settings.Default.DownloadLocation);
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
