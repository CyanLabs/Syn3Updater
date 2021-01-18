using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using SharedCode;
using File = System.IO.File;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for UpgradingWindow.xaml
    /// </summary>
    public partial class UpgradingWindow
    {
        public UpgradingWindow()
        {
            InitializeComponent();
            vm = DataContext as UpgradingViewModel;
            Core.UpgradingWindow = this;
        }

        public UpgradingViewModel vm;

        private async void AcrylicWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Thread.Sleep(100);
            await StartCheck();
        }

        public static string BaseFolder = "";
        readonly int _oldversion = Core.LauncherPrefs.ReleaseInstalled;
        private async Task StartCheck()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string InstallPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Syn3Updater", "UninstallString", null);
            if (InstallPath == null)
            {
                BaseFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            else
            {
                BaseFolder = Path.GetDirectoryName(InstallPath);
            }

            if (!Directory.Exists(BaseFolder)) Directory.CreateDirectory(BaseFolder);

            Process[] processlist = Process.GetProcesses();

            if (processlist.Any(x => x.ProcessName == "Syn3Updater"))
            {
                try
                {
                    var proc = processlist.First(x => x.ProcessName == "Syn3Updater");
                    proc.Kill();
                    proc.Dispose();
                }
                catch
                {
                    // ignored
                }
            }
            string configFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\CyanLabs\\Syn3Updater";
            if (File.Exists(configFolderPath + "\\launcherPrefs.json"))
            {
                Core.LauncherPrefs = JsonConvert.DeserializeObject<LauncherPrefs>(File.ReadAllText(configFolderPath + "\\launcherPrefs.json"));
            }

            UpdateCheck check = new UpdateCheck();
            await check.Execute(Core.LauncherPrefs.ReleaseBranch, this, BaseFolder);
            while (!check.Complete)
            {
                await Task.Delay(100);
            }

            StringBuilder path = new StringBuilder(260);
            SHGetSpecialFolderPath(IntPtr.Zero, path, CSIDL_PROGRAMS, false);
            string s = path.ToString();
            if (Directory.Exists(s + "\\Syn3Updater")) Directory.Delete(s + "\\Syn3Updater",true);

            Process p = new Process();
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.FileName = BaseFolder + "\\Syn3Updater.exe";
            p.StartInfo.WorkingDirectory = BaseFolder;
            p.StartInfo.Arguments = "/launcher";
            p.StartInfo.UseShellExecute = false;
            p.Start();

            await Task.Delay(2000);

            Application.Current.Shutdown();
        }

        [DllImport("shell32.dll")]
        static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out] StringBuilder lpszPath, int nFolder, bool fCreate);
        // ReSharper disable once InconsistentNaming
        private const int CSIDL_PROGRAMS = 0x02; // 
    }
}
