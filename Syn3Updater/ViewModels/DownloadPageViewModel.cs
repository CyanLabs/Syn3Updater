using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using Syn3Updater.Helpers;
using Syn3Updater.Models;
using Syn3Updater.Services;

namespace Syn3Updater.ViewModels
{
    public class DownloadPageViewModel : ViewModelBase
    {
        public event EventHandler<EventArgs<int>> PercentageChanged;
        private CancellationTokenSource _tokenSource = new();
        private CancellationToken _ct;
        private int _currentProgress, _totalPercentage, _totalPercentageMax, _count;
        private string? _downloadInfo, _downloadPercentage, _log, _selectedRelease, _selectedRegion, _progressBarSuffix, _installMode, _action, _my20Mode;
        private bool _cancelButtonEnabled;
        private FileHelper _fileHelper;

        private ObservableCollection<string>? _downloadQueueList;

        public ObservableCollection<string>? DownloadQueueList
        {
            get => _downloadQueueList;
            set => this.RaiseAndSetIfChanged(ref _downloadQueueList, value);
        }

        public bool CancelButtonEnabled
        {
            get => _cancelButtonEnabled;
            set => this.RaiseAndSetIfChanged(ref _cancelButtonEnabled, value);
        }

        public string? InstallMode
        {
            get => _installMode;
            set => this.RaiseAndSetIfChanged(ref _installMode, value);
        }

        private string? _installModeForced;

        public string? InstallModeForced
        {
            get => _installModeForced;
            set => this.RaiseAndSetIfChanged(ref _installModeForced, value);
        }

        public int CurrentProgress
        {
            get => _currentProgress;
            set => this.RaiseAndSetIfChanged(ref _currentProgress, value);
        }

        public string? DownloadPercentage
        {
            get => _downloadPercentage;
            set => this.RaiseAndSetIfChanged(ref _downloadPercentage, value);
        }

        public int TotalPercentage
        {
            get => _totalPercentage;
            set => this.RaiseAndSetIfChanged(ref _totalPercentage, value);
        }

        public int TotalPercentageMax
        {
            get => _totalPercentageMax;
            set => this.RaiseAndSetIfChanged(ref _totalPercentageMax, value);
        }

        public string? DownloadInfo
        {
            get => _downloadInfo;
            set => this.RaiseAndSetIfChanged(ref _downloadInfo, value);
        }

        public string? Log
        {
            get => _log;
            set => this.RaiseAndSetIfChanged(ref _log, value);
        }

        public string? My20Mode
        {
            get => _my20Mode;
            set => this.RaiseAndSetIfChanged(ref _my20Mode, value);
        }

        private string? _downloadConnections;
        public string? DownloadConnections
        {
            get => _downloadConnections;
            set => this.RaiseAndSetIfChanged(ref _downloadConnections, value);
        }

        public DownloadPageViewModel()
        {
            AppMan.App.StartDownloadsTab += delegate
            {
                DownloadConnections = AppMan.App.Settings.DownloadConnections.ToString();
                My20Mode = AppMan.App.Settings.My20 switch
                {
                    null => "AutoDetect",
                    true => "Enabled",
                    false => "Disabled"
                };
                InstallModeForced = AppMan.App.ModeForced ? "Yes" : "No";
                InstallMode = AppMan.App.InstallMode;
                Log = string.Empty;
                _selectedRelease = AppMan.App.SelectedRelease;
                _selectedRegion = AppMan.App.SelectedRegion;
                string selectedMapVersion = AppMan.App.SelectedMapVersion;
                string text = $"Selected Region: {_selectedRegion} - Release: {_selectedRelease} - Map Version: {selectedMapVersion}";
                Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";

                _action = AppMan.App.Action;

                text = $"Install Mode: {AppMan.App.Settings.InstallMode} ({InstallMode}) Forced: {AppMan.App.ModeForced}";
                Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";

                text = $"MY20 Protection: {My20Mode}";
                Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";

                CancelButtonEnabled = true;
                CurrentProgress = 0;

                PercentageChanged += DownloadPercentageChanged!;

                DownloadQueueList = new ObservableCollection<string>();
                foreach (SModel.Ivsu item in AppMan.App.Ivsus)
                    DownloadQueueList.Add(item.Url);

                _ct = _tokenSource.Token;

                _fileHelper = new FileHelper(PercentageChanged);

                DoDownloadCopyTask();
            };
        }

