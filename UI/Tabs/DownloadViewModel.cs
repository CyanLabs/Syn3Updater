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
using Syn3Updater.Helpers;
using Syn3Updater.Model;

namespace Syn3Updater.UI.Tabs
{
    public class DownloadViewModel : LanguageAwareBaseViewModel
    {
        public event EventHandler<EventArgs<int>> PercentageChanged;
        private string _version;
        CancellationToken ct;
        private string SelectedRelease, SelectedRegion, SelectedMapVersion;

        private void UpdateLog(string text)
        {
            Log += text + Environment.NewLine;
            ApplicationManager.Logger.Info(text);
        }
        public void Init()
        {
            SelectedRelease = ApplicationManager.Instance.SelectedRelease;
            SelectedRegion = ApplicationManager.Instance.SelectedRegion;
            SelectedMapVersion = ApplicationManager.Instance.SelectedMapVersion;
            UpdateLog($"[App] Selected Region: {SelectedRegion} - Release: {SelectedRelease} - Map Version: {SelectedMapVersion}");
            
            CancelButtonEnabled = true;
            InstallMode = ApplicationManager.Instance.InstallMode;
            UpdateLog($"[App] Install mode set to {InstallMode}");
            OnPropertyChanged("InstallMode");
            PercentageChanged += DownloadPercentageChanged;
            CurrentProgress = 0;

            DownloadQueueList = new ObservableCollection<string>();
            foreach (HomeViewModel.Ivsu item in ApplicationManager.Instance.Ivsus)
            {
                DownloadQueueList.Add(item.Url);
            }
            OnPropertyChanged("DownloadQueueList");

            _version = Properties.Settings.Default.CurrentSyncVersion.ToString();
            _version = $"{_version[0]}.{_version[1]}.{_version.Substring(2, _version.Length - 2)}";

            

            ct = tokenSource.Token;
            Task.Run(DoDownload, tokenSource.Token);
        }

