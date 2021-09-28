//using DiscordRPC;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;
using Syn3Updater.Helpers;
using Syn3Updater.Helpers.Windows;
using Syn3Updater.Models;
using Syn3Updater.Views;

namespace Syn3Updater
{
    public class AppMan
    {
        #region Constructors

        private AppMan()
        {
            MainSettings = new MainSettings();
        }
        
        public ObservableCollection<SModel.Ivsu> Ivsus = new();
        public ObservableCollection<SModel.Ivsu> ExtraIvsus = new();
        public static AppMan App { get; } = new();
        public MainSettings? MainSettings { get; set; }
        public ProfileSettings Settings { get; set; }
        public static HttpClient Client = new();
        #endregion

        #region Events
        public void FireDownloadsStartEvent()
        {
            ShowDownloadsTab?.Invoke(this, EventArgs.Empty);
            StartDownloadsTab?.Invoke(this, EventArgs.Empty);
        }

        public void FireHomeTabEvent()
        {
            ShowHomeTab?.Invoke(this, EventArgs.Empty);
        }
        
        public void FireInterrogatorLogCompleted()
        {
            ShowInterrogatorLogCompleted?.Invoke(this, EventArgs.Empty);
        }

        public void FireUtilityTabEvent()
        {
            ShowUtilityTab?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ShowDownloadsTab;
        public event EventHandler StartDownloadsTab;
        public event EventHandler ShowHomeTab;
        public event EventHandler ShowInterrogatorLogCompleted;
        public event EventHandler ShowUtilityTab;

        #endregion

        #region Properties & Fields

        private MainWindow _mainWindow;

        public string DownloadPath,
            DriveName,
            DrivePartitionType,
            DriveFileSystem,
            DriveNumber;

        public string? DriveLetter;

        public string? SelectedMapVersion;

        public string? SelectedRelease;

        public string? SelectedRegion;

        public string? InstallMode;

        public string SVersion;

        public string? Action;

        public string MainConfigFile,
            ProfileFile,
            AppDataPath,
            ProfilePath,
            Header,
            Magnet;

        public bool SkipFormat, IsDownloading, DownloadToFolder, ModeForced, DownloadOnly, Cancelled, AppsSelected;
        
        // Initiate GraphQlClient
        public readonly GraphQLHttpClient GraphQlClient = new(Api.Syn3UpdaterGraphQl, new NewtonsoftJsonSerializer())
        {
            HttpClient = { DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer",ApiSecret.Token)}}
        };

        #endregion

        #region Methods

        public void Initialize()
        {
            if (OperatingSystem.IsWindows())
                WindowsSystemHelper.WriteRegistryHandler();
            else if (OperatingSystem.IsMacOS())
            {
                //TODO MAC OSX Intent
            }
            
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 8;
            ServicePointManager.Expect100Continue = false;
            string[] args = Environment.GetCommandLineArgs();

            //Logger.Debug($"Syn3 Updater {Assembly.GetEntryAssembly()?.GetName().Version} is Starting");
            
            foreach (string value in args)
                if (value.Contains("syn3updater://"))
                    Magnet = value.Replace("syn3updater://", "").TrimEnd('/');

            //Logger.Debug($"URL Intent: {Magnet}");

            if (OperatingSystem.IsWindows())
                AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\CyanLabs\\Syn3Updater\\";
            else if (OperatingSystem.IsMacOS())
                AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Library/Application Support/Syn3Updater/";

            ProfilePath = AppDataPath + "Profiles" + Path.DirectorySeparatorChar;
            MainConfigFile = AppDataPath + "settings.json";

            if (!Directory.Exists(AppDataPath)) Directory.CreateDirectory(AppDataPath);
            if (!Directory.Exists(ProfilePath)) Directory.CreateDirectory(ProfilePath);

            if (File.Exists(MainConfigFile))
            {
                try
                {
                    MainSettings = JsonConvert.DeserializeObject<MainSettings>(File.ReadAllText(MainConfigFile));
                }
                catch (JsonReaderException)
                {
                    File.Delete(MainConfigFile);
                    MainSettings = new MainSettings();
                }
            }
            else
            {
                MainSettings = new MainSettings();
            }

            LoadProfile();

            if (string.IsNullOrEmpty(MainSettings?.LogPath)) MainSettings.LogPath = AppDataPath + "Logs" + Path.DirectorySeparatorChar;
            
            try
            {
                if (!Directory.Exists(MainSettings.LogPath)) Directory.CreateDirectory(MainSettings.LogPath);
            }
            catch (DirectoryNotFoundException)
            {
                MainSettings.LogPath = AppDataPath + "Logs" + Path.DirectorySeparatorChar;
            }

            if (string.IsNullOrWhiteSpace(MainSettings.DownloadPath))
            {
                if (OperatingSystem.IsWindows()) 
                    MainSettings.DownloadPath = $@"{WindowsSystemHelper.GetPath(WindowsSystemHelper.KnownFolder.Downloads)}\Syn3Updater\";
                else if (OperatingSystem.IsMacOS())
                    MainSettings.DownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Downloads/Syn3Updater";
            }
            DownloadPath = App.MainSettings.DownloadPath;

            try
            {
                if (!Directory.Exists(DownloadPath)) Directory.CreateDirectory(DownloadPath);
            }
            catch (Exception)
            {
                if (OperatingSystem.IsWindows())
                    MainSettings.DownloadPath = $@"{WindowsSystemHelper.GetPath(WindowsSystemHelper.KnownFolder.Downloads)}\Syn3Updater\";
                else if (OperatingSystem.IsMacOS())
                    MainSettings.DownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Downloads/Syn3Updater";
                    
                DownloadPath = App.MainSettings.DownloadPath;
            }

            Header = ApiHelper.GetGeneratedUserAgent();
            
            SettingsHelper.SaveMainSettings(App.MainConfigFile,App.MainSettings);
            SettingsHelper.SaveProfileSettings(App.ProfilePath, App.Settings);
            
            Client.DefaultRequestHeaders.UserAgent.TryParseAdd(Header);
        }

        private void LoadProfile()
        {
            if (MainSettings != null && string.IsNullOrEmpty(MainSettings.Profile))
            {
                MainSettings.Profile = "default";
                ProfileFile = ProfilePath + "default.json";
            }
            else if (MainSettings != null)
            {
                ProfileFile = ProfilePath + $"{MainSettings.Profile}.json";
            }
            
            if (File.Exists(ProfileFile))
            {
                try
                {
                    Settings = JsonConvert.DeserializeObject<ProfileSettings>(File.ReadAllText(ProfileFile));
                }
                catch (JsonReaderException)
                {
                    File.Delete(ProfileFile);
                    Settings = new ProfileSettings();
                }
            }
            else
            {
                Settings = new ProfileSettings();
            }

            string version = App.Settings.CurrentVersion.ToString();
            try
            {
                SVersion = version.Length == 7
                    ? $"{version[0]}.{version[1]}.{version.Substring(2, version.Length - 2)}"
                    : "0.0.00000";
            }
            catch (IndexOutOfRangeException e)
            {
                // ignored
            }
        }

        public static void Exit()
        {
            SettingsHelper.SaveMainSettings(App.MainConfigFile,App.MainSettings);
            SettingsHelper.SaveProfileSettings(App.ProfilePath, App.Settings);
        }
        #endregion
    }
}