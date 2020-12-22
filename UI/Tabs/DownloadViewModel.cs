using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Syn3Updater.Helper;
using Syn3Updater.Model;

namespace Syn3Updater.UI.Tabs
{
    public class DownloadViewModel : LanguageAwareBaseViewModel
    {
        private ActionCommand _cancelButton;
        private CancellationToken _ct;
        private int _currentProgress, _totalPercentage, _totalPercentageMax, _count;
        private string _downloadInfo, _downloadPercentage, _log, _selectedRelease, _selectedRegion, _selectedMapVersion, _version, _progressBarSuffix;
        private Task _downloadTask;
        private FileHelper _fileHelper;
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private ObservableCollection<string> _downloadQueueList;
        private bool _cancelButtonEnabled;
        private string _installMode;

        public event EventHandler<EventArgs<int>> PercentageChanged;

        public ActionCommand CancelButton => _cancelButton ?? (_cancelButton = new ActionCommand(CancelAction));

        public ObservableCollection<string> DownloadQueueList
        {
            get => _downloadQueueList;
            set => SetProperty(ref _downloadQueueList, value);
        }

        public bool CancelButtonEnabled
        {
            get => _cancelButtonEnabled;
            set => SetProperty(ref _cancelButtonEnabled, value);
        }

        public string InstallMode
        {
            get => _installMode;
            set => SetProperty(ref _installMode, value);
        }

        public int CurrentProgress
        {
            get => _currentProgress;
            set => SetProperty(ref _currentProgress, value);
        }

        public string DownloadPercentage
        {
            get => _downloadPercentage;
            set => SetProperty(ref _downloadPercentage, value);
        }

        public int TotalPercentage
        {
            get => _totalPercentage;
            set => SetProperty(ref _totalPercentage, value);
        }

        public int TotalPercentageMax
        {
            get => _totalPercentageMax;
            set => SetProperty(ref _totalPercentageMax, value);
        }

        public string DownloadInfo
        {
            get => _downloadInfo;
            set => SetProperty(ref _downloadInfo, value);
        }
        public string Log
        {
            get => _log;
            set => SetProperty(ref _log, value);
        }

        private void UpdateLog(string text)
        {
            Log += DateTime.Now + " " + text + Environment.NewLine;
            ApplicationManager.Logger.Info(text);
        }

        public void Init()
        {
            if (ApplicationManager.Instance.IsDownloading && (_downloadTask == null || !_downloadTask.Status.Equals(TaskStatus.Running)))
            {
                _selectedRelease = ApplicationManager.Instance.SelectedRelease;
                _selectedRegion = ApplicationManager.Instance.SelectedRegion;
                _selectedMapVersion = ApplicationManager.Instance.SelectedMapVersion;
                UpdateLog($"[App] Selected Region: {_selectedRegion} - Release: {_selectedRelease} - Map Version: {_selectedMapVersion}");

                CancelButtonEnabled = true;
                //OnPropertyChanged("CancelButtonEnabled");
                InstallMode = ApplicationManager.Instance.InstallMode;
                UpdateLog($"[App] Install mode set to {InstallMode}");
                //OnPropertyChanged("InstallMode");
                PercentageChanged += DownloadPercentageChanged;
                CurrentProgress = 0;

                DownloadQueueList = new ObservableCollection<string>();
                foreach (HomeViewModel.LocalIvsu item in ApplicationManager.Instance.Ivsus) DownloadQueueList.Add(item.Url);
                //OnPropertyChanged("DownloadQueueList");

                _version = Properties.Settings.Default.CurrentSyncVersion.ToString();
                _version = $"{_version[0]}.{_version[1]}.{_version.Substring(2, _version.Length - 2)}";

                _ct = _tokenSource.Token;
                Log = "";

                _fileHelper = new FileHelper(PercentageChanged);
                _downloadTask = Task.Run(DoDownload, _tokenSource.Token);
            }
        }

