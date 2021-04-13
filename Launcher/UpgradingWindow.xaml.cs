using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Cyanlabs.Updater.Common;

namespace Cyanlabs.Launcher
{
    /// <summary>
    ///     Interaction logic for UpgradingWindow.xaml
    /// </summary>
    public partial class UpgradingWindow
    {
        #region Constructors
        public UpgradingWindow()
        {
            InitializeComponent();
            Vm = DataContext as UpgradingViewModel;
            Core.UpgradingWindow = this;
        }
        #endregion

        #region Properties & Fields
        public static string BaseFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public UpgradingViewModel Vm;
        #endregion

        #region Methods
        private async void AcrylicWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Thread.Sleep(100);
            await StartCheck();
        }

        private async Task StartCheck()
        {
            // Set 'SecurityProtocolType' to Tls12 to allow Windows 7 and old .NET Framework versions to access TLS1.2 secure sites
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Check if Syn3Updater Installer path exists in registry, if so use it's path as the destination path
            Process[] processlist = Process.GetProcesses();

            // Forcefully close all Syn3Updater processes, ignore all exceptions
            if (processlist.Any(x => x.ProcessName == "Syn3Updater"))
                try
                {
                    Process proc = processlist.First(x => x.ProcessName == "Syn3Updater");
                    proc.Kill();
                    proc.Dispose();
                }
                catch
                {
                    // ignored
                }

            // Attempt to load existing settings if found, if not use defaults
            string configFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\CyanLabs\\Syn3Updater";
            if (File.Exists(configFolderPath + "\\launcherPrefs.json"))
            {
                try
                {
                    Core.LauncherPrefs = JsonConvert.DeserializeObject<LauncherPrefs>(File.ReadAllText(configFolderPath + "\\launcherPrefs.json"));
                }
                catch (JsonReaderException)
                {
                    File.Delete(configFolderPath + "\\launcherPrefs.json");
                    Application.Current.Shutdown();
                    Process.Start(BaseFolder + "\\Syn3Updater.exe", "/launcher");
                }
            }

            // Delete Launcher_OldVersion.exe
            if (File.Exists("Launcher_OldVersion.exe"))
                File.Delete("Launcher_OldVersion.exe");

            // Start and wait for the UpdateCheck to complete
            UpdateCheck check = new UpdateCheck();
            bool updated = await check.Execute(Core.LauncherPrefs.ReleaseBranch, this, BaseFolder);
            // Update complete, either no update needed or new update downloaded and extracted, run Syn3Updater.exe           
            Process p = new Process
            {
                StartInfo =
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = BaseFolder+"\\Syn3Updater.exe",
                    WorkingDirectory = BaseFolder ?? string.Empty,
                    Arguments = "/launcher" + (updated ? " /updated" : ""),
                    UseShellExecute = false
                }
            };
            p.Start();

            await Task.Delay(2000);

            // Exit Launcher
            Application.Current.Shutdown();
        }
        #endregion

    }
}