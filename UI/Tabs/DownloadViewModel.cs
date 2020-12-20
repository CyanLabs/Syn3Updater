using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
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
        //https://www.technical-recipes.com/2018/reporting-the-percentage-progress-of-large-file-downloads-in-c-wpf/
        private static readonly HttpClient client = new HttpClient();

        private ActionCommand _cancelButton;
        private CancellationToken _ct;

        private int _currentProgress;

        private string _downloadInfo;

        private string _downloadPercentage;

        private string _log;
        private string _selectedRelease, _selectedRegion, _selectedMapVersion;

        private int _totalPercentage;

        private int _totalPercentageMax;
        private string _version;
        private int count;

        private string progress_bar_suffix = "";

        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        public ActionCommand CancelButton => _cancelButton ?? (_cancelButton = new ActionCommand(CancelAction));
        public ObservableCollection<string> DownloadQueueList { get; set; }
        public bool CancelButtonEnabled { get; set; }

        public string InstallMode { get; set; }

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

        public event EventHandler<EventArgs<int>> PercentageChanged;

        private void UpdateLog(string text)
        {
            Log += text + Environment.NewLine;
            ApplicationManager.Logger.Info(text);
        }

        public void Init()
        {
            _selectedRelease = ApplicationManager.Instance.SelectedRelease;
            _selectedRegion = ApplicationManager.Instance.SelectedRegion;
            _selectedMapVersion = ApplicationManager.Instance.SelectedMapVersion;
            UpdateLog(
                $"[App] Selected Region: {_selectedRegion} - Release: {_selectedRelease} - Map Version: {_selectedMapVersion}");

            CancelButtonEnabled = true;
            InstallMode = ApplicationManager.Instance.InstallMode;
            UpdateLog($"[App] Install mode set to {InstallMode}");
            OnPropertyChanged("InstallMode");
            PercentageChanged += DownloadPercentageChanged;
            CurrentProgress = 0;

            DownloadQueueList = new ObservableCollection<string>();
            foreach (HomeViewModel.Ivsu item in ApplicationManager.Instance.Ivsus) DownloadQueueList.Add(item.Url);
            OnPropertyChanged("DownloadQueueList");

            _version = Properties.Settings.Default.CurrentSyncVersion.ToString();
            _version = $"{_version[0]}.{_version[1]}.{_version.Substring(2, _version.Length - 2)}";


            _ct = tokenSource.Token;
            Task.Run(DoDownload, tokenSource.Token);
        }

        private void DoCopy()
        {
            foreach (HomeViewModel.Ivsu item in ApplicationManager.Instance.Ivsus)
            {
                if (_ct.IsCancellationRequested)
                {
                    UpdateLog("[App] Process cancelled by user");
                    return;
                }

                if (ValidateFile(ApplicationManager.Instance.DownloadLocation + item.FileName,
                    $@"{ApplicationManager.Instance.DriveLetter}\SyncMyRide\{item.FileName}", item.Md5, true))
                {
                    UpdateLog($"[Copier] {item.FileName} exists and successfully validated, skipping copy");
                    count++;
                }
                else
                {
                    UpdateLog($"[Copier] {item.FileName} is missing or invalid, copying");
                    DownloadInfo = $"Copying: {item.FileName}";
                    progress_bar_suffix = "copied";

                    for (var i = 1; i < 4; i++)
                    {
                        if (i > 1)
                        {
                            UpdateLog($"[Copier] {item.FileName} is missing or invalid, copying (Attempt #{i})");
                            DownloadInfo = $"Copying (Attempt #{i}): {item.FileName}";
                        }

                        var bufferSize = 1024 * 512;
                        using (FileStream inStream =
                            new FileStream(ApplicationManager.Instance.DownloadLocation + item.FileName, FileMode.Open,
                                FileAccess.Read, FileShare.ReadWrite))
                        using (FileStream fileStream = new FileStream(
                            $@"{ApplicationManager.Instance.DriveLetter}\SyncMyRide\{item.FileName}",
                            FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            int bytesRead;
                            var totalReads = 0;
                            var totalBytes = inStream.Length;
                            byte[] bytes = new byte[bufferSize];
                            var prevPercent = 0;

                            while ((bytesRead = inStream.Read(bytes, 0, bufferSize)) > 0)
                            {
                                if (_ct.IsCancellationRequested)
                                {
                                    fileStream.Close();
                                    fileStream.Dispose();
                                    try
                                    {
                                        File.Delete(
                                            $@"{ApplicationManager.Instance.DriveLetter}\SyncMyRide\{item.FileName}");
                                    }
                                    catch (IOException)
                                    {
                                    }
                                }

                                fileStream.Write(bytes, 0, bytesRead);
                                totalReads += bytesRead;
                                var percent = Convert.ToInt32(totalReads / (decimal) totalBytes * 100);
                                if (percent != prevPercent)
                                {
                                    PercentageChanged.Raise(this, percent);
                                    prevPercent = percent;
                                }
                            }
                        }

                        var validfile = ValidateFile(ApplicationManager.Instance.DownloadLocation + item.FileName,
                            $@"{ApplicationManager.Instance.DriveLetter}\SyncMyRide\{item.FileName}", item.Md5, true);
                        if (validfile)
                        {
                            UpdateLog($"[Copier] copied {item.FileName} and successfully validated");
                            count++;
                            break;
                        }

                        if (i == 3)
                        {
                            UpdateLog(
                                $"[Copier] unable to successfully validate {item.FileName} after 3 tries, ABORTING PROCESS!");
                            MessageBox.Show(
                                string.Format(LanguageManager.GetValue("MessageBox.FailedToValidate3"), item.FileName),
                                "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Error);
                            CancelAction();
                            break;
                        }
                    }
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    DownloadQueueList.Remove(ApplicationManager.Instance.DownloadLocation + item.FileName);
                    OnPropertyChanged("DownloadQueueList");
                });
                count++;
                PercentageChanged.Raise(this, 100);
            }

            DownloadInfo = "";
            DownloadPercentage = "";

            GenerateLog();
            UpdateLog("[App] All files downloaded and copied to USB successfully!");
            DownloadInfo = "COMPLETED SUCCESSFULLY";
            if (MessageBox.Show(LanguageManager.GetValue("MessageBox.UpdateCurrentversion"), "Syn3 Updater",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information) == MessageBoxResult.Yes)
                Properties.Settings.Default.CurrentSyncVersion = Convert.ToInt32(ApplicationManager.Instance
                    .SelectedRelease.Replace(".", "").Replace("Sync ", ""));
            MessageBox.Show(LanguageManager.GetValue("MessageBox.Completed"), "Syn3 Updater", MessageBoxButton.OK,
                MessageBoxImage.Information);
            Process.Start(
                $"https://cyanlabs.net/tutorials/update-ford-sync-3-2-2-3-0-to-version-3-4-all-years-3-4-19200/#{InstallMode}");
            CancelAction();
        }

        public static string GetOsFriendlyName()
        {
            var result = string.Empty;
            ManagementObjectSearcher searcher =
                new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
            foreach (ManagementBaseObject o in searcher.Get())
            {
                ManagementObject os = (ManagementObject) o;
                result = os["Caption"].ToString();
                break;
            }

            return
                $"{result} ({Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId", "")})";
        }

        private void GenerateLog()
        {
            var data =
                $@"CYANLABS - SYN3 UPDATER - V{Assembly.GetExecutingAssembly().GetName().Version}{Environment.NewLine}";
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

            var driveletter = ApplicationManager.Instance.DriveLetter;
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
            ApplicationManager.Instance.IsDownloading = false;
            tokenSource.Cancel();
            Thread.Sleep(2000);
            TotalPercentage = 0;
            CurrentProgress = 0;
            DownloadInfo = "";
            DownloadPercentage = "";
            Application.Current.Dispatcher.Invoke(() =>
            {
                DownloadQueueList.Clear();
                OnPropertyChanged("DownloadQueueList");
            });
            tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();
            ApplicationManager.Instance.FireHomeTabEvent();
        }

        private void DownloadPercentageChanged(object sender, EventArgs<int> e)
        {
            DownloadPercentage = $"{e.Value}% {progress_bar_suffix}";
            CurrentProgress = e.Value;
            TotalPercentage = count == 0 ? e.Value : count * 100 + e.Value;
        }

        private async void DoDownload()
        {
            var total = ApplicationManager.Instance.Ivsus.Count;
            count = 0;

            if (ApplicationManager.Instance.DownloadOnly)
                TotalPercentageMax = 100 * total * 2;
            else
                TotalPercentageMax = 100 * total * 4;

            try
            {
                foreach (HomeViewModel.Ivsu item in ApplicationManager.Instance.Ivsus)
                {
                    if (_ct.IsCancellationRequested)
                    {
                        UpdateLog("[App] Process cancelled by user");
                        return;
                    }

                    if (ValidateFile(item.Url, ApplicationManager.Instance.DownloadLocation + item.FileName, item.Md5,
                        false))
                    {
                        UpdateLog($"[Downloader] {item.FileName} exists and successfully validated, skipping download");
                        count++;
                    }
                    else
                    {
                        UpdateLog($"[Downloader] {item.FileName} is missing or invalid, downloading");
                        DownloadInfo = $"Downloading: {item.Url}";
                        progress_bar_suffix = "downloaded";
                        try
                        {
                            for (var i = 1; i < 4; i++)
                            {
                                if (i > 1)
                                {
                                    UpdateLog(
                                        $"[Downloader] {item.FileName} is missing or invalid, downloading (Attempt #{i})");
                                    DownloadInfo = $"Downloading (Attempt #{i}): {item.Url}";
                                }

                                try
                                {
                                    await HttpGetForLargeFile(item.Url,
                                        ApplicationManager.Instance.DownloadLocation + item.FileName, _ct);
                                }
                                catch (HttpRequestException webException)
                                {
                                    MessageBox.Show(
                                        string.Format(LanguageManager.GetValue("MessageBox.WebException",
                                            webException.Message)), "Syn3 Updater",
                                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                }

                                var validfile = ValidateFile(item.Url,
                                    ApplicationManager.Instance.DownloadLocation + item.FileName, item.Md5, false);
                                if (validfile)
                                {
                                    UpdateLog($"[Downloader] downloaded {item.FileName} and successfully validated");
                                    count++;
                                    break;
                                }

                                if (i == 3)
                                {
                                    UpdateLog(
                                        $"[Downloader] unable to successfully validate {item.FileName} after 3 tries, ABORTING PROCESS!");
                                    MessageBox.Show(
                                        string.Format(LanguageManager.GetValue("MessageBox.FailedToValidate3",
                                            item.FileName)), "Syn3 Updater", MessageBoxButton.OK,
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
                        OnPropertyChanged("DownloadQueueList");
                    });
                    count++;
                    PercentageChanged.Raise(this, 100);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    DownloadQueueList.Clear();
                    OnPropertyChanged("DownloadQueueList");
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
                    DownloadInfo = "COMPLETED SUCCESSFULLY!";
                    MessageBox.Show(LanguageManager.GetValue("MessageBox.DownloadOnlyComplete"), "Syn3 Updater",
                        MessageBoxButton.OK, MessageBoxImage.Information);
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
                var drivenumber = ApplicationManager.Instance.DriveNumber;
                var driveletter = ApplicationManager.Instance.DriveLetter;

                using (Process p = new Process())
                {
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.FileName = @"diskpart.exe";
                    p.StartInfo.CreateNoWindow = true;

                    //UpdateLog(@"Re-creating partition table as MBR and formatting as ExFat on selected USB drive");

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

            foreach (HomeViewModel.Ivsu item in ApplicationManager.Instance.Ivsus)
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DownloadQueueList.Add(ApplicationManager.Instance.DownloadLocation + item.FileName);
                    OnPropertyChanged("DownloadQueueList");
                });

            Directory.CreateDirectory($@"{ApplicationManager.Instance.DriveLetter}\SyncMyRide\");

            Task.Run(DoCopy, tokenSource.Token);
        }

        private void CreateAutoInstall()
        {
            UpdateLog("[App] Generating Autoinstall.lst");
            var autoinstalllst =
                $@"; CyanLabs Syn3Updater 2.x - Autoinstall Mode - {_selectedRelease} {_selectedRegion}{Environment.NewLine}{Environment.NewLine}[SYNCGen3.0_ALL_PRODUCT]{Environment.NewLine}";

            var extrafiles = "";
            int baseint = 0, extraint = 0;
            foreach (HomeViewModel.Ivsu item in ApplicationManager.Instance.Ivsus)
                if (item.Type == @"APPS" || item.Type == @"VOICE" ||
                    item.Type == @"ENH_DAB" || item.Type == @"MAP_LICENSE" || item.Type == @"VOICE_NAV")
                {
                    baseint++;
                    autoinstalllst +=
                        $@"Item{baseint} = {item.Type} - {item.FileName}\rOpen{baseint} = SyncMyRide\{item.FileName}\r"
                            .Replace(@"\r", Environment.NewLine);
                }
                else
                {
                    if (extrafiles == "") extrafiles = $@"[SYNCGen3.0_ALL]{Environment.NewLine}";
                    if (extraint == 10)
                    {
                        extraint = 0;
                        extrafiles +=
                            $@"Options = Delay,Include,Transaction{Environment.NewLine}[SYNCGen3.0_{_version}]{Environment.NewLine}";
                    }

                    extraint++;
                    extrafiles +=
                        $@"Item{extraint} = {item.Type} - {item.FileName}\rOpen{extraint} = SyncMyRide\{item.FileName}\r"
                            .Replace(@"\r", Environment.NewLine);
                }

            if (extrafiles != "") extrafiles += @"Options = Delay,Include,Transaction";
            autoinstalllst += $@"Options = AutoInstall{Environment.NewLine}";
            autoinstalllst += extrafiles;
            //UpdateLog($@"Creating {_mode} autoinstall.lst");
            File.WriteAllText($@"{ApplicationManager.Instance.DriveLetter}\autoinstall.lst", autoinstalllst);
            File.Create($@"{ApplicationManager.Instance.DriveLetter}\DONTINDX.MSA");
        }

        private void CreateReformat()
        {
            UpdateLog("[App] Generating reformat.lst");
            var reformatlst = "";
            var i = 0;
            foreach (HomeViewModel.Ivsu item in ApplicationManager.Instance.Ivsus)
            {
                if (item.Md5 == HomeViewModel.SyncReformatToolMd5 ||
                    item.Md5 == HomeViewModel.DowngradePackageAppMd5 && _selectedRelease != @"Sync 3.3.19052" ||
                    item.Md5 == HomeViewModel.DowngradePackageAppMd5) continue;
                i++;
                reformatlst += $@"{item.Type}={item.FileName}";
                if (i != ApplicationManager.Instance.Ivsus.Count) reformatlst += Environment.NewLine;
            }

            File.WriteAllText($@"{ApplicationManager.Instance.DriveLetter}\reformat.lst", reformatlst);

            UpdateLog("[App] Generating autoinstall.lst");
            var autoinstalllst =
                $@"; CyanLabs Syn3Updater 2.x - {InstallMode} Mode - {_selectedRelease} {_selectedRegion}{Environment.NewLine}{Environment.NewLine}[SYNCGen3.0_ALL_PRODUCT]{Environment.NewLine}";
            if (InstallMode == @"downgrade")
            {
                autoinstalllst +=
                    $@"Item1 = TOOL - {HomeViewModel.DowngradePackageToolFileName}\rOpen1 = SyncMyRide\{HomeViewModel.DowngradePackageToolFileName}\r"
                        .Replace(@"\r", Environment.NewLine);
                autoinstalllst +=
                    $@"Item2 = APP - {HomeViewModel.DowngradePackageAppFileName}\rOpen2 = SyncMyRide\{HomeViewModel.DowngradePackageAppFileName}\r"
                        .Replace(@"\r", Environment.NewLine);
                autoinstalllst += $@"Options = AutoInstall{Environment.NewLine}[SYNCGen3.0_ALL]{Environment.NewLine}";
                autoinstalllst +=
                    $@"Item1 = REFORMAT TOOL - {HomeViewModel.SyncReformatToolFileName}\rOpen1 = SyncMyRide\{HomeViewModel.SyncReformatToolFileName}\r"
                        .Replace(@"\r", Environment.NewLine);
                autoinstalllst += $@"Options = AutoInstall,Include,Transaction{Environment.NewLine}";
            }
            else if (InstallMode == @"reformat")
            {
                autoinstalllst +=
                    $@"Item1 = REFORMAT TOOL  - {HomeViewModel.SyncReformatToolFileName}\rOpen1 = SyncMyRide\{HomeViewModel.SyncReformatToolFileName}\r"
                        .Replace(@"\r", Environment.NewLine);
                autoinstalllst += @"Options = AutoInstall";
            }
            //UpdateLog($@"Creating {InstallMode} autoinstall.lst");

            File.WriteAllText($@"{ApplicationManager.Instance.DriveLetter}\autoinstall.lst", autoinstalllst);
            File.Create($@"{ApplicationManager.Instance.DriveLetter}\DONTINDX.MSA");
        }

        public string CalculateMd5(string filename)
        {
            long totalBytesRead = 0;
            using (Stream file = File.OpenRead(filename))
            {
                var size = file.Length;
                HashAlgorithm hasher = MD5.Create();
                int bytesRead;
                byte[] buffer;
                do
                {
                    buffer = new byte[4096];
                    bytesRead = file.Read(buffer, 0, buffer.Length);
                    totalBytesRead += bytesRead;
                    hasher.TransformBlock(buffer, 0, bytesRead, null, 0);
                    var read = totalBytesRead;
                    //CurrentProgress = ((int)((double)read / size * 100));
                    if (totalBytesRead % 102400 == 0) PercentageChanged.Raise(this, (int) ((double) read / size * 100));
                } while (bytesRead != 0);

                hasher.TransformFinalBlock(buffer, 0, 0);
                return BitConverter.ToString(hasher.Hash).Replace("-", string.Empty);
            }
        }

        private bool ValidateFile(string srcfile, string localfile, string md5, bool copy)
        {
            var filename = Path.GetFileName(localfile);
            if (_ct.IsCancellationRequested)
            {
                UpdateLog("[App] Process cancelled by user");
                return false;
            }

            if (ApplicationManager.Instance.SkipCheck)
            {
                UpdateLog($"[Validator] SkipCheck activated, spoofing validation check for {filename}");
                return true;
            }

            if (!File.Exists(localfile))
            {
                UpdateLog($"[Validator] {filename} is missing");
                return false;
            }

            DownloadInfo = $"Validating: {localfile}";
            progress_bar_suffix = "validated";
            var localMd5 = CalculateMd5(localfile);

            if (md5 == null)
            {
                var filesize = new FileInfo(localfile).Length;
                if (copy)
                {
                    var srcfilesize = new FileInfo(srcfile).Length;

                    if (srcfilesize == filesize)
                        if (localMd5 == CalculateMd5(srcfile))
                        {
                            UpdateLog($"[Validator] {filename} checksum matches already verified local copy");
                            return true;
                        }
                }
                else
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        long newfilesize = -1;
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, new Uri(srcfile));

                        if (long.TryParse(
                            httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _ct).Result.Content
                                .Headers.ContentLength.ToString(), out var contentLength))
                            newfilesize = contentLength;

                        if (newfilesize == filesize)
                        {
                            UpdateLog(
                                $"[Validator] no source checksum available for {filename} comparing filesize only");
                            return true;
                        }
                    }
                }
            }
            else if (string.Equals(localMd5, md5, StringComparison.CurrentCultureIgnoreCase))
            {
                UpdateLog($"[Validator] {filename} matches known good checksum");
                return true;
            }

            UpdateLog($"[Validator] {filename} failed to validate");
            return false;
        }

        public async Task HttpGetForLargeFile(string path, string filename, CancellationToken token)
        {
            using (HttpResponseMessage response = await client.GetAsync(path,
                HttpCompletionOption.ResponseHeadersRead, _ct))
            {
                var total = response.Content.Headers.ContentLength.HasValue
                    ? response.Content.Headers.ContentLength.Value
                    : -1L;

                var canReportProgress = total != -1;

                using (Stream stream = await response.Content.ReadAsStreamAsync())
                {
                    var totalRead = 0L;
                    byte[] buffer = new byte[4096];
                    var moreToRead = true;
                    const int CHUNK_SIZE = 4096;
                    FileStream fileStream = File.Create(filename, CHUNK_SIZE);
                    do
                    {
                        if (token.IsCancellationRequested)
                        {
                            fileStream.Close();
                            fileStream.Dispose();
                            try
                            {
                                File.Delete(filename);
                            }
                            catch (IOException)
                            {
                            }
                        }

                        var read = await stream.ReadAsync(buffer, 0, buffer.Length, token);

                        if (read == 0)
                        {
                            moreToRead = false;
                            fileStream.Close();
                            fileStream.Dispose();
                        }
                        else
                        {
                            byte[] data = new byte[read];
                            buffer.ToList().CopyTo(0, data, 0, read);
                            await fileStream.WriteAsync(buffer, 0, read, _ct);
                            totalRead += read;

                            if (canReportProgress)
                            {
                                var downloadPercentage = totalRead * 1d / (total * 1d) * 100;
                                var value = Convert.ToInt32(downloadPercentage);
                                PercentageChanged.Raise(this, value);
                            }
                        }
                    } while (moreToRead);
                }
            }
        }
    }
}