using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using SharedCode;

namespace Cyanlabs.Launcher
{
    /// <summary>
    ///     Interaction logic for UpgradingWindow.xaml
    /// </summary>
    public partial class UpgradingWindow
    {
        // ReSharper disable once InconsistentNaming
        private const int CSIDL_PROGRAMS = 0x02; // 

        public static string BaseFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public UpgradingViewModel Vm;

        public UpgradingWindow()
        {
            InitializeComponent();
            Vm = DataContext as UpgradingViewModel;
            Core.UpgradingWindow = this;
        }

        private async void AcrylicWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Thread.Sleep(100);
            await StartCheck();
        }

        private async Task StartCheck()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string installPath = (string) Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Syn3Updater", "UninstallString", null);
            if (installPath != null) BaseFolder = Path.GetDirectoryName(installPath);

            if (!Directory.Exists(BaseFolder)) Directory.CreateDirectory(BaseFolder ?? string.Empty);

            Process[] processlist = Process.GetProcesses();

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

            string configFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\CyanLabs\\Syn3Updater";
            if (File.Exists(configFolderPath + "\\launcherPrefs.json"))
                Core.LauncherPrefs = JsonConvert.DeserializeObject<LauncherPrefs>(File.ReadAllText(configFolderPath + "\\launcherPrefs.json"));

            UpdateCheck check = new UpdateCheck();
            await check.Execute(Core.LauncherPrefs.ReleaseBranch, this, BaseFolder);
            while (!check.Complete) await Task.Delay(100);

            StringBuilder path = new StringBuilder(260);
            SHGetSpecialFolderPath(IntPtr.Zero, path, CSIDL_PROGRAMS, false);
            string s = path.ToString();
            if (Directory.Exists(s + "\\Syn3Updater")) Directory.Delete(s + "\\Syn3Updater", true);

            Process p = new Process
            {
                StartInfo =
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = BaseFolder + "\\Syn3Updater.exe",
                    WorkingDirectory = BaseFolder ?? string.Empty,
                    Arguments = "/launcher",
                    UseShellExecute = false
                }
            };
            p.Start();

            await Task.Delay(2000);

            Application.Current.Shutdown();
        }

        [DllImport("shell32.dll")]
        private static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out] StringBuilder lpszPath, int nFolder, bool fCreate);
    }
}