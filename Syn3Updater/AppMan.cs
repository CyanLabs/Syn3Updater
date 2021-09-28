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
        public void FireDownloadsTabEvent()
        {
            ShowDownloadsTab?.Invoke(this, EventArgs.Empty);
        }
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

        public void FireSettingsTabEvent()
        {
            ShowSettingsTab?.Invoke(this, EventArgs.Empty);
        }

        public void FireNewsTabEvent()
        {
            ShowNewsTab?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ShowDownloadsTab;
        public event EventHandler StartDownloadsTab;
        public event EventHandler ShowHomeTab;
        public event EventHandler ShowInterrogatorLogCompleted;
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
            ProgramDataPath,
            LocalAppDataPath,
            ProfilePath,
            Header,
            LauncherConfigFile,
            Magnet,
            Outdated;

        public bool SkipFormat, IsDownloading, UtilityCreateLogStep1Complete, AppsSelected, DownloadToFolder, ModeForced, Cancelled, DownloadOnly, ClearSelections;
        
        // Initiate GraphQlClient
        public readonly GraphQLHttpClient GraphQlClient = new(Api.Syn3UpdaterGraphQl, new NewtonsoftJsonSerializer())
        {
            HttpClient = { DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer",ApiSecret.Token)}}
        };
        public int AppUpdated = 0;

        #endregion

        #region Methods

        public void Initialize()
        {
            if (OperatingSystem.IsWindows())
            {
                WindowsSystemHelper.WriteRegistryHandler();
            } else if (OperatingSystem.IsMacOS())
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
            {
                ProgramDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\CyanLabs\\Syn3Updater" + Path.DirectorySeparatorChar;
                LocalAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\CyanLabs\\Syn3Updater" + Path.DirectorySeparatorChar;
            } 
            else if (OperatingSystem.IsMacOS())
            {
                ProgramDataPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Library/Application Support/Syn3Updater" + Path.DirectorySeparatorChar;
                LocalAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Library/Application Support/Syn3Updater" + Path.DirectorySeparatorChar;
            }

            ProfilePath = LocalAppDataPath + "Profiles" +  + Path.DirectorySeparatorChar;
            MainConfigFile = LocalAppDataPath + "settings.json";

            if (!Directory.Exists(ProgramDataPath)) Directory.CreateDirectory(ProgramDataPath);
            if (!Directory.Exists(LocalAppDataPath)) Directory.CreateDirectory(LocalAppDataPath);
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
            else if (File.Exists(ProgramDataPath + "settings.json"))
            {
                File.Move(ProgramDataPath + "settings.json", MainConfigFile);
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

            if (string.IsNullOrEmpty(MainSettings.LogPath)) MainSettings.LogPath = LocalAppDataPath + "Logs" + Path.DirectorySeparatorChar;
            try
            {
                if (!Directory.Exists(MainSettings.LogPath)) Directory.CreateDirectory(MainSettings.LogPath);
            }
            catch (DirectoryNotFoundException)
            {
                MainSettings.LogPath = LocalAppDataPath + "Logs" + Path.DirectorySeparatorChar;
            }

            if (string.IsNullOrWhiteSpace(MainSettings.DownloadPath))
            {
                if (OperatingSystem.IsWindows())
                {
                    string downloads = WindowsSystemHelper.GetPath(WindowsSystemHelper.KnownFolder.Downloads);
                    MainSettings.DownloadPath = $@"{downloads}\Syn3Updater\";
                } 
                else if (OperatingSystem.IsMacOS())
                {
                    MainSettings.DownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Downloads/Syn3Updater";
                }
            }
            
            DownloadPath = App.MainSettings.DownloadPath;

            try
            {
                if (!Directory.Exists(DownloadPath))
                {
                    Directory.CreateDirectory(DownloadPath);
                }
            }
            catch (DirectoryNotFoundException)
            {
                if (OperatingSystem.IsWindows())
                {
                    string downloads = WindowsSystemHelper.GetPath(WindowsSystemHelper.KnownFolder.Downloads);
                    MainSettings.DownloadPath = $@"{downloads}\Syn3Updater\";
                } 
                else if (OperatingSystem.IsMacOS())
                {
                    MainSettings.DownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Downloads/Syn3Updater";
                }

                DownloadPath = App.MainSettings.DownloadPath;
            }
            catch (IOException)
            {
                if (OperatingSystem.IsWindows())
                {
                    string downloads = WindowsSystemHelper.GetPath(WindowsSystemHelper.KnownFolder.Downloads);
                    MainSettings.DownloadPath = $@"{downloads}\Syn3Updater\";
                } 
                else if (OperatingSystem.IsMacOS())
                {
                    MainSettings.DownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Downloads/Syn3Updater";
                }

                DownloadPath = App.MainSettings.DownloadPath;
            }

            Randomize();
            App.SaveMainSettings();
            App.SaveProfileSettings();
            Client.DefaultRequestHeaders.UserAgent.TryParseAdd(Header);
        }

        private void Randomize()
        {
            try
            {
                Random rand = new();
                GraphQLResponse<UseragentRoot> graphQlResponse = Task.Run(async () => await GraphQlClient.SendQueryAsync<UseragentRoot>(GraphQlHelper.GetUserAgents())).Result;
                UseragentRoot userAgents = graphQlResponse.Data;
                List<string> header = userAgents.UserAgents.Select(ua => ua.Useragent.Replace("[PLACEHOLDER]", rand.Next(ua.Min, ua.Max).ToString())).ToList();
                int index = rand.Next(header.Count);
                Header = header[index];
            }
            catch (Exception)
            {
                Header = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:70.0) Gecko/20100101 Firefox/86.0";
            }
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

        public void SaveMainSettings()
        {
            try
            {
                string mainJson = JsonConvert.SerializeObject(MainSettings, Formatting.Indented);
                File.WriteAllText(MainConfigFile, mainJson);
            }
            catch (IOException)
            {
                // ignored
            }
        }
        
        public void SaveProfileSettings()
        {
            try
            {
                string profileJson = JsonConvert.SerializeObject(Settings, Formatting.Indented);
                File.WriteAllText(ProfileFile, profileJson);
            }
            catch (IOException)
            {
                // ignored
            }
        }

        public void ResetSettings()
        {
            try
            {
                if (File.Exists(MainConfigFile)) File.Delete(MainConfigFile);
                if (File.Exists(ProfileFile)) File.Delete(ProfileFile);
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        public static void Exit()
        {
            App.SaveMainSettings();
            App.SaveProfileSettings();
        }

        #endregion
    }
}