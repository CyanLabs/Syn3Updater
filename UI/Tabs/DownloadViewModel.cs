using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Syn3Updater.Helper;
using Syn3Updater.Helpers;
using Syn3Updater.Model;

namespace Syn3Updater.UI.Tabs
{
    public class DownloadViewModel : LanguageAwareBaseViewModel
    {

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        public event EventHandler<EventArgs<int>> PercentageChanged;
        private BackgroundWorker _worker = new BackgroundWorker();
        private List<string> DownloadQueue = new List<string>();
        private string _version;
        public void Init()
        {
            InstallMode = ApplicationManager.Instance.InstallMode;
            OnPropertyChanged("InstallMode");

            PercentageChanged += OnPercentageChanged;
            _worker.DoWork += DoWork;
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;

            CurrentProgress = 0;

            DownloadQueueList = new ObservableCollection<string>();

            foreach (HomeViewModel.Ivsu item in ApplicationManager.Instance._ivsus)
            {
                DownloadQueueList.Add(item.Url);
            }
            OnPropertyChanged("DownloadQueueList");

            _version = Properties.Settings.Default.CurrentSyncVersion.ToString();
            _version = $"{_version[0]}.{_version[1]}.{_version.Substring(2, _version.Length - 2)}";

            _worker.RunWorkerAsync();
        }

        private void CancelDownloadAction()
        {
            tokenSource.Cancel();
            _worker.CancelAsync();
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
            tokenSource.Dispose(); // Clean up old token source....
            tokenSource = new CancellationTokenSource(); // "Reset" the cancellation token source...
            ApplicationManager.Instance.FireHomeTabEvent();
        }

        private void OnPercentageChanged(object sender, EventArgs<int> e)
        {
            DownloadPercentage = e.Value + "%" + " downloaded";
            CurrentProgress = e.Value;
            TotalPercentage = count == 0 ? CurrentProgress : (count * 100) + e.Value;
        }

