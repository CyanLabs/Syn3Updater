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
using Syn3Updater.Helper;
using Syn3Updater.Model;

namespace Syn3Updater.UI.Tabs
{
    internal class DownloadViewModel : LanguageAwareBaseViewModel
    {
        #region Constructors
        private ActionCommand _cancelButton;
        public ActionCommand CancelButton => _cancelButton ?? (_cancelButton = new ActionCommand(CancelAction));
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        #endregion

        #region Events
        public event EventHandler<EventArgs<int>> PercentageChanged;
        #endregion

        #region Properties & Fields
        private int _currentProgress, _totalPercentage, _totalPercentageMax, _count;
        private string _downloadInfo, _downloadPercentage, _log, _selectedRelease, _selectedRegion, _selectedMapVersion, _progressBarSuffix, _installMode;
        private bool _cancelButtonEnabled;
        private Task _downloadTask;
        private FileHelper _fileHelper;
        private CancellationToken _ct;

        private ObservableCollection<string> _downloadQueueList;
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
        #endregion

        #region Methods
        public void Init()
        {
            if (!ApplicationManager.Instance.IsDownloading || (_downloadTask != null && _downloadTask.Status.Equals(TaskStatus.Running))) return;

            _selectedRelease = ApplicationManager.Instance.SelectedRelease;
            _selectedRegion = ApplicationManager.Instance.SelectedRegion;
            _selectedMapVersion = ApplicationManager.Instance.SelectedMapVersion;
            UpdateLog($"[App] Selected Region: {_selectedRegion} - Release: {_selectedRelease} - Map Version: {_selectedMapVersion}");

            InstallMode = ApplicationManager.Instance.InstallMode;
            UpdateLog($"[App] Install mode set to {InstallMode}");

            CancelButtonEnabled = true;
            Log = "";
            CurrentProgress = 0;
            
            PercentageChanged += DownloadPercentageChanged;

            DownloadQueueList = new ObservableCollection<string>();
            foreach (SyncModel.SyncIvsu item in ApplicationManager.Instance.Ivsus) DownloadQueueList.Add(item.Url);
            
            _ct = _tokenSource.Token;
            
            _fileHelper = new FileHelper(PercentageChanged);
            _downloadTask = Task.Run(DoDownload, _tokenSource.Token);
        }

        private void DoCopy()
        {
            foreach (SyncModel.SyncIvsu item in ApplicationManager.Instance.Ivsus)
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

                        _fileHelper.copy_file(ApplicationManager.Instance.DownloadLocation + item.FileName, $@"{ApplicationManager.Instance.DriveLetter}\SyncMyRide\{item.FileName}", _ct);

                        if (ValidateFile(ApplicationManager.Instance.DownloadLocation + item.FileName,
                            $@"{ApplicationManager.Instance.DriveLetter}\SyncMyRide\{item.FileName}", item.Md5, true))
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

                Application.Current.Dispatcher.Invoke(() => DownloadQueueList.Remove(ApplicationManager.Instance.DownloadLocation + item.FileName));
                _count++;
                PercentageChanged.Raise(this, 100);
            }

            UpdateLog("[App] All files downloaded and copied to USB successfully!");
            DownloadInfo = LanguageManager.GetValue("String.Completed");
            USBHelper.GenerateLog(Log);
            if (MessageBox.Show(LanguageManager.GetValue("MessageBox.UpdateCurrentversion"), "Syn3 Updater", MessageBoxButton.YesNo, MessageBoxImage.Information) ==
                MessageBoxResult.Yes)
            {
                Properties.Settings.Default.CurrentSyncVersion = Convert.ToInt32(ApplicationManager.Instance.SelectedRelease.Replace(".", "").Replace("Sync ", ""));
                ApplicationManager.Instance.SyncVersion = ApplicationManager.Instance.SelectedRelease.Replace("Sync ", "");
            }
                
            MessageBox.Show(LanguageManager.GetValue("MessageBox.Completed"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Information);
            Process.Start($"https://cyanlabs.net/tutorials/update-ford-sync-3-2-2-3-0-to-version-3-4-all-years-3-4-19200/#{InstallMode}");
            CancelAction();
        }

        private void CancelAction()
        {
            CancelButtonEnabled = false;
            ApplicationManager.Instance.IsDownloading = false;
            _tokenSource.Cancel();
            TotalPercentage = 0;
            CurrentProgress = 0;
            DownloadInfo = "";
            DownloadPercentage = "";
            Application.Current.Dispatcher.Invoke(() =>
            {
                DownloadQueueList.Clear();
            });
            _tokenSource.Dispose();
            _tokenSource = new CancellationTokenSource();
            ApplicationManager.Instance.FireHomeTabEvent();
        }

