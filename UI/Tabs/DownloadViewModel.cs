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

        private const string SyncReformatTool = "1u5t-14g386-cb.tar.gz";
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private ObservableCollection<FileList> fileListIvsus = new ObservableCollection<FileList>();
        public event EventHandler<EventArgs<int>> PercentageChanged;
        private BackgroundWorker _worker = new BackgroundWorker();
        private List<string> DownloadQueue = new List<string>();

        public void Init()
        {
            InstallMode = Properties.Settings.Default.CurrentInstallMode;
            OnPropertyChanged("InstallMode");

            PercentageChanged += OnPercentageChanged;
            _worker.DoWork += DoWork;
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;

            CurrentProgress = 0;

            DownloadQueueList = new List<string>();
            //DownloadQueue.Add("http://127.0.0.1/swparts/4U5T-14G423-AB_1598633764000.TAR.GZ");

            foreach (HomeViewModel.Ivsu item in ApplicationManager.Instance._ivsus)
            {
                if (!ValidateFile(item.Url, ApplicationManager.Instance.DownloadLocation + item.FileName, item.Md5, false))
                {
                    DownloadQueueList.Add(item.Url);
                    DownloadQueue.Add(item.Url);
                }


                if (!fileListIvsus.Any(p => p.FileName == item.FileName)) fileListIvsus.Add(new FileList() { FileName = item.FileName, Type = item.Type, Md5 = item.Md5 });

            }

            TotalPercentageMax = 100 * DownloadQueue.Count;
            _worker.RunWorkerAsync();

            OnPropertyChanged("DownloadQueueList");
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
            DownloadQueueList.Clear();
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
            var total = DownloadQueue.Count;
            count = 0;
            
            foreach (var url in DownloadQueue)
            {
                //  Do the download  
                DownloadInfo = "Downloading: " + url;
                string _fileName = url.Substring(url.LastIndexOf("/", StringComparison.Ordinal) + 1,
                    url.Length - url.LastIndexOf("/", StringComparison.Ordinal) - 1);

                try
                {
                    await HttpGetForLargeFile(url,ApplicationManager.Instance.DownloadLocation + _fileName, tokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                DownloadQueueList.Remove(url);
                OnPropertyChanged("DownloadQueueList");
                CurrentProgress = 100 / (total - count);
                count++;

            }
            if (_worker.CancellationPending)
            {
                e.Cancel = true;
            }
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
                } while (bytesRead != 0);

                hasher.TransformFinalBlock(buffer, 0, 0);
                return BitConverter.ToString(hasher.Hash).Replace("-", String.Empty);
            }
        }

        private bool ValidateFile(string srcfile, string localfile, string md5, bool copy)
        {
            if (ApplicationManager.Instance.Skipcheck) return true;
            if (!File.Exists(localfile)) return false;

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

        public List<string> DownloadQueueList { get; set; }

        public class FileList
        {
            public string FileName { get; set; }
            public string Type { get; set; }
            public string Md5 { get; set; }
        }

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