        private void DoCopy()
        {

            int total = ApplicationManager.Instance.Ivsus.Count;
            foreach (HomeViewModel.Ivsu item in ApplicationManager.Instance.Ivsus)
            {
                if (ct.IsCancellationRequested)
                {
                    UpdateLog("[App] Process cancelled by user");
                    return;
                }
                if (ValidateFile(ApplicationManager.Instance.DownloadLocation + item.FileName,
                    ApplicationManager.Instance.DriveLetter + @"\SyncMyRide\" + item.FileName, item.Md5, true))
                {
                    UpdateLog($"[Copier] {item.FileName} exists and successfully validated, skipping copy");
                    count++;
                }
                else
                {
                    UpdateLog($"[Copier] {item.FileName} is missing or invalid, copying");
                    DownloadInfo = "Copying: " + item.FileName;
                    progress_bar_suffix = "copied";

                    for (int i = 1; i < 4; i++)
                    {
                        if (i > 1)
                        {
                            UpdateLog($"[Copier] {item.FileName} is missing or invalid, copying (Attempt #{i})");
                            DownloadInfo = "Copying (Attempt #" + i + "): " + item.FileName;
                        }
                        int bufferSize = 1024 * 512;
                        using (FileStream inStream =
                            new FileStream(ApplicationManager.Instance.DownloadLocation + item.FileName, FileMode.Open,
                                FileAccess.Read, FileShare.ReadWrite))
                        using (FileStream fileStream = new FileStream(ApplicationManager.Instance.DriveLetter + @"\SyncMyRide\" + item.FileName,
                            FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            int bytesRead = -1;
                            var totalReads = 0;
                            var totalBytes = inStream.Length;
                            byte[] bytes = new byte[bufferSize];
                            int prevPercent = 0;

                            while ((bytesRead = inStream.Read(bytes, 0, bufferSize)) > 0)
                            {
                                if (ct.IsCancellationRequested)
                                {
                                    fileStream.Close();
                                    fileStream.Dispose();
                                    try
                                    {
                                        File.Delete(ApplicationManager.Instance.DriveLetter + @"\SyncMyRide\" + item.FileName);
                                    }
                                    catch (IOException)
                                    {

                                    }
                                }
                                fileStream.Write(bytes, 0, bytesRead);
                                totalReads += bytesRead;
                                int percent =
                                    System.Convert.ToInt32(((decimal) totalReads / (decimal) totalBytes) * 100);
                                if (percent != prevPercent)
                                {
                                    PercentageChanged.Raise(this, percent);
                                    prevPercent = percent;
                                }
                            }
                        }

                        bool validfile = ValidateFile(ApplicationManager.Instance.DownloadLocation + item.FileName,
                            ApplicationManager.Instance.DriveLetter + @"\SyncMyRide\" + item.FileName, item.Md5, true);
                        if (validfile)
                        {
                            UpdateLog($"[Copier] copied {item.FileName} and successfully validated");
                            count++;
                            break;
                        }
                        if (i == 3)
                        {
                            UpdateLog($"[Copier] unable to successfully validate {item.FileName} after 3 tries, ABORTING PROCESS!");
                            MessageBox.Show(LanguageManager.GetValue(string.Format("MessageBox.FailedToValidate3", item.FileName)), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Error);
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
                PercentageChanged.Raise(this,100);
            }

            DownloadInfo = "";
            DownloadPercentage = "";

            GenerateLog();
            UpdateLog("[App] All files downloaded and copied to USB successfully!");
            DownloadInfo = "COMPLETED SUCCESSFULLY";
            if (MessageBox.Show(LanguageManager.GetValue("MessageBox.UpdateCurrentversion"), "Syn3 Updater", MessageBoxButton.YesNo,
                MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                Properties.Settings.Default.CurrentSyncVersion = Convert.ToInt32(ApplicationManager.Instance.SelectedRelease.Replace(".", "").Replace("Sync ", ""));
            }
            MessageBox.Show(LanguageManager.GetValue("MessageBox.Completed"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Information);
            Process.Start("https://cyanlabs.net/tutorials/update-ford-sync-3-2-2-3-0-to-version-3-4-all-years-3-4-19200/#" + InstallMode);
            CancelAction();
        }

        public static string GetOsFriendlyName()
        {
            string result = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
            foreach (var o in searcher.Get())
            {
                var os = (ManagementObject)o;
                result = os["Caption"].ToString();
                break;
            }
            return result + " (" + Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId", "") + ")";
        }

        private void GenerateLog()
        {
            string data = $@"CYANLABS - SYN3 UPDATER - V{Assembly.GetExecutingAssembly().GetName().Version}" + Environment.NewLine;
            data += @"Operating System: " + GetOsFriendlyName() + Environment.NewLine;
            data += Environment.NewLine;
            data += @"PREVIOUS CONFIGURATION" + Environment.NewLine;
            data += @"Version: " + _version + Environment.NewLine;
            data += @"Region: " + Properties.Settings.Default.CurrentSyncRegion + Environment.NewLine;
            data += @"Navigation: " + Properties.Settings.Default.CurrentSyncNav + Environment.NewLine;
            data += @"Mode: " + (Properties.Settings.Default.CurrentInstallMode == @"autodetect" ? InstallMode : Properties.Settings.Default.CurrentInstallMode + " FORCED") + Environment.NewLine;
            data += Environment.NewLine;
            data += @"USB DETAILS" + Environment.NewLine;
            data += @"Model: " + ApplicationManager.Instance.DriveName + Environment.NewLine;
            data += @"FileSystem: " + ApplicationManager.Instance.DriveFileSystem + Environment.NewLine;
            data += @"Partition Type: " + ApplicationManager.Instance.DrivePartitionType + Environment.NewLine;

            string driveletter = ApplicationManager.Instance.DriveLetter;
            if (File.Exists(driveletter + @"\reformat.lst"))
            {
                data += Environment.NewLine;
                data += @"REFORMAT.LST" + Environment.NewLine;
                data += File.ReadAllText(driveletter + @"\reformat.lst") + Environment.NewLine;
            }

            if (File.Exists(driveletter + @"\autoinstall.lst"))
            {
                data += Environment.NewLine;
                data += @"AUTOINSTALL.LST" + Environment.NewLine;
                data += File.ReadAllText(driveletter + @"\autoinstall.lst") + Environment.NewLine;
            }

            if (Directory.Exists(driveletter + @"\SyncMyRide"))
            {
                data += Environment.NewLine;
                DirectoryInfo di = new DirectoryInfo(driveletter + @"\SyncMyRide");
                FileInfo[] allFiles = di.GetFiles("*", SearchOption.AllDirectories);
                data += @"SYNCMYRIDE FILES (" + allFiles.Length + ")" + Environment.NewLine;
                foreach (FileInfo file in allFiles)
                    data += $"{file.Name} ({Functions.BytesToString(file.Length)})" + Environment.NewLine;
                data += Environment.NewLine;
            }

            data += @"LOG" + Environment.NewLine;
            data += Log;
            File.WriteAllText(driveletter + @"\log.txt", data);
        }

        private string progress_bar_suffix = "";

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
            DownloadPercentage = e.Value + "% " + progress_bar_suffix;
            CurrentProgress = e.Value;
            TotalPercentage = count == 0 ? e.Value : (count * 100) + e.Value;
        }

        CancellationTokenSource tokenSource = new CancellationTokenSource();
        int count;
        private async void DoDownload()
        {
            int total = ApplicationManager.Instance.Ivsus.Count;
            count = 0;

            if (ApplicationManager.Instance.DownloadOnly)
            {
                TotalPercentageMax = (100 * total) * 2;
            } 
            else
            {
                TotalPercentageMax = (100 * total) * 4;
            }
            
            try
            {
                foreach (HomeViewModel.Ivsu item in ApplicationManager.Instance.Ivsus)
                {
                    if (ct.IsCancellationRequested)
                    {
                        UpdateLog("[App] Process cancelled by user");
                        return;
                    }
                    if (ValidateFile(item.Url, ApplicationManager.Instance.DownloadLocation + item.FileName, item.Md5, false))
                    {
                        UpdateLog($"[Downloader] {item.FileName} exists and successfully validated, skipping download");
                        count++;
                    }
                    else
                    {
                        UpdateLog($"[Downloader] {item.FileName} is missing or invalid, downloading");
                        DownloadInfo = "Downloading: " + item.Url;
                        progress_bar_suffix = "downloaded";
                        try
                        {
                            for (int i = 1; i < 4; i++) 
                            {
                                if (i > 1)
                                {
                                    UpdateLog($"[Downloader] {item.FileName} is missing or invalid, downloading (Attempt #{i})");
                                    DownloadInfo = "Downloading (Attempt #" + i + "): " + item.Url;
                                }

                                try
                                {
                                    await HttpGetForLargeFile(item.Url,
                                        ApplicationManager.Instance.DownloadLocation + item.FileName, ct);
                                }
                                catch (System.Net.Http.HttpRequestException webException)
                                {
                                    MessageBox.Show(LanguageManager.GetValue(string.Format("MessageBox.WebException", webException.Message)), "Syn3 Updater",
                                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                }
                                bool validfile = ValidateFile(item.Url, ApplicationManager.Instance.DownloadLocation + item.FileName, item.Md5, false);
                                if (validfile)
                                {
                                    UpdateLog($"[Downloader] downloaded {item.FileName} and successfully validated");
                                    count++;
                                    break;
                                }
                                if (i == 3)
                                {
                                    UpdateLog($"[Downloader] unable to successfully validate {item.FileName} after 3 tries, ABORTING PROCESS!");
                                    MessageBox.Show(LanguageManager.GetValue(string.Format("MessageBox.FailedToValidate3", item.FileName)), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Error);
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
            catch (System.InvalidOperationException)
            {
                //
            }

            if (ct.IsCancellationRequested == false)
            {
                if (ApplicationManager.Instance.DownloadOnly)
                {
                    UpdateLog("[App] Process completed successfully (download only)");
                    DownloadInfo = "COMPLETED SUCCESSFULLY!";
                    MessageBox.Show(LanguageManager.GetValue("MessageBox.DownloadOnlyComplete"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Information);
                    ApplicationManager.Instance.IsDownloading = false;
                    CancelAction();
                }
                else
                    PrepareUsb();
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

                    //UpdateLog(@"Re-creating partition table as MBR and formatting as ExFat on selected USB drive");

                    p.Start();
                    p.StandardInput.WriteLine("SELECT DISK=" + drivenumber);
                    p.StandardInput.WriteLine("CLEAN");
                    p.StandardInput.WriteLine("CONVERT MBR");
                    p.StandardInput.WriteLine("CREATE PARTITION PRIMARY");
                    p.StandardInput.WriteLine("FORMAT FS=EXFAT LABEL=\"CYANLABS\" QUICK");
                    p.StandardInput.WriteLine("ASSIGN " + driveletter.Replace(":", ""));
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
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DownloadQueueList.Add(ApplicationManager.Instance.DownloadLocation + item.FileName);
                    OnPropertyChanged("DownloadQueueList");
                });
            }

            Directory.CreateDirectory(ApplicationManager.Instance.DriveLetter + @"\SyncMyRide\");

            Task.Run(DoCopy, tokenSource.Token);
        }

        private void CreateAutoInstall()
        {
            UpdateLog("[App] Generating Autoinstall.lst");
            string autoinstalllst = @"; CyanLabs Syn3Updater 2.x - Autoinstall Mode - " +
                                    SelectedRelease + " " + SelectedRegion +
                                    Environment.NewLine + Environment.NewLine + @"[SYNCGen3.0_ALL_PRODUCT]" +
                                    Environment.NewLine;

            string extrafiles = "";
            int baseint = 0, extraint = 0;
            foreach (HomeViewModel.Ivsu item in ApplicationManager.Instance.Ivsus)
            {
                if (item.Type == @"APPS" || item.Type == @"VOICE" ||
                    item.Type == @"ENH_DAB" || item.Type == @"MAP_LICENSE" || item.Type == @"VOICE_NAV")
                {
                    baseint++;
                    autoinstalllst += string.Format(@"Item{0} = {1} - {2}\rOpen{0} = SyncMyRide\{2}\r", baseint,
                            item.Type, item.FileName)
                        .Replace(@"\r", Environment.NewLine);
                }
                else
                {
                    if (extrafiles == "")
                    {
                        extrafiles = @"[SYNCGen3.0_ALL]" + Environment.NewLine;
                    }
                    if (extraint == 10)
                    {
                        extraint = 0;
                        extrafiles += @"Options = Delay,Include,Transaction" + Environment.NewLine + @"[SYNCGen3.0_" + _version + "]" + Environment.NewLine;
                    }
                    extraint++;
                    extrafiles += string.Format(@"Item{0} = {1} - {2}\rOpen{0} = SyncMyRide\{2}\r", extraint,
                            item.Type, item.FileName)
                        .Replace(@"\r", Environment.NewLine);

                }
            }

            if (extrafiles != "")
            {
                extrafiles += @"Options = Delay,Include,Transaction";

            }
            autoinstalllst += @"Options = AutoInstall" + Environment.NewLine;
            autoinstalllst += extrafiles;
            //UpdateLog($@"Creating {_mode} autoinstall.lst");
            File.WriteAllText(ApplicationManager.Instance.DriveLetter + @"\autoinstall.lst", autoinstalllst);
            File.Create(ApplicationManager.Instance.DriveLetter + @"\DONTINDX.MSA");
        }

        private void CreateReformat()
        {
            UpdateLog("[App] Generating reformat.lst");
            string SelectedRelease = ApplicationManager.Instance.SelectedRelease;
            string SelectedRegion = ApplicationManager.Instance.SelectedRegion;
            string SelectedMapVersion = ApplicationManager.Instance.SelectedMapVersion;
            string reformatlst = "";
            int i = 0;
            foreach (HomeViewModel.Ivsu item in ApplicationManager.Instance.Ivsus)
            {
                if (item.Md5 == HomeViewModel.SyncReformatToolMd5 || (item.Md5 == HomeViewModel.DowngradePackageAppMd5 && SelectedRelease != @"Sync 3.3.19052") || item.Md5 == HomeViewModel.DowngradePackageAppMd5) continue;
                i++;
                reformatlst += item.Type + @"=" + item.FileName;
                if (i != ApplicationManager.Instance.Ivsus.Count) reformatlst += Environment.NewLine;
            }
            File.WriteAllText(ApplicationManager.Instance.DriveLetter + @"\reformat.lst", reformatlst);

            UpdateLog("[App] Generating autoinstall.lst");
            string autoinstalllst = @"; CyanLabs Syn3Updater 2.x - " + InstallMode + @" Mode - " + SelectedRelease + " " + SelectedRegion + Environment.NewLine + Environment.NewLine + @"[SYNCGen3.0_ALL_PRODUCT]" + Environment.NewLine;
            if (InstallMode == @"downgrade")
            {
                autoinstalllst += string.Format(@"Item1 = TOOL - {0}\rOpen1 = SyncMyRide\{0}\r", HomeViewModel.DowngradePackageToolFileName).Replace(@"\r", Environment.NewLine);
                autoinstalllst += string.Format(@"Item2 = APP - {0}\rOpen2 = SyncMyRide\{0}\r", HomeViewModel.DowngradePackageAppFileName).Replace(@"\r", Environment.NewLine);
                autoinstalllst += @"Options = AutoInstall" + Environment.NewLine + @"[SYNCGen3.0_ALL]" + Environment.NewLine;
                autoinstalllst += string.Format(@"Item1 = REFORMAT TOOL - {0}\rOpen1 = SyncMyRide\{0}\r", HomeViewModel.SyncReformatToolFileName).Replace(@"\r", Environment.NewLine);
                autoinstalllst += @"Options = AutoInstall,Include,Transaction" + Environment.NewLine;
            }
            else if (InstallMode == @"reformat")
            {
                autoinstalllst += string.Format(@"Item1 = REFORMAT TOOL  - {0}\rOpen1 = SyncMyRide\{0}\r", HomeViewModel.SyncReformatToolFileName).Replace(@"\r", Environment.NewLine);
                autoinstalllst += @"Options = AutoInstall";
            }
            //UpdateLog($@"Creating {InstallMode} autoinstall.lst");

            File.WriteAllText(ApplicationManager.Instance.DriveLetter + @"\autoinstall.lst", autoinstalllst);
            File.Create(ApplicationManager.Instance.DriveLetter + @"\DONTINDX.MSA");
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
                    if (totalBytesRead % 102400 == 0)
                    {
                        PercentageChanged.Raise(this, ((int)((double)read / size * 100)));
                    }

                } while (bytesRead != 0);

                hasher.TransformFinalBlock(buffer, 0, 0);
                return BitConverter.ToString(hasher.Hash).Replace("-", String.Empty);
            }
        }

        private bool ValidateFile(string srcfile, string localfile, string md5, bool copy)
        {
            string filename = Path.GetFileName(localfile);
            if (ct.IsCancellationRequested)
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
            DownloadInfo = "Validating: " + localfile;
            progress_bar_suffix = "validated";
            string localMd5 = CalculateMd5(localfile);

            if (md5 == null)
            {
                long filesize = new FileInfo(localfile).Length;
                if (copy)
                {
                    long srcfilesize = new FileInfo(srcfile).Length;

                    if (srcfilesize == filesize)
                    {
                        if (localMd5 == CalculateMd5(srcfile))
                        {
                            UpdateLog($"[Validator] {filename} checksum matches already verified local copy");
                            return true;
                        }
                    }
                }
                else
                {
                    using (HttpClient client = new HttpClient())
                    {
                        long newfilesize = -1;
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, new Uri(srcfile));

                        if (long.TryParse(client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).Result.Content.Headers.ContentLength.ToString(), out long contentLength))
                            newfilesize = contentLength;

                        if (newfilesize == filesize)
                        {
                            UpdateLog($"[Validator] no source checksum available for {filename} comparing filesize only");
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

        //https://www.technical-recipes.com/2018/reporting-the-percentage-progress-of-large-file-downloads-in-c-wpf/
        private static readonly HttpClient client = new HttpClient();
        public async Task HttpGetForLargeFile(string path, string filename, CancellationToken token)
        {
            using (HttpResponseMessage response = await client.GetAsync(path,
                   HttpCompletionOption.ResponseHeadersRead, ct))
            {
                var total = response.Content.Headers.ContentLength.HasValue ?
                   response.Content.Headers.ContentLength.Value : -1L;

                var canReportProgress = total != -1;

                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    var totalRead = 0L;
                    var buffer = new byte[4096];
                    var moreToRead = true;
                    const int CHUNK_SIZE = 4096;
                    var fileStream = File.Create(filename, CHUNK_SIZE);
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
                            if (fileStream != null)
                            {
                                fileStream.Close();
                                fileStream.Dispose();
                            }
                        }
                        else
                        {
                            var data = new byte[read];
                            buffer.ToList().CopyTo(0, data, 0, read);
                            await fileStream.WriteAsync(buffer, 0, read, ct);
                            totalRead += read;
                            
                        if (canReportProgress)
                            {
                                var downloadPercentage = ((totalRead * 1d) / (total * 1d)) * 100;
                                var value = Convert.ToInt32(downloadPercentage);
                                PercentageChanged.Raise(this, value);
                            }
                        }
                    }
                    while (moreToRead);
                }
            }
        }

        private ActionCommand _cancelButton;
        public ActionCommand CancelButton => _cancelButton ?? (_cancelButton = new ActionCommand(CancelAction));
        public ObservableCollection<string> DownloadQueueList { get; set; }
        public bool CancelButtonEnabled { get; set; }

        private string _downloadFilesRemaining;
        public string DownloadFilesRemaining
        {
            get => _downloadFilesRemaining;
            set => SetProperty(ref _downloadFilesRemaining, value);
        }

        public string InstallMode { get; set; }

        private int _currentProgress;
        public int CurrentProgress
        {
            get => _currentProgress;
            set => SetProperty(ref _currentProgress, value);
        }
        
        private string _downloadPercentage;
        public string DownloadPercentage
        {
            get => _downloadPercentage;
            set => SetProperty(ref _downloadPercentage, value);
        }

        private int _totalPercentage;
        public int TotalPercentage
        {
            get => _totalPercentage;
            set => SetProperty(ref _totalPercentage, value);
        }

        private int _totalPercentageMax;
        public int TotalPercentageMax
        {
            get => _totalPercentageMax;
            set => SetProperty(ref _totalPercentageMax, value);
        }

        private string _downloadInfo;
        public string DownloadInfo
        {
            get => _downloadInfo;
            set => SetProperty(ref _downloadInfo, value);
        }

        private string _log;
        public string Log
        {
            get => _log;
            set => SetProperty(ref _log, value);
        }
    }
}