        CancellationTokenSource tokenSource = new CancellationTokenSource();
        int count = 0;
        private async void DoWork(object sender, DoWorkEventArgs e)
        {
            int total = ApplicationManager.Instance._ivsus.Count;
            count = 0;
            TotalPercentageMax = 100 * total;

            //TODO Handle verification of file that has just downloaded
            foreach (HomeViewModel.Ivsu item in ApplicationManager.Instance._ivsus)
            {
                if (!ValidateFile(item.Url, ApplicationManager.Instance.DownloadLocation + item.FileName, item.Md5, false))
                {
                    DownloadInfo = "Downloading: " + item.Url;
                    try
                    {
                        for (int i = 1; i < 4; i++)
                        {
                            if(i > 1) DownloadInfo = "Downloading (Attempt #" + i + "): " + item.Url;
                            await HttpGetForLargeFile(item.Url,
                                ApplicationManager.Instance.DownloadLocation + item.FileName, tokenSource.Token);
                            
                            bool validfile = ValidateFile(item.Url, ApplicationManager.Instance.DownloadLocation + item.FileName, item.Md5, false);
                            if (validfile) break;
                            if (i == 3)
                            {
                                MessageBox.Show("Something has gone wrong!");
                                CancelDownloadAction();
                                break;
                            }
                            TotalPercentageMax += 100;
                            count++;
                            
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
            }

            //Downloads complete
            if (ApplicationManager.Instance._downloadonly)
                MessageBox.Show(LanguageManager.GetValue("MessageBox.DownloadOnlyComplete"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                PrepareUsb();

            if (_worker.CancellationPending)
            {
                e.Cancel = true;
            }
        }

        private void PrepareUsb()
        {
            string drivenumber = ApplicationManager.Instance.drivenumber;
            string driveletter = ApplicationManager.Instance.driveletter;

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
                p.StandardInput.WriteLine("ASSIGN " + driveletter.Replace(":",""));
                p.StandardInput.WriteLine("EXIT");

                p.WaitForExit();
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

            foreach (HomeViewModel.Ivsu item in ApplicationManager.Instance._ivsus)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DownloadQueueList.Add(item.Url);
                    OnPropertyChanged("DownloadQueueList");
                });
            }

            Directory.CreateDirectory(ApplicationManager.Instance.driveletter + @"\SyncMyRide\");

            //CopyFiles();
        }

        private void CreateAutoInstall()
        {
            string SelectedRelease = ApplicationManager.Instance.selectedrelease;
            string SelectedRegion  = ApplicationManager.Instance.selectedregion;
            string SelectedMapVersion = ApplicationManager.Instance.selectedmapversion;
            string autoinstalllst = @"; CyanLabs Syn3Updater 2.x - Autoinstall Mode - " +
                                    SelectedRelease + " " + SelectedRegion +
                                    Environment.NewLine + Environment.NewLine + @"[SYNCGen3.0_ALL_PRODUCT]" +
                                    Environment.NewLine;

            string extrafiles = "";
            int baseint = 0, extraint = 0;
            foreach (HomeViewModel.Ivsu item in ApplicationManager.Instance._ivsus)
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
            File.WriteAllText(ApplicationManager.Instance.driveletter + @"\autoinstall.lst", autoinstalllst);
            File.Create(ApplicationManager.Instance.driveletter + @"\DONTINDX.MSA");
        }

        private void CreateReformat()
        {
            string SelectedRelease = ApplicationManager.Instance.selectedrelease;
            string SelectedRegion = ApplicationManager.Instance.selectedregion;
            string SelectedMapVersion = ApplicationManager.Instance.selectedmapversion;
            string reformatlst = "";
            int i = 0;
            foreach (HomeViewModel.Ivsu item in ApplicationManager.Instance._ivsus)
            {
                if (item.Md5 == HomeViewModel.SyncReformatToolMd5 || (item.Md5 == HomeViewModel.DowngradePackageAppMd5 && SelectedRelease != @"Sync 3.3.19052") || item.Md5 == HomeViewModel.DowngradePackageAppMd5) continue;
                i++;
                reformatlst += item.Type + @"=" + item.FileName;
                if (i != ApplicationManager.Instance._ivsus.Count) reformatlst += Environment.NewLine;
            }

            //UpdateLog($@"Creating {_mode} reformat.lst");

            File.WriteAllText(ApplicationManager.Instance.driveletter + @"\reformat.lst", reformatlst);
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

            File.WriteAllText(ApplicationManager.Instance.driveletter + @"\autoinstall.lst", autoinstalllst);
            File.Create(ApplicationManager.Instance.driveletter + @"\DONTINDX.MSA");
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
                    CurrentProgress = ((int)((double)read / size * 100));
                    TotalPercentage = (count * 100) + ((int)((double)read / size * 100));
                } while (bytesRead != 0);

                hasher.TransformFinalBlock(buffer, 0, 0);
                return BitConverter.ToString(hasher.Hash).Replace("-", String.Empty);
            }
        }

        private bool ValidateFile(string srcfile, string localfile, string md5, bool copy)
        {
            if (ApplicationManager.Instance.Skipcheck) return true;
            if (!File.Exists(localfile)) return false;
            DownloadInfo = "Validating: " + localfile;
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

                        if (long.TryParse(client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result.Content.Headers.ContentLength.ToString(), out long contentLength))
                            newfilesize = contentLength;

                        if (newfilesize == filesize)
                        {
                            return true;
                        }
                    }
                }
            }
            else if (string.Equals(localMd5, md5, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            return false;
        }

        //https://www.technical-recipes.com/2018/reporting-the-percentage-progress-of-large-file-downloads-in-c-wpf/
        private static HttpClient client = new HttpClient();
        public async Task HttpGetForLargeFile(string path, string filename, CancellationToken token)
        {
            using (HttpResponseMessage response = await client.GetAsync(path,
                   HttpCompletionOption.ResponseHeadersRead))
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
                            await fileStream.WriteAsync(buffer, 0, read);
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

        private ActionCommand _cancelDownload;
        public ActionCommand CancelDownload => _cancelDownload ?? (_cancelDownload = new ActionCommand(CancelDownloadAction));

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
    }


}