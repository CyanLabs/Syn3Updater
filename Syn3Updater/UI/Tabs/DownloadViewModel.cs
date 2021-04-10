using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Cyanlabs.Syn3Updater.Helper;
using Cyanlabs.Syn3Updater.Model;
using Cyanlabs.Updater.Services;

namespace Cyanlabs.Syn3Updater.UI.Tabs
{
    internal class DownloadViewModel : LanguageAwareBaseViewModel
    {
        #region Events

        public event EventHandler<EventArgs<int>> PercentageChanged;

        #endregion

        #region Constructors

        private ActionCommand _cancelButton;
        public ActionCommand CancelButton => _cancelButton ??= new ActionCommand(CancelAction);
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        #endregion

        #region Properties & Fields

        private int _currentProgress, _totalPercentage, _totalPercentageMax, _count;
        private string _downloadInfo, _downloadPercentage, _log, _selectedRelease, _selectedRegion, _selectedMapVersion, _progressBarSuffix, _installMode, _action;
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

        #endregion

        #region Methods

        public void Init()
        {
            if (!AppMan.App.IsDownloading || _downloadTask?.Status.Equals(TaskStatus.Running) == true) return;
            Log = string.Empty;
            _selectedRelease = AppMan.App.SelectedRelease;
            _selectedRegion = AppMan.App.SelectedRegion;
            _selectedMapVersion = AppMan.App.SelectedMapVersion;
            string text = $"Selected Region: {_selectedRegion} - Release: {_selectedRelease} - Map Version: {_selectedMapVersion}";
            Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";

            InstallMode = AppMan.App.InstallMode;
            _action = AppMan.App.Action;

            text = $"Install Mode: {InstallMode}";
            Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";
            AppMan.Logger.Info(text);

            CancelButtonEnabled = true;
            CurrentProgress = 0;

            PercentageChanged += DownloadPercentageChanged;

            DownloadQueueList = new ObservableCollection<string>();
            foreach (SModel.Ivsu item in AppMan.App.Ivsus) 
                DownloadQueueList.Add(item.Url);

            _ct = _tokenSource.Token;

            _fileHelper = new FileHelper(PercentageChanged);

            _downloadTask = Task.Run(DoDownload, _tokenSource.Token).ContinueWith(async t =>
           {
               if (t.IsFaulted)
               {
                   if (t.Exception != null) Application.Current.Dispatcher.Invoke(() => AppMan.Logger.CrashWindow(t.Exception.InnerExceptions.FirstOrDefault()));

                   CancelAction();
               }

               if (t.IsCompleted && !t.IsFaulted)
                   await DownloadComplete();
           }, _tokenSource.Token);
        }