        private async void DoDownloadCopyTask()
        {
            bool doDownload;

            if (!AppMan.App.DownloadOnly)
            {
                if (OperatingSystem.IsWindows())
                {
                    if (await FormatUSBWindowsAsync() != true) return;
                } else if (OperatingSystem.IsMacOS())
                {
                    if (await FormatUSBMacAsync() != true) return;
                }
            }
                

            try
            {
                doDownload = await Task.Run(DoDownload, _tokenSource.Token);
            }
            catch (Exception)
            {
                CancelAction();
                AppMan.App.FireHomeTabEvent();
                return;
            }

            if (!doDownload)
            {
                CancelAction();
                AppMan.App.FireHomeTabEvent();
                return;
            }

            CurrentProgress = 100;
            TotalPercentage = AppMan.App.DownloadOnly ? TotalPercentageMax : TotalPercentage / 2;
            if (AppMan.App.DownloadOnly && !_ct.IsCancellationRequested)
            {
                DownloadPercentage = "";
                DownloadInfo = "";
                AppMan.App.IsDownloading = false;
                //TODO MessageBox.DownloadOnlyComplete
                AppMan.App.FireHomeTabEvent();
            }
            else if (!_ct.IsCancellationRequested)
            {
                if (PrepareUsbAsync() != true) return;
                bool doCopy;
                try
                {
                    doCopy = await Task.Run(DoCopy, _tokenSource.Token);
                }
                catch (Exception)
                {
                    CancelAction();
                    AppMan.App.FireHomeTabEvent();
                    return;
                }

                if (!doCopy)
                {
                    CancelAction();
                    AppMan.App.FireHomeTabEvent();
                    return;
                }
                await CopyComplete();
            }
        }