        private void DownloadPercentageChanged(object sender, EventArgs<int> e)
        {
            DownloadPercentage = $"{e.Value}% {_progressBarSuffix}";
            CurrentProgress = e.Value;
            TotalPercentage = _count == 0 ? e.Value : _count * 100 + e.Value;
        }

        private async void DoDownload()
        {
            _count = 0;
            TotalPercentageMax = 100 * ApplicationManager.Instance.Ivsus.Count * (ApplicationManager.Instance.DownloadOnly ? 2 : 4);

            try
            {
                foreach (SyncModel.SyncIvsu item in ApplicationManager.Instance.Ivsus)
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

                                if (ValidateFile(item.Url, ApplicationManager.Instance.DownloadLocation + item.FileName, item.Md5, false))
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

                    Application.Current.Dispatcher.Invoke(() =>{DownloadQueueList.Remove(item.Url);});
                    _count++;
                    PercentageChanged.Raise(this, 100);
                }
                Application.Current.Dispatcher.Invoke(() => {DownloadQueueList.Clear();});
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
                using (Process p = new Process())
                {
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.FileName = @"diskpart.exe";
                    p.StartInfo.CreateNoWindow = true;

                    UpdateLog("[App] Re-creating partition table as MBR and formatting as ExFat on selected USB drive");

                    p.Start();
                    p.StandardInput.WriteLine($"SELECT DISK={ApplicationManager.Instance.DriveNumber}");
                    p.StandardInput.WriteLine("CLEAN");
                    p.StandardInput.WriteLine("CONVERT MBR");
                    p.StandardInput.WriteLine("CREATE PARTITION PRIMARY");
                    p.StandardInput.WriteLine("FORMAT FS=EXFAT LABEL=\"CYANLABS\" QUICK");
                    p.StandardInput.WriteLine($"ASSIGN {ApplicationManager.Instance.DriveLetter.Replace(":", "")}");
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

            foreach (SyncModel.SyncIvsu item in ApplicationManager.Instance.Ivsus)
            {
                Application.Current.Dispatcher.Invoke(() => DownloadQueueList.Add(ApplicationManager.Instance.DownloadLocation + item.FileName));
            }
            
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
            foreach (SyncModel.SyncIvsu item in ApplicationManager.Instance.Ivsus)
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
                        extrafiles += $@"Options = Delay,Include,Transaction{Environment.NewLine}[SYNCGen3.0_{ApplicationManager.Instance.SyncVersion}]{Environment.NewLine}";
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
            foreach (SyncModel.SyncIvsu item in ApplicationManager.Instance.Ivsus)
            {
                if (item.Md5 == Api.ReformatToolMd5 || item.Md5 == Api.DowngradeAppMd5 && _selectedRelease != @"Sync 3.3.19052" ||
                    item.Md5 == Api.DowngradeToolMd5) continue;
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
                    $@"Item1 = TOOL - {Api.DowngradeToolFileName}\rOpen1 = SyncMyRide\{Api.DowngradeToolFileName}\r".Replace(@"\r",
                        Environment.NewLine);
                autoinstalllst +=
                    $@"Item2 = APP - {Api.DowngradeAppFileName}\rOpen2 = SyncMyRide\{Api.DowngradeAppFileName}\r".Replace(@"\r",
                        Environment.NewLine);
                autoinstalllst += $@"Options = AutoInstall{Environment.NewLine}[SYNCGen3.0_ALL]{Environment.NewLine}";
                autoinstalllst +=
                    $@"Item1 = REFORMAT TOOL - {Api.ReformatToolFileName}\rOpen1 = SyncMyRide\{Api.ReformatToolFileName}\r".Replace(@"\r",
                        Environment.NewLine);
                autoinstalllst += $@"Options = AutoInstall,Include,Transaction{Environment.NewLine}";
            }
            else if (InstallMode == @"reformat")
            {
                autoinstalllst +=
                    $@"Item1 = REFORMAT TOOL  - {Api.ReformatToolFileName}\rOpen1 = SyncMyRide\{Api.ReformatToolFileName}\r".Replace(@"\r",
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
            FileHelper.ValidateResult validateResult = _fileHelper.validate_file(srcfile, localfile, md5, copy, _ct);
            UpdateLog(validateResult.Message);
            return validateResult.Result;
        }
        #endregion
    }
}