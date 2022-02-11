//using DiscordRPC;

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;
using Syn3Updater.Helpers;
using Syn3Updater.Helpers.Windows;
using Syn3Updater.Models;

namespace Syn3Updater
{
    public class AppMan
    {
        #region Constructors

        private AppMan()
        {
        }
        
        public ObservableCollection<SModel.Ivsu> Ivsus = new();
        public ObservableCollection<SModel.Ivsu> ExtraIvsus = new();
        public static AppMan App { get; } = new();
        public MainSettings MainSettings { get; set; }
        public ProfileSettings Settings { get; set; }
        public static HttpClient Client = new();
        #endregion

        #region Events
        public void FireDownloadsStartEvent()
        {
            ShowDownloadsTab.Invoke(this, EventArgs.Empty);
            StartDownloadsTab.Invoke(this, EventArgs.Empty);
        }

        public void FireHomeTabEvent()
        {
            ShowHomeTab.Invoke(this, EventArgs.Empty);
        }
        
        public void FireInterrogatorLogCompleted()
        {
            ShowInterrogatorLogCompleted.Invoke(this, EventArgs.Empty);
        }
        
        public event EventHandler ShowDownloadsTab;
        public event EventHandler StartDownloadsTab;
        public event EventHandler ShowHomeTab;
        public event EventHandler ShowInterrogatorLogCompleted;

        #endregion

        #region Properties & Fields

        public string DownloadPath,
            DriveName, DrivePartitionType, DriveFileSystem, DriveNumber, DrivePath, SelectedMapVersion, SelectedRelease, SelectedRegion, InstallMode,
            SVersion, Action, MainConfigFile, ProfileFile, AppDataPath, ProfilePath, Header, Magnet, AutoInstall;

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
                //TODO Reimplement Windows Intent
            else if (OperatingSystem.IsMacOS())
            {
                //TODO MAC OSX Intent
            }
            
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 8;
            ServicePointManager.Expect100Continue = false;
            string[] args = Environment.GetCommandLineArgs();

            foreach (string value in args)
                if (value.Contains("syn3updater://"))
                    Magnet = value.Replace("syn3updater://", "").TrimEnd('/');
            

            if (OperatingSystem.IsWindows())
                AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\CyanLabs\\Syn3Updater\\";
            else if (OperatingSystem.IsMacOS())
                AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Library/Application Support/Syn3Updater/";
            else
                //TODO Linux?
                AppDataPath = "";
            
            ProfilePath = AppDataPath + "Profiles" + Path.DirectorySeparatorChar;
            MainConfigFile = AppDataPath + "settings.json";

            if (!Directory.Exists(AppDataPath)) Directory.CreateDirectory(AppDataPath);
            if (!Directory.Exists(ProfilePath)) Directory.CreateDirectory(ProfilePath);

            if (File.Exists(MainConfigFile))
            {
                try
                {
                    MainSettings = JsonConvert.DeserializeObject<MainSettings>(File.ReadAllText(MainConfigFile)) ?? new MainSettings();
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

            if (string.IsNullOrEmpty(MainSettings.LogPath)) MainSettings.LogPath = AppDataPath + "Logs" + Path.DirectorySeparatorChar;
            
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
                    MainSettings.DownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Downloads/Syn3Updater/";
                else
                    //TODO Linux?
                    MainSettings.DownloadPath = "";
            }
            
            DownloadPath = MainSettings.DownloadPath ?? string.Empty;
            
            try
            {
                if (!Directory.Exists(DownloadPath)) Directory.CreateDirectory(DownloadPath);
            }
            catch (Exception)
            {
                if (OperatingSystem.IsWindows())
                    MainSettings.DownloadPath = $@"{WindowsSystemHelper.GetPath(WindowsSystemHelper.KnownFolder.Downloads)}\Syn3Updater\";
                else if (OperatingSystem.IsMacOS())
                    MainSettings.DownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Downloads/Syn3Updater/";
                    
                DownloadPath = MainSettings.DownloadPath ?? string.Empty;
            }

            Header = ApiHelper.GetGeneratedUserAgent();
            
            SettingsHelper.SaveMainSettings(App.MainConfigFile,App.MainSettings);
            SettingsHelper.SaveProfileSettings(App.ProfileFile, App.Settings);
            
            Client.DefaultRequestHeaders.UserAgent.TryParseAdd(Header);
        }

        private void LoadProfile()
        {
            if (string.IsNullOrEmpty(MainSettings.Profile))
            {
                MainSettings.Profile = "default";
                ProfileFile = ProfilePath + "default.json";
            }
            else
            {
                ProfileFile = ProfilePath + $"{MainSettings.Profile}.json";
            }
            
            if (File.Exists(ProfileFile))
            {
                try
                {
                    Settings = JsonConvert.DeserializeObject<ProfileSettings>(File.ReadAllText(ProfileFile)) ?? new ProfileSettings();
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
            catch (IndexOutOfRangeException)
            {
                // ignored
            }
        }

        public static void Exit()
        {
            SettingsHelper.SaveMainSettings(App.MainConfigFile,App.MainSettings);
            SettingsHelper.SaveProfileSettings(App.ProfileFile, App.Settings);
        }
        #endregion
    }
}