        //TODO Refactor DoDownload
        private async Task<bool> DoDownload()
        {
            _count = 0;
            if (!Directory.Exists(AppMan.App.DownloadPath)) Directory.CreateDirectory(AppMan.App.DownloadPath);
            TotalPercentageMax = 100 * AppMan.App.Ivsus.Count * (AppMan.App.DownloadOnly ? 2 : 4);
            foreach (SModel.Ivsu item in AppMan.App.Ivsus)
            {
                if (_ct.IsCancellationRequested)
                {
                    Log += $"[{DateTime.Now}] Process cancelled by user{Environment.NewLine}";
                    return false;
                }

                if (await Task.Run(async () => await ValidateFile(item.Url, AppMan.App.DownloadPath + item.FileName, item.Md5, false, true), _ct))
                {
                    string text = $"Validated: {item.FileName} (Skipping Download)";
                    Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";

                    if (item.Source == @"naviextras")
                    {
                        FileHelper.OutputResult outputResult = _fileHelper.ExtractMultiPackage(item, _ct);

                        text = $"Extracting: {item.FileName} (This may take some time!)";
                        Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";

                        if (outputResult.Result)
                        {
                            Log += "[" + DateTime.Now + "] " + outputResult.Message + Environment.NewLine;
                        }
                    }

                    _count += 2;
                }
                else
                {
                    if (_ct.IsCancellationRequested) return false;
                    DownloadInfo = $"Downloading: {item.FileName}";

                    Log += "[" + DateTime.Now + "] " + $"Downloading: {item.FileName}" + Environment.NewLine;

                    _progressBarSuffix = "Downloaded";
                    try
                    {
                        for (int i = 1; i < 4; i++)
                        {
                            if (_ct.IsCancellationRequested) return false;
                            string text;
                            if (i > 1)
                            {
                                DownloadInfo = $"Downloading (Attempt #{i}): {item.Url}";
                                Log += "[" + DateTime.Now + "] " + $"Downloading (Attempt #{i}): {item.FileName}" + Environment.NewLine;
                            }

                            if (await _fileHelper.DownloadFile(item.Url, AppMan.App.DownloadPath + item.FileName, _ct, AppMan.App.Settings.DownloadConnections))
                            {
                                _count++;
                            }
                            else
                            {
                                return false;
                            }

                            if (await Task.Run(async () => await ValidateFile(item.Url, AppMan.App.DownloadPath + item.FileName, item.Md5, false)))
                            {
                                _count++;
                                text = $"Downloaded: {item.FileName}";
                                Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";
                                if (item.Source == @"naviextras")
                                {
                                    FileHelper.OutputResult outputResult = _fileHelper.ExtractMultiPackage(item, _ct);

                                    text = $"Extracting: {item.FileName} (This may take some time!)";
                                    Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";

                                    if (outputResult.Result)
                                    {
                                        Log += "[" + DateTime.Now + "] " + outputResult.Message + Environment.NewLine;
                                    }
                                }

                                break;
                            }

                            if (i != 3) continue;
                            text = $"Unable to validate {item.FileName} after 3 attempts, ABORTING PROCESS!";
                            Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";
                            //TODO MessageBox.FailedToValidate3
                            return false;
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }

                DownloadQueueList?.Remove(item.Url);
            }

            DownloadQueueList?.Clear();
            return true;
        }

        //TODO Refactor DoCopy
        private async Task<bool> DoCopy()
        {
            foreach (SModel.Ivsu extraitem in AppMan.App.ExtraIvsus) AppMan.App.Ivsus.Add(extraitem);
            foreach (SModel.Ivsu item in AppMan.App.Ivsus)
            {
                if (_ct.IsCancellationRequested)
                {
                    Log += "[" + DateTime.Now + "] Process cancelled by user" + Environment.NewLine;
                    return false;
                }

                if (item.Source == @"naviextras")
                {
                    _count++;
                    continue;
                }

                if (await Task.Run(async () => await ValidateFile(AppMan.App.DownloadPath + item.FileName, $@"{AppMan.App.DrivePath}{Path.DirectorySeparatorChar}SyncMyRide{Path.DirectorySeparatorChar}{item.FileName}", item.Md5,
                    true, true), _ct))
                {
                    string text = $"{item.FileName} exists and validated successfully, skipping copy";
                    Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";

                    _count += 2;
                }
                else
                {
                    if (_ct.IsCancellationRequested) return false;

                    DownloadInfo = $"Copying: {item.FileName}";

                    Log += $"[{DateTime.Now}] Copying: {item.FileName} {Environment.NewLine}";

                    _progressBarSuffix = "Copied";

                    for (int i = 1; i < 4; i++)
                    {
                        if (_ct.IsCancellationRequested) return false;
                        if (i > 1)
                        {
                            DownloadInfo = $"Copying (Attempt #{i}): {item.FileName}";
                            Log += $"[{DateTime.Now}] Copying (Attempt #{i}): {item.FileName} {Environment.NewLine}";
                        }

                        if (await _fileHelper.CopyFileAsync(AppMan.App.DownloadPath + item.FileName, $@"{AppMan.App.DrivePath}{Path.DirectorySeparatorChar}SyncMyRide{Path.DirectorySeparatorChar}{item.FileName}", _ct))
                        {
                            _count++;
                        }
                        else
                        {
                            return false;
                        }

                        if (await Task.Run(async () => await ValidateFile(AppMan.App.DownloadPath + item.FileName,
                            $@"{AppMan.App.DrivePath}{Path.DirectorySeparatorChar}SyncMyRide{Path.DirectorySeparatorChar}{item.FileName}", item.Md5, true), _ct))
                        {
                            string text = $"Copied: {item.FileName}";
                            Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";
                            _count++;
                            break;
                        }

                        if (i != 3) continue;
                        {
                            string text = $"unable to validate {item.FileName} after 3 tries, ABORTING PROCESS!";
                            Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";

                            //TODO MessageBox.FailedToValidate3
                            return false;
                        }
                    }
                }

                DownloadQueueList?.Remove(AppMan.App.DownloadPath + item.FileName);
                PercentageChanged.Raise(this, 100, 0);
            }

            return true;
        }

        private async Task CopyComplete()
        {
            switch (_action)
            {
                case "main":
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

                    break;
                case "logutility":
                case "logutilitymy20":
                case "gracenotesremoval":
                case "voiceshrinker":
                case "downgrade":
                    CreateAutoInstall();
                    break;
            }

            CancelButtonEnabled = false;
            string text = AppMan.App.DownloadToFolder
                ? "ALL FILES DOWNLOADED AND COPIED TO THE SELECTED FOLDER SUCCESSFULLY!"
                : "ALL FILES DOWNLOADED AND COPIED TO THE USB DRIVE SUCCESSFULLY!";

            Log += $"[{DateTime.Now}] {text} {Environment.NewLine}";

            DownloadInfo = "Completed";
            AppMan.App.IsDownloading = false;

            if (_action != "logutilitymy20" && _action != "logutility")
            {
                //TODO Confirm Log Upload
                //ContentDialogResult result = await Application.Current.Dispatcher.Invoke(() => UIHelper.ShowDialog(LM.GetValue("MessageBox.UploadLog"), LM.GetValue("String.Notice"), LM.GetValue("String.No"), LM.GetValue("String.Upload")));
                await USBHelper.GenerateLog(Log, true);
            }

            if (_action == "main")
            {
                if (_selectedRelease != "Only Update Maps")
                {
                    //TODO Confirm Log Upload
                    AppMan.App.Settings.CurrentVersion = Convert.ToInt32(AppMan.App.SelectedRelease.Replace(".", "").Replace("Sync ", ""));
                    AppMan.App.SVersion = AppMan.App.SelectedRelease.Replace("Sync ", "");
                }

                if (AppMan.App.DownloadToFolder)
                {
                    //TODO Display Folder Completion Message 
                    try
                    {
                        Process.Start(AppMan.App.DrivePath);
                    }
                    catch (Exception)
                    {
                        //ignored
                    }
                    
                }
                else
                {
                   //TODO Display USB Drive Completion Message 
                   Process.Start($"https://cyanlabs.net/tutorials/windows-automated-method-update-to-3-4/#{InstallMode}");
                }

                AppMan.App.FireHomeTabEvent();
            }
            else if (_action == "logutility")
            {
                AppMan.App.FireHomeTabEvent();
                AppMan.App.FireInterrogatorLogCompleted();
            }
            else if (_action == "gracenotesremoval" || _action == "voiceshrinker" || _action == "downgrade")
            {
                //TODO Display MessageBox.GenericUtilityComplete Message 
                //AppMan.App.FireUtilityTabEvent();
            }
            CancelAction();
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
            DownloadQueueList?.Clear();
            AppMan.App.AppsSelected = false;
            AppMan.App.SkipFormat = false;
            _tokenSource.Dispose();
            _tokenSource = new CancellationTokenSource();
        }

        private readonly ConcurrentDictionary<int, int> _parts = new();

        private void DownloadPercentageChanged(object sender, EventArgs<int> e)
        {
            if (e.Part == 0)
            {
                CurrentProgress = e.Value;
                DownloadPercentage = $"{e.Value}% {_progressBarSuffix}";
                TotalPercentage = _count * 100 + e.Value;
            }
            else
            {
                if (_parts.TryGetValue(e.Part, out int _))
                    _parts[e.Part] = e.Value;
                else
                    _parts.TryAdd(e.Part, e.Value);

                double downloadPercentage = Convert.ToInt32(_parts.Sum(part => part.Value) * 1d / (AppMan.App.Settings.DownloadConnections * 1d));
                CurrentProgress = Convert.ToInt32(downloadPercentage);
                DownloadPercentage = $"{CurrentProgress}% {_progressBarSuffix}";
                TotalPercentage = _count * 100 + CurrentProgress;
            }
        }

        private async Task<bool> FormatUSBWindowsAsync()
        {
            if (AppMan.App.SkipFormat) return true;
            if (AppMan.App.DownloadToFolder)
            {
                Log += "[" + DateTime.Now + "] Clearing Selected Folder" + Environment.NewLine;
                try
                {
                    foreach (string file in Directory.GetFiles(AppMan.App.DrivePath))
                        File.Delete(file);
                    foreach (string dir in Directory.GetDirectories(AppMan.App.DrivePath))
                        Directory.Delete(dir, true);
                }
                catch (Exception)
                {
                    Log += "[" + DateTime.Now + "] Unable to clear folder, continuing anyway" + Environment.NewLine;
                    return false;
                }
            }
            else
            {
                Log += "[" + DateTime.Now + "] Formatting USB drive" + Environment.NewLine;
                using (Process p = new())
                {
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.FileName = "diskpart.exe";
                    p.StartInfo.CreateNoWindow = true;

                    Log += "[" + DateTime.Now + "] Re-creating partition table as MBR and formatting as ExFat on selected USB drive" + Environment.NewLine;

                    p.Start();
                    await p.StandardInput.WriteLineAsync($"SELECT DISK={AppMan.App.DriveNumber}");
                    await p.StandardInput.WriteLineAsync("CLEAN");
                    await p.StandardInput.WriteLineAsync("CONVERT MBR");
                    await p.StandardInput.WriteLineAsync("CREATE PARTITION PRIMARY");
                    await p.StandardInput.WriteLineAsync("FORMAT FS=EXFAT LABEL=\"CYANLABS\" QUICK");
                    await p.StandardInput.WriteLineAsync($"ASSIGN LETTER={AppMan.App.DrivePath.Replace(":", "")}");
                    await p.StandardInput.WriteLineAsync("EXIT");

                    p.WaitForExit();
                }

                await Task.Delay(5000);
            }
            return true;
        }
        
        private async Task<bool> FormatUSBMacAsync()
        {
            if (AppMan.App.SkipFormat) return true;
            if (AppMan.App.DownloadToFolder)
            {
                Log += "[" + DateTime.Now + "] Clearing Selected Folder" + Environment.NewLine;
                try
                {
                    foreach (string file in Directory.GetFiles(AppMan.App.DrivePath))
                        File.Delete(file);
                    foreach (string dir in Directory.GetDirectories(AppMan.App.DrivePath))
                        Directory.Delete(dir, true);
                }
                catch (Exception)
                {
                    Log += "[" + DateTime.Now + "] Unable to clear folder, continuing anyway" + Environment.NewLine;
                    return false;
                }
            }
            else
            {
                Log += "[" + DateTime.Now + "] Formatting USB drive" + Environment.NewLine;
                using (Process p = new())
                {
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.FileName = "diskutil";
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.Arguments = $"eraseDisk ExFat CYANLABS MBR {AppMan.App.DrivePath}";
                    p.StartInfo.CreateNoWindow = true;

                    Log += "[" + DateTime.Now + "] Re-creating partition table as MBR and formatting as ExFat on selected USB drive" + Environment.NewLine;

                    p.Start();
                    while (!p.StandardOutput.EndOfStream)
                    {
                        Log += "[" + DateTime.Now + "] DiskUtil: " + p.StandardOutput.ReadLine() + Environment.NewLine;
                    }
                    p.WaitForExit();
                }

                if (Log.Contains($"Could not mount {AppMan.App.DrivePath}s1 after erase"))
                {
                    using (Process p = new())
                    {
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.FileName = "diskutil";
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.Arguments = $"mount {AppMan.App.DrivePath}s1";
                        p.StartInfo.CreateNoWindow = true;

                        Log += "[" + DateTime.Now + "] Mounting newly created partition on selected USB drive" + Environment.NewLine;

                        p.Start();
                        while (!p.StandardOutput.EndOfStream)
                        {
                            Log += "[" + DateTime.Now + "] DiskUtil: " + p.StandardOutput.ReadLine() + Environment.NewLine;
                        }
                        p.WaitForExit();
                    }
                }

                AppMan.App.DrivePath = "/Volumes/CYANLABS";
            }
            return true;
        }

        private bool PrepareUsbAsync()
        {
            if (AppMan.App.DownloadToFolder)
            {
                Log += "[" + DateTime.Now + "] Preparing selected directory (No USB Drive Selected)" + Environment.NewLine;
            }
            else
            {
                Log += "[" + DateTime.Now + "] Preparing USB drive" + Environment.NewLine;
            }
            
            foreach (SModel.Ivsu item in AppMan.App.Ivsus)
                DownloadQueueList?.Add(AppMan.App.DownloadPath + item.FileName);

            try
            {
                Directory.CreateDirectory($@"{AppMan.App.DrivePath}{Path.DirectorySeparatorChar}SyncMyRide{Path.DirectorySeparatorChar}");
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
            
            return true;
        }

        private void CreateAutoInstall()
        {
            string autoinstalllst; 
            if (string.IsNullOrEmpty(AppMan.App.AutoInstall))
            {
                Log += "[" + DateTime.Now + "] Generating Autoinstall.lst" + Environment.NewLine;
                autoinstalllst = DownloadViewModelService.CreateAutoInstallFile(_selectedRelease, _selectedRegion).ToString();
            }
            else
            {
                Log += "[" + DateTime.Now + "] Using Autoinstall.lst Template" + Environment.NewLine;
                autoinstalllst = AppMan.App.AutoInstall;
            }
            try
            {
                File.WriteAllText($@"{AppMan.App.DrivePath}{Path.DirectorySeparatorChar}autoinstall.lst", autoinstalllst);
                File.Create($@"{AppMan.App.DrivePath}{Path.DirectorySeparatorChar}DONTINDX.MSA").Close();
            }
            catch (IOException)
            {
            }
        }

        private void CreateReformat()
        {
            Log += "[" + DateTime.Now + "] Generating reformat.lst" + Environment.NewLine;

            string reformatlst = "";
            int i = 0;
            foreach (SModel.Ivsu item in AppMan.App.Ivsus)
            {
                if (item.Source == "naviextras") continue;
                switch (InstallMode)
                {
                    case "reformat" when item.Md5 == Api.ReformatTool.Md5:
                    case "downgrade" when item.Md5 == Api.ReformatTool.Md5 || item.Md5 == Api.DowngradeAppIvsu.Md5 && _selectedRelease != "Sync 3.3.19052" || item.Md5 == Api.DowngradeToolIvsu.Md5:
                        continue;
                }

                i++;
                reformatlst += $"{item.Type}={item.FileName}";
                if (i != AppMan.App.Ivsus.Count)
                    reformatlst += Environment.NewLine;
            }

            try
            {
                File.WriteAllText($@"{AppMan.App.DrivePath}{Path.DirectorySeparatorChar}reformat.lst", reformatlst);
            }
            catch (IOException)
            {
               
            }

            Log += "[" + DateTime.Now + "] Generating autoinstall.lst" + Environment.NewLine;

            StringBuilder autoinstalllst = new(
                $@"; CyanLabs Syn3Updater {Assembly.GetEntryAssembly()?.GetName().Version} - {InstallMode} {(AppMan.App.ModeForced ? "FORCED " : "")} Mode - {_selectedRelease} {_selectedRegion}{Environment.NewLine}{Environment.NewLine}[SYNCGen3.0_ALL_PRODUCT]{Environment.NewLine}");
            switch (InstallMode)
            {
                case "downgrade":
                    autoinstalllst.Append(
                        $@"Item1 = TOOL - {Api.DowngradeToolIvsu.FileName}\rOpen1 = SyncMyRide\{Api.DowngradeToolIvsu.FileName}\r").Replace(@"\r",
                        Environment.NewLine);
                    autoinstalllst.Append(
                        $@"Item2 = APP - {Api.DowngradeAppIvsu.FileName}\rOpen2 = SyncMyRide\{Api.DowngradeAppIvsu.FileName}\r").Replace(@"\r",
                        Environment.NewLine);
                    autoinstalllst.Append($@"Options = AutoInstall{Environment.NewLine}[SYNCGen3.0_ALL]{Environment.NewLine}");
                    autoinstalllst.Append($@"Item1 = REFORMAT TOOL - {Api.ReformatTool.FileName}\rOpen1 = SyncMyRide\{Api.ReformatTool.FileName}\r")
                        .Replace(@"\r", Environment.NewLine);
                    autoinstalllst.Append("Options = AutoInstall,Include,Transaction").Append(Environment.NewLine);
                    break;
                case "reformat":
                    autoinstalllst.Append($@"Item1 = REFORMAT TOOL  - {Api.ReformatTool.FileName}\rOpen1 = SyncMyRide\{Api.ReformatTool.FileName}\r")
                        .Replace(@"\r", Environment.NewLine);
                    autoinstalllst.Append("Options = AutoInstall");
                    break;
            }

            try
            {
                File.WriteAllText($@"{AppMan.App.DrivePath}{Path.DirectorySeparatorChar}autoinstall.lst", autoinstalllst.ToString());
                File.Create($@"{AppMan.App.DrivePath}{Path.DirectorySeparatorChar}DONTINDX.MSA").Close();
            }
            catch (IOException)
            {
               
            }
        }

        private async Task<bool> ValidateFile(string srcfile, string localfile, string? md5, bool copy, bool existing = false)
        {
            if (existing)
            {
                DownloadInfo = $"Checking Existing File: {Path.GetFileName(localfile)}";
                Log += $"[{DateTime.Now}] Checking Existing File: {Path.GetFileName(localfile)} {Environment.NewLine}";
            }
            else
            {
                DownloadInfo = $"Validating: {Path.GetFileName(localfile)}";
                Log += $"[{DateTime.Now}] Validating: {Path.GetFileName(localfile)} {Environment.NewLine}";
            }

            _progressBarSuffix = "Validated";
            FileHelper.OutputResult outputResult = await _fileHelper.ValidateFile(srcfile, localfile, md5, copy, _ct);

            if (outputResult.Message != "")
            {
                Log += "[" + DateTime.Now + "] " + outputResult.Message + Environment.NewLine;
            }

            return outputResult.Result;
        }
    }
}