        private void DoCopy()
        {
            foreach (HomeViewModel.LocalIvsu item in ApplicationManager.Instance.Ivsus)
            {
                if (_ct.IsCancellationRequested)
                {
                    UpdateLog("[App] Process cancelled by user");
                    return;
                }

                if (ValidateFile(ApplicationManager.Instance.DownloadLocation + item.FileName, $@"{ApplicationManager.Instance.DriveLetter}\SyncMyRide\{item.FileName}", item.Md5,
                    true))
                {
                    UpdateLog($"[Copier] {item.FileName} exists and successfully validated, skipping copy");
                    _count++;
                }
                else
                {
                    UpdateLog($"[Copier] {item.FileName} is missing or invalid, copying");
                    DownloadInfo = $"Copying: {item.FileName}";
                    _progressBarSuffix = LanguageManager.GetValue("String.Copied");

                    for (int i = 1; i < 4; i++)
                    {
                        if (_ct.IsCancellationRequested) return;
                        if (i > 1)
                        {
                            UpdateLog($"[Copier] {item.FileName} is missing or invalid, copying (Attempt #{i})");
                            DownloadInfo = $"Copying (Attempt #{i}): {item.FileName}";
                        }

                        _fileHelper.copy_file(ApplicationManager.Instance.DownloadLocation + item.FileName, $@"{ApplicationManager.Instance.DriveLetter}\SyncMyRide\{item.FileName}",
                            _ct);

                        bool validfile = ValidateFile(ApplicationManager.Instance.DownloadLocation + item.FileName,
                            $@"{ApplicationManager.Instance.DriveLetter}\SyncMyRide\{item.FileName}", item.Md5, true);
                        if (validfile)
                        {
                            UpdateLog($"[Copier] copied {item.FileName} and successfully validated");
                            _count++;
                            break;
                        }

                        if (i == 3)
                        {
                            
                            UpdateLog($"[Copier] unable to successfully validate {item.FileName} after 3 tries, ABORTING PROCESS!");
                            MessageBox.Show(string.Format(LanguageManager.GetValue("MessageBox.FailedToValidate3"), item.FileName), "Syn3 Updater", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            CancelAction();
                            break;
                        }
                    }
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    DownloadQueueList.Remove(ApplicationManager.Instance.DownloadLocation + item.FileName);
                    //OnPropertyChanged("DownloadQueueList");
                });
                _count++;
                PercentageChanged.Raise(this, 100);
            }

            DownloadInfo = "";
            DownloadPercentage = "";
            UpdateLog("[App] All files downloaded and copied to USB successfully!");
            DownloadInfo = LanguageManager.GetValue("String.Completed");
            GenerateLog();
            if (MessageBox.Show(LanguageManager.GetValue("MessageBox.UpdateCurrentversion"), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Information) ==
                MessageBoxResult.Yes)
                Properties.Settings.Default.CurrentSyncVersion = Convert.ToInt32(ApplicationManager.Instance.SelectedRelease.Replace(".", "").Replace("Sync ", ""));
            MessageBox.Show(LanguageManager.GetValue("MessageBox.Completed"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Information);
            Process.Start($"https://cyanlabs.net/tutorials/update-ford-sync-3-2-2-3-0-to-version-3-4-all-years-3-4-19200/#{InstallMode}");
            CancelAction();
        }

        public static string GetOsFriendlyName()
        {
            string result = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
            foreach (ManagementBaseObject o in searcher.Get())
            {
                ManagementObject os = (ManagementObject) o;
                result = os["Caption"].ToString();
                break;
            }

            return $"{result} ({Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId", "")})";
        }

        private void GenerateLog()
        {
            string data = $@"CYANLABS - SYN3 UPDATER - V{Assembly.GetExecutingAssembly().GetName().Version}{Environment.NewLine}";
            data += $@"Operating System: {GetOsFriendlyName()}{Environment.NewLine}";
            data += Environment.NewLine;
            data += $@"PREVIOUS CONFIGURATION{Environment.NewLine}";
            data += $@"Version: {_version}{Environment.NewLine}";
            data += $@"Region: {Properties.Settings.Default.CurrentSyncRegion}{Environment.NewLine}";
            data += $@"Navigation: {Properties.Settings.Default.CurrentSyncNav}{Environment.NewLine}";
            data +=
                $@"Mode: {(Properties.Settings.Default.CurrentInstallMode == @"autodetect" ? InstallMode : $"{Properties.Settings.Default.CurrentInstallMode} FORCED")}{Environment.NewLine}";
            data += Environment.NewLine;
            data += $@"USB DETAILS{Environment.NewLine}";
            data += $@"Model: {ApplicationManager.Instance.DriveName}{Environment.NewLine}";
            data += $@"FileSystem: {ApplicationManager.Instance.DriveFileSystem}{Environment.NewLine}";
            data += $@"Partition Type: {ApplicationManager.Instance.DrivePartitionType}{Environment.NewLine}";

            string driveletter = ApplicationManager.Instance.DriveLetter;
            if (File.Exists($@"{driveletter}\reformat.lst"))
            {
                data += Environment.NewLine;
                data += $@"REFORMAT.LST{Environment.NewLine}";
                data += File.ReadAllText($@"{driveletter}\reformat.lst") + Environment.NewLine;
            }

            if (File.Exists($@"{driveletter}\autoinstall.lst"))
            {
                data += Environment.NewLine;
                data += $@"AUTOINSTALL.LST{Environment.NewLine}";
                data += File.ReadAllText($@"{driveletter}\autoinstall.lst") + Environment.NewLine;
            }

            if (Directory.Exists($@"{driveletter}\SyncMyRide"))
            {
                data += Environment.NewLine;
                DirectoryInfo di = new DirectoryInfo($@"{driveletter}\SyncMyRide");
                FileInfo[] allFiles = di.GetFiles("*", SearchOption.AllDirectories);
                data += $@"SYNCMYRIDE FILES ({allFiles.Length}){Environment.NewLine}";
                foreach (FileInfo file in allFiles)
                    data += $"{file.Name} ({MathHelper.BytesToString(file.Length)}){Environment.NewLine}";
                data += Environment.NewLine;
            }

            data += $@"LOG{Environment.NewLine}";
            data += Log;
            File.WriteAllText($@"{driveletter}\log.txt", data);
        }

        private void CancelAction()
        {
            CancelButtonEnabled = false;
            //OnPropertyChanged("CancelButtonEnabled");
            ApplicationManager.Instance.IsDownloading = false;
            _tokenSource.Cancel();
            //Thread.Sleep(2000);
            TotalPercentage = 0;
            CurrentProgress = 0;
            DownloadInfo = "";
            DownloadPercentage = "";
            Application.Current.Dispatcher.Invoke(() =>
            {
                DownloadQueueList.Clear();
                //OnPropertyChanged("DownloadQueueList");
            });
            _tokenSource.Dispose();
            _tokenSource = new CancellationTokenSource();
        }

        private void DownloadPercentageChanged(object sender, EventArgs<int> e)
        {
            DownloadPercentage = $"{e.Value}% {_progressBarSuffix}";
            CurrentProgress = e.Value;
            TotalPercentage = _count == 0 ? e.Value : _count * 100 + e.Value;
        }

        private async void DoDownload()
        {
            int total = ApplicationManager.Instance.Ivsus.Count;
            _count = 0;

            TotalPercentageMax = 100 * total * (ApplicationManager.Instance.DownloadOnly ? 2 : 4);

            try
            {
                foreach (HomeViewModel.LocalIvsu item in ApplicationManager.Instance.Ivsus)
                {
                    if (_ct.IsCancellationRequested)
                    {
                        UpdateLog("[App] Process cancelled by user");
                        return;
                    }

                    if (ValidateFile(item.Url, ApplicationManager.Instance.DownloadLocation + item.FileName, item.Md5, false))
                    {
                        UpdateLog($"[Downloader] {item.FileName} exists and successfully validated, skipping download");
                        _count++;
                    }
                    else
                    {
                        UpdateLog($"[Downloader] {item.FileName} is missing or invalid, downloading");
                        DownloadInfo = $"Downloading: {item.Url}";
                        _progressBarSuffix = LanguageManager.GetValue("String.Downloaded");
                        try
                        {
                            for (int i = 1; i < 4; i++)
                            {
                                if (_ct.IsCancellationRequested) return;
                                if (i > 1)
                                {
                                    UpdateLog($"[Downloader] {item.FileName} is missing or invalid, downloading (Attempt #{i})");
                                    DownloadInfo = $"Downloading (Attempt #{i}): {item.Url}";
                                }

                                try
                                {
                                    await _fileHelper.download_file(item.Url, ApplicationManager.Instance.DownloadLocation + item.FileName, _ct);
                                }
                                catch (HttpRequestException webException)
                                {
                                    MessageBox.Show(string.Format(LanguageManager.GetValue("MessageBox.WebException", webException.Message)), "Syn3 Updater", MessageBoxButton.OK,
                                        MessageBoxImage.Exclamation);
                                }

                                bool validfile = ValidateFile(item.Url, ApplicationManager.Instance.DownloadLocation + item.FileName, item.Md5, false);
                                if (validfile)
                                {
                                    UpdateLog($"[Downloader] downloaded {item.FileName} and successfully validated");
                                    _count++;
                                    break;
                                }

                                if (i == 3)
                                {
                                    UpdateLog($"[Downloader] unable to successfully validate {item.FileName} after 3 tries, ABORTING PROCESS!");
                                    MessageBox.Show(string.Format(LanguageManager.GetValue("MessageBox.FailedToValidate3", item.FileName)), "Syn3 Updater", MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                    CancelAction();
                                    break;
                                }
                            }
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                        }
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DownloadQueueList.Remove(item.Url);
                        //OnPropertyChanged("DownloadQueueList");
                    });
                    _count++;
                    PercentageChanged.Raise(this, 100);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    DownloadQueueList.Clear();
                    //OnPropertyChanged("DownloadQueueList");
                });
            }
            catch (InvalidOperationException)
            {
                //
            }

            if (_ct.IsCancellationRequested == false)
            {
                if (ApplicationManager.Instance.DownloadOnly)
                {
                    UpdateLog("[App] Process completed successfully (download only)");
                    DownloadInfo = LanguageManager.GetValue("Strings.Completed");
                    MessageBox.Show(LanguageManager.GetValue("MessageBox.DownloadOnlyComplete"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Information);
                    ApplicationManager.Instance.IsDownloading = false;
                    CancelAction();
                }
                else
                {
                    PrepareUsb();
                }
            }
        }

        private void PrepareUsb()
        {
            UpdateLog("[App] Preparing USB drive");
            if (ApplicationManager.Instance.SkipFormat == false)
            {
                UpdateLog("[App] Formatting USB drive");
                string drivenumber = ApplicationManager.Instance.DriveNumber;
                string driveletter = ApplicationManager.Instance.DriveLetter;

                using (Process p = new Process())
                {
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.FileName = @"diskpart.exe";
                    p.StartInfo.CreateNoWindow = true;

                    UpdateLog("[App] Re-creating partition table as MBR and formatting as ExFat on selected USB drive");

                    p.Start();
                    p.StandardInput.WriteLine($"SELECT DISK={drivenumber}");
                    p.StandardInput.WriteLine("CLEAN");
                    p.StandardInput.WriteLine("CONVERT MBR");
                    p.StandardInput.WriteLine("CREATE PARTITION PRIMARY");
                    p.StandardInput.WriteLine("FORMAT FS=EXFAT LABEL=\"CYANLABS\" QUICK");
                    p.StandardInput.WriteLine($"ASSIGN {driveletter.Replace(":", "")}");
                    p.StandardInput.WriteLine("EXIT");

                    p.WaitForExit();
                }

                Thread.Sleep(5000);
            }

            switch (InstallMode)
            {
                case "autoinstall":
                    CreateAutoInstall();
                    break;
                case "downgrade":
                    CreateReformat();
                    break;
                case "reformat":
                    CreateReformat();
                    break;
            }

            foreach (HomeViewModel.LocalIvsu item in ApplicationManager.Instance.Ivsus)
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DownloadQueueList.Add(ApplicationManager.Instance.DownloadLocation + item.FileName);
                    //OnPropertyChanged("DownloadQueueList");
                });

            Directory.CreateDirectory($@"{ApplicationManager.Instance.DriveLetter}\SyncMyRide\");

            Task.Run(DoCopy, _tokenSource.Token);
        }

        private void CreateAutoInstall()
        {
            UpdateLog("[App] Generating Autoinstall.lst");
            string autoinstalllst =
                $@"; CyanLabs Syn3Updater 2.x - Autoinstall Mode - {_selectedRelease} {_selectedRegion}{Environment.NewLine}{Environment.NewLine}[SYNCGen3.0_ALL_PRODUCT]{Environment.NewLine}";

            string extrafiles = "";
            int baseint = 0, extraint = 0;
            foreach (HomeViewModel.LocalIvsu item in ApplicationManager.Instance.Ivsus)
                if (item.Type == @"APPS" || item.Type == @"VOICE" || item.Type == @"ENH_DAB" || item.Type == @"MAP_LICENSE" || item.Type == @"VOICE_NAV")
                {
                    baseint++;
                    autoinstalllst += $@"Item{baseint} = {item.Type} - {item.FileName}\rOpen{baseint} = SyncMyRide\{item.FileName}\r".Replace(@"\r", Environment.NewLine);
                }
                else
                {
                    if (extrafiles == "") extrafiles = $@"[SYNCGen3.0_ALL]{Environment.NewLine}";
                    if (extraint == 10)
                    {
                        extraint = 0;
                        extrafiles += $@"Options = Delay,Include,Transaction{Environment.NewLine}[SYNCGen3.0_{_version}]{Environment.NewLine}";
                    }

                    extraint++;
                    extrafiles += $@"Item{extraint} = {item.Type} - {item.FileName}\rOpen{extraint} = SyncMyRide\{item.FileName}\r".Replace(@"\r", Environment.NewLine);
                }

            if (extrafiles != "") extrafiles += @"Options = Delay,Include,Transaction";
            autoinstalllst += $@"Options = AutoInstall{Environment.NewLine}";
            autoinstalllst += extrafiles;
            File.WriteAllText($@"{ApplicationManager.Instance.DriveLetter}\autoinstall.lst", autoinstalllst);
            File.Create($@"{ApplicationManager.Instance.DriveLetter}\DONTINDX.MSA");
        }

        private void CreateReformat()
        {
            UpdateLog("[App] Generating reformat.lst");
            string reformatlst = "";
            int i = 0;
            foreach (HomeViewModel.LocalIvsu item in ApplicationManager.Instance.Ivsus)
            {
                if (item.Md5 == HomeViewModel.SyncReformatToolMd5 || item.Md5 == HomeViewModel.DowngradePackageAppMd5 && _selectedRelease != @"Sync 3.3.19052" ||
                    item.Md5 == HomeViewModel.DowngradePackageToolMd5) continue;
                i++;
                reformatlst += $@"{item.Type}={item.FileName}";
                if (i != ApplicationManager.Instance.Ivsus.Count) reformatlst += Environment.NewLine;
            }

            File.WriteAllText($@"{ApplicationManager.Instance.DriveLetter}\reformat.lst", reformatlst);

            UpdateLog("[App] Generating autoinstall.lst");
            string autoinstalllst =
                $@"; CyanLabs Syn3Updater 2.x - {InstallMode} Mode - {_selectedRelease} {_selectedRegion}{Environment.NewLine}{Environment.NewLine}[SYNCGen3.0_ALL_PRODUCT]{Environment.NewLine}";
            if (InstallMode == @"downgrade")
            {
                autoinstalllst +=
                    $@"Item1 = TOOL - {HomeViewModel.DowngradePackageToolFileName}\rOpen1 = SyncMyRide\{HomeViewModel.DowngradePackageToolFileName}\r".Replace(@"\r",
                        Environment.NewLine);
                autoinstalllst +=
                    $@"Item2 = APP - {HomeViewModel.DowngradePackageAppFileName}\rOpen2 = SyncMyRide\{HomeViewModel.DowngradePackageAppFileName}\r".Replace(@"\r",
                        Environment.NewLine);
                autoinstalllst += $@"Options = AutoInstall{Environment.NewLine}[SYNCGen3.0_ALL]{Environment.NewLine}";
                autoinstalllst +=
                    $@"Item1 = REFORMAT TOOL - {HomeViewModel.SyncReformatToolFileName}\rOpen1 = SyncMyRide\{HomeViewModel.SyncReformatToolFileName}\r".Replace(@"\r",
                        Environment.NewLine);
                autoinstalllst += $@"Options = AutoInstall,Include,Transaction{Environment.NewLine}";
            }
            else if (InstallMode == @"reformat")
            {
                autoinstalllst +=
                    $@"Item1 = REFORMAT TOOL  - {HomeViewModel.SyncReformatToolFileName}\rOpen1 = SyncMyRide\{HomeViewModel.SyncReformatToolFileName}\r".Replace(@"\r",
                        Environment.NewLine);
                autoinstalllst += @"Options = AutoInstall";
            }

            File.WriteAllText($@"{ApplicationManager.Instance.DriveLetter}\autoinstall.lst", autoinstalllst);
            File.Create($@"{ApplicationManager.Instance.DriveLetter}\DONTINDX.MSA");
        }

        private bool ValidateFile(string srcfile, string localfile, string md5, bool copy)
        {
            DownloadInfo = $"Validating: {localfile}";
            _progressBarSuffix = LanguageManager.GetValue("String.Validated");
            FileHelper.ValidateResult validateResult = _fileHelper.ValidateFile(srcfile, localfile, md5, copy, _ct);
            UpdateLog(validateResult.Message);
            return validateResult.Result;
        }
    }
}