        private async Task DoDownload()
        {
            _count = 0;
            TotalPercentageMax = 100 * AppMan.App.Ivsus.Count * (AppMan.App.DownloadOnly ? 2 : 4);

            foreach (SModel.Ivsu item in AppMan.App.Ivsus)
            {
                if (_ct.IsCancellationRequested)
                {
                    Log += "[" + DateTime.Now + "] Process cancelled by user" + Environment.NewLine;
                    AppMan.Logger.Info("Process cancelled by user");
                    return;
                }

                if (await ValidateFile(item.Url, AppMan.App.DownloadPath + item.FileName, item.Md5, false, true))
                {
                    string text = $"Validated: {item.FileName} (Skipping Download)";
                    Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";
                    AppMan.Logger.Info(text);

                    if (item.Source == "naviextras")
                    {
                        FileHelper.OutputResult outputResult = _fileHelper.ExtractMultiPackage(item, _ct);

                        text = $"Extracting: {item.FileName} (This may take some time!)";
                        Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";
                        AppMan.Logger.Info(text);

                        if (outputResult.Result)
                        {
                            Log += "[" + DateTime.Now + "] " + outputResult.Message + Environment.NewLine;
                            AppMan.Logger.Info(outputResult.Message);
                        }
                    }
                    _count++;
                }
                else
                {
                    if (_ct.IsCancellationRequested) return;
                    DownloadInfo = $"Downloading: {item.FileName}";

                    Log += "[" + DateTime.Now + "] " + $"Downloading: {item.FileName}" + Environment.NewLine;
                    AppMan.Logger.Info($"Downloading: {item.FileName}");

                    _progressBarSuffix = LM.GetValue("String.Downloaded");
                    try
                    {
                        string text = "";
                        for (int i = 1; i < 4; i++)
                        {
                            if (_ct.IsCancellationRequested) return;
                            if (i > 1)
                            {
                                text = $"Downloading (Attempt #{i}): {item.Url}";
                                DownloadInfo = text;
                                Log += "[" + DateTime.Now + "] " + $"Downloading: {item.FileName}" + Environment.NewLine;
                                AppMan.Logger.Info($"Downloading: {item.FileName}");
                            }
                            
                            if (!await _fileHelper.DownloadFile(item.Url, AppMan.App.DownloadPath + item.FileName, _ct))
                            {
                                CancelAction();
                                break;
                            }

                            if (await ValidateFile(item.Url, AppMan.App.DownloadPath + item.FileName, item.Md5, false))
                            {
                                text = $"Downloaded: {item.FileName}";
                                Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";
                                AppMan.Logger.Info(text);
                                if (item.Source == "naviextras")
                                {
                                    FileHelper.OutputResult outputResult = _fileHelper.ExtractMultiPackage(item, _ct);

                                    text = $"Extracting: {item.FileName} (This may take some time!)";
                                    Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";
                                    AppMan.Logger.Info(text);

                                    if (outputResult.Result)
                                    {
                                        Log += "[" + DateTime.Now + "] " + outputResult.Message + Environment.NewLine;
                                        AppMan.Logger.Info(outputResult.Message);
                                    }
                                }
                                _count++;
                                break;
                            }

                            if (i == 3)
                            {
                                text = $"unable to validate {item.FileName} after 3 tries, ABORTING PROCESS!";
                                Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";
                                AppMan.Logger.Info(text);

                                Application.Current.Dispatcher.Invoke(() =>
                                    ModernWpf.MessageBox.Show(string.Format(LM.GetValue("MessageBox.FailedToValidate3"), item.FileName), "Syn3 Updater",
                                        MessageBoxButton.OK, MessageBoxImage.Error));
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

                Application.Current.Dispatcher.Invoke(() => DownloadQueueList.Remove(item.Url));
                _count++;
                PercentageChanged.Raise(this, 100);
            }

            Application.Current.Dispatcher.Invoke(() => DownloadQueueList.Clear());
        }

        private async Task DownloadComplete()
        {
            if (!_ct.IsCancellationRequested)
            {
                if (AppMan.App.DownloadOnly)
                {
                    string text = "Process completed successfully (download only)";
                    Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";
                    AppMan.Logger.Info(text);

                    DownloadInfo = LM.GetValue("String.Completed");
                    Application.Current.Dispatcher.Invoke(() =>
                        ModernWpf.MessageBox.Show(LM.GetValue("MessageBox.DownloadOnlyComplete"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Information));
                    AppMan.App.IsDownloading = false;
                    CancelAction();
                }
                else
                {
                    await PrepareUsbAsync();
                }
            }
        }

#pragma warning disable 1998
        private async Task DoCopy()
#pragma warning restore 1998
        {
            foreach (SModel.Ivsu extraitem in AppMan.App.ExtraIvsus)
            {
                AppMan.App.Ivsus.Add(extraitem);
            }
            foreach (SModel.Ivsu item in AppMan.App.Ivsus)
            {
                if (_ct.IsCancellationRequested)
                {
                    Log += "[" + DateTime.Now + "] Process cancelled by user" + Environment.NewLine;
                    AppMan.Logger.Info("Process cancelled by user");
                    return;
                }
                if (item.Source == "naviextras")
                {
                    _count++;
                    continue;
                }
                if (await ValidateFile(AppMan.App.DownloadPath + item.FileName, $@"{AppMan.App.DriveLetter}\SyncMyRide\{item.FileName}", item.Md5,
                    true, true))
                {
                    string text = $"{item.FileName} exists and validated successfully, skipping copy";

                    Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";
                    AppMan.Logger.Info(text);

                    _count++;
                }
                else
                {
                    if (_ct.IsCancellationRequested) return;

                    string text = $"Copying: {item.FileName}";
                    DownloadInfo = text;

                    Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";
                    AppMan.Logger.Info(text);

                    _progressBarSuffix = LM.GetValue("String.Copied");

                    for (int i = 1; i < 4; i++)
                    {
                        if (_ct.IsCancellationRequested) return;
                        if (i > 1)
                        {
                            text = $"Copying (Attempt #{i}): {item.FileName}";
                            DownloadInfo = text;

                            Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";
                            AppMan.Logger.Info(text);
                        }

                        try
                        {
                            await _fileHelper.CopyFileAsync(AppMan.App.DownloadPath + item.FileName,
                                $@"{AppMan.App.DriveLetter}\SyncMyRide\{item.FileName}", _ct);
                        }
                        catch (HttpRequestException webException)
                        {
                            Application.Current.Dispatcher.Invoke(() => ModernWpf.MessageBox.Show(
                                webException.GetFullMessage(), "Syn3 Updater",
                                MessageBoxButton.OK,
                                MessageBoxImage.Exclamation));
                            AppMan.Logger.Info("ERROR: " + webException.GetFullMessage());
                            CancelAction();
                        }
                        catch (IOException ioException)
                        {
                            Application.Current.Dispatcher.Invoke(() => ModernWpf.MessageBox.Show(
                                ioException.GetFullMessage(), "Syn3 Updater",
                                MessageBoxButton.OK,
                                MessageBoxImage.Exclamation));
                            AppMan.Logger.Info("ERROR: " + ioException.GetFullMessage());
                            CancelAction();
                        }

                        if (await ValidateFile(AppMan.App.DownloadPath + item.FileName,
                            $@"{AppMan.App.DriveLetter}\SyncMyRide\{item.FileName}", item.Md5, true))
                        {
                            text = $"Copied: {item.FileName}";
                            Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";
                            AppMan.Logger.Info(text);
                            _count++;
                            break;
                        }

                        if (i == 3)
                        {
                            text = $"unable to validate {item.FileName} after 3 tries, ABORTING PROCESS!";
                            Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";
                            AppMan.Logger.Info(text);

                            Application.Current.Dispatcher.Invoke(() => ModernWpf.MessageBox.Show(
                                string.Format(LM.GetValue("MessageBox.FailedToValidate3"), item.FileName), "Syn3 Updater", MessageBoxButton.OK,
                                MessageBoxImage.Error));
                            CancelAction();
                            break;
                        }
                    }
                }

                Application.Current.Dispatcher.Invoke(() => DownloadQueueList.Remove(AppMan.App.DownloadPath + item.FileName));
                _count++;
                PercentageChanged.Raise(this, 100);
            }
        }

        private void CopyComplete()
        {
            if (_action == "main")
            {
                switch (InstallMode)
                {
                    case "autoinstall":
                        CreateAutoInstall();
                        break;
                    case "downgrade":
                    case "reformat":
                        CreateReformat();
                        break;
                }
            }
            else if (_action == "logutility" || _action == "gracenotesremoval" || _action == "voiceshrinker" || _action == "downgrade")
                CreateAutoInstall();

            CancelButtonEnabled = false;
            string text = String.Empty;
            if (AppMan.App.DownloadToFolder)
            {
                text = "ALL FILES DOWNLOADED AND COPIED TO THE SELECTED FOLDER SUCCESSFULLY!";
            }
            else
            {
                text = "ALL FILES DOWNLOADED AND COPIED TO THE USB DRIVE SUCCESSFULLY!";
            }

            Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";
            AppMan.Logger.Info(text);

            DownloadInfo = LM.GetValue("String.Completed");
            AppMan.App.IsDownloading = false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                USBHelper.GenerateLog(Log, ModernWpf.MessageBox.Show(LM.GetValue("MessageBox.UploadLog"), "Syn3 Updater", MessageBoxButton.YesNo,
                MessageBoxImage.Information) == MessageBoxResult.Yes);

                if (_action == "main")
                {
                    if (ModernWpf.MessageBox.Show(string.Format(LM.GetValue("MessageBox.UpdateCurrentversion"), AppMan.App.SVersion, AppMan.App.SelectedRelease.Replace("Sync ", "")), "Syn3 Updater", MessageBoxButton.YesNo,
                        MessageBoxImage.Information) == MessageBoxResult.Yes)
                    {
                        AppMan.App.Settings.CurrentVersion =
                            Convert.ToInt32(AppMan.App.SelectedRelease.Replace(".", "").Replace("Sync ", ""));
                        AppMan.App.SVersion = AppMan.App.SelectedRelease.Replace("Sync ", "");
                    }

                    if (AppMan.App.DownloadToFolder)
                    {
                        ModernWpf.MessageBox.Show(LM.GetValue("MessageBox.CompletedFolder"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Information);
                        Process.Start(AppMan.App.DriveLetter);
                    }
                    else
                    {
                        ModernWpf.MessageBox.Show(LM.GetValue("MessageBox.Completed"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Information);
                        Process.Start($"https://cyanlabs.net/tutorials/windows-automated-method-update-to-3-4/#{InstallMode}");
                    }

                    AppMan.App.FireHomeTabEvent();
                }
                else if (_action == "logutility")
                {
                    ModernWpf.MessageBox.Show(LM.GetValue("MessageBox.LogUtilityComplete"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Information);
                    AppMan.App.UtilityCreateLogStep1Complete = true;
                    AppMan.App.FireUtilityTabEvent();
                }
                else if (_action == "gracenotesremoval" || _action == "voiceshrinker" || _action == "downgrade")
                {
                    ModernWpf.MessageBox.Show(LM.GetValue("MessageBox.GenericUtilityComplete"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Information);
                    AppMan.App.FireUtilityTabEvent();
                }
            });
            Reset();
        }

        private void CancelAction()
        {
            CancelButtonEnabled = false;
            AppMan.App.IsDownloading = false;
            _tokenSource.Cancel();
            TotalPercentage = 0;
            CurrentProgress = 0;
            DownloadInfo = "";
            DownloadPercentage = "";
            Application.Current.Dispatcher.Invoke(() => DownloadQueueList.Clear());
            AppMan.App.AppsSelected = false;
            AppMan.App.SkipFormat = false;
            _tokenSource.Dispose();
            _tokenSource = new CancellationTokenSource();
            AppMan.App.FireHomeTabEvent();
        }

        private void Reset()
        {
            CancelButtonEnabled = false;
            AppMan.App.IsDownloading = false;
            _tokenSource.Cancel();
            TotalPercentage = 0;
            CurrentProgress = 0;
            DownloadInfo = "";
            DownloadPercentage = "";
            Application.Current.Dispatcher.Invoke(() => DownloadQueueList.Clear());
            AppMan.App.AppsSelected = false;
            AppMan.App.SkipFormat = false;
            _tokenSource.Dispose();
            _tokenSource = new CancellationTokenSource();
        }

        private void DownloadPercentageChanged(object sender, EventArgs<int> e)
        {
            DownloadPercentage = $"{e.Value}% {_progressBarSuffix}";
            CurrentProgress = e.Value;
            TotalPercentage = _count == 0 ? e.Value : (_count * 100) + e.Value;
        }

        private async Task PrepareUsbAsync()
        {
            if (AppMan.App.DownloadToFolder)
            {
                Log += "[" + DateTime.Now + "] Preparing selected directory (No USB Drive Selected)" + Environment.NewLine;
                AppMan.Logger.Info("Preparing selected directory  (No USB Drive Selected)");
            }
            else
            {
                Log += "[" + DateTime.Now + "] Preparing USB drive" + Environment.NewLine;
                AppMan.Logger.Info("Preparing USB drive");
            }

            if (AppMan.App.DownloadToFolder)
            {
                foreach (string file in Directory.GetFiles(AppMan.App.DriveLetter))
                    File.Delete(file);
                foreach (string dir in Directory.GetDirectories(AppMan.App.DriveLetter))
                    Directory.Delete(dir, true);
            }
            else
            {
                if (!AppMan.App.SkipFormat && !AppMan.App.DownloadOnly)
                {
                    Log += "[" + DateTime.Now + "] Formatting USB drive" + Environment.NewLine;
                    AppMan.Logger.Info("Formatting USB drive");
                    using (Process p = new Process())
                    {
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.RedirectStandardInput = true;
                        p.StartInfo.FileName = "diskpart.exe";
                        p.StartInfo.CreateNoWindow = true;

                        Log += "[" + DateTime.Now + "] Re-creating partition table as MBR and formatting as ExFat on selected USB drive" + Environment.NewLine;
                        AppMan.Logger.Info("Re-creating partition table as MBR and formatting as ExFat on selected USB drive");

                        p.Start();
                        p.StandardInput.WriteLine($"SELECT DISK={AppMan.App.DriveNumber}");
                        p.StandardInput.WriteLine("CLEAN");
                        p.StandardInput.WriteLine("CONVERT MBR");
                        p.StandardInput.WriteLine("CREATE PARTITION PRIMARY");
                        p.StandardInput.WriteLine("FORMAT FS=EXFAT LABEL=\"CYANLABS\" QUICK");
                        p.StandardInput.WriteLine($"ASSIGN LETTER={AppMan.App.DriveLetter.Replace(":", "")}");
                        p.StandardInput.WriteLine("EXIT");

                        p.WaitForExit();
                    }

                    Thread.Sleep(5000);
                }
            }

            foreach (SModel.Ivsu item in AppMan.App.Ivsus)
                Application.Current.Dispatcher.Invoke(() => DownloadQueueList.Add(AppMan.App.DownloadPath + item.FileName));

            Directory.CreateDirectory($@"{AppMan.App.DriveLetter}\SyncMyRide\");
            await DoCopy().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    if (t.Exception != null) 
                        Application.Current.Dispatcher.Invoke(() => AppMan.Logger.CrashWindow(t.Exception.InnerExceptions.FirstOrDefault()));
                    CancelAction();
                }

                if (t.IsCompleted && !t.IsFaulted)
                    CopyComplete();
            }, _tokenSource.Token);

        }

        private void CreateAutoInstall()
        {
            Log += "[" + DateTime.Now + "] Generating Autoinstall.lst" + Environment.NewLine;
            AppMan.Logger.Info("Generating Autoinstall.lst");
            StringBuilder autoinstalllst = DownloadViewModelService.CreateAutoInstallFile(_selectedRelease, _selectedRegion);
            File.WriteAllText($@"{AppMan.App.DriveLetter}\autoinstall.lst", autoinstalllst.ToString());
            File.Create($@"{AppMan.App.DriveLetter}\DONTINDX.MSA");
        }

    

        private void CreateReformat()
        {
            Log += "[" + DateTime.Now + "] Generating reformat.lst" + Environment.NewLine;
            AppMan.Logger.Info("Generating reformat.lst");

            string reformatlst = "";
            int i = 0;
            foreach (SModel.Ivsu item in AppMan.App.Ivsus)
            {
                if (item.Source == "naviextras") continue;
                if (InstallMode == "reformat")
                {
                    if (item.Md5 == Api.ReformatTool.Md5) continue;
                }
                else if (InstallMode == "downgrade")
                {
                    if (item.Md5 == Api.ReformatTool.Md5 || (item.Md5 == Api.DowngradeApp.Md5 && _selectedRelease != "Sync 3.3.19052") || item.Md5 == Api.DowngradeTool.Md5)
                        continue;
                }
                i++;
                reformatlst += $"{item.Type}={item.FileName}";
                if (i != AppMan.App.Ivsus.Count)
                    reformatlst += Environment.NewLine;
            }

            File.WriteAllText($@"{AppMan.App.DriveLetter}\reformat.lst", reformatlst);

            Log += "[" + DateTime.Now + "] Generating autoinstall.lst" + Environment.NewLine;
            AppMan.Logger.Info("Generating autoinstall.lst");

            var autoinstalllst = new StringBuilder(
                $@"; CyanLabs Syn3Updater {Assembly.GetEntryAssembly()?.GetName().Version} {AppMan.App.LauncherPrefs.ReleaseTypeInstalled} - {InstallMode} {(AppMan.App.ModeForced ? "FORCED " : "")} Mode - {_selectedRelease} {_selectedRegion}{Environment.NewLine}{Environment.NewLine}[SYNCGen3.0_ALL_PRODUCT]{Environment.NewLine}");
            if (InstallMode == "downgrade")
            {
                autoinstalllst.Append(
                    $@"Item1 = TOOL - {Api.DowngradeTool.FileName}\rOpen1 = SyncMyRide\{Api.DowngradeTool.FileName}\r").Replace(@"\r",
                        Environment.NewLine);
                autoinstalllst.Append(
                    $@"Item2 = APP - {Api.DowngradeApp.FileName}\rOpen2 = SyncMyRide\{Api.DowngradeApp.FileName}\r").Replace(@"\r",
                        Environment.NewLine);
                autoinstalllst.Append($@"Options = AutoInstall{Environment.NewLine}[SYNCGen3.0_ALL]{Environment.NewLine}");
                autoinstalllst.Append($@"Item1 = REFORMAT TOOL - {Api.ReformatTool.FileName}\rOpen1 = SyncMyRide\{Api.ReformatTool.FileName}\r").Replace(@"\r", Environment.NewLine);
                autoinstalllst.Append("Options = AutoInstall,Include,Transaction").Append(Environment.NewLine);
            }
            else if (InstallMode == "reformat")
            {
                autoinstalllst.Append($@"Item1 = REFORMAT TOOL  - {Api.ReformatTool.FileName}\rOpen1 = SyncMyRide\{Api.ReformatTool.FileName}\r").Replace(@"\r", Environment.NewLine);
                autoinstalllst.Append("Options = AutoInstall");
            }

            File.WriteAllText($@"{AppMan.App.DriveLetter}\autoinstall.lst", autoinstalllst.ToString());
            File.Create($@"{AppMan.App.DriveLetter}\DONTINDX.MSA");
        }

        private async Task<bool> ValidateFile(string srcfile, string localfile, string md5, bool copy, bool existing = false)
        {
            string text = $"Validating: {Path.GetFileName(localfile)}";
            if (existing)
                text = $"Checking Existing File: {Path.GetFileName(localfile)}";
            DownloadInfo = text;
            Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";
            AppMan.Logger.Info(text);

            _progressBarSuffix = LM.GetValue("String.Validated");
            FileHelper.OutputResult outputResult = await _fileHelper.ValidateFile(srcfile, localfile, md5, copy, _ct);

            if (outputResult.Message != "")
            {
                Log += "[" + DateTime.Now + "] " + outputResult.Message + Environment.NewLine;
                AppMan.Logger.Info(outputResult.Message);
            }

            return outputResult.Result;
        }

        #endregion
    }
}