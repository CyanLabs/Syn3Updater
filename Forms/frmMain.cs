using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoUpdaterDotNET;
using Microsoft.WindowsAPICodePack.Taskbar;
using Newtonsoft.Json;
using Syn3Updater.Helpers;
using Syn3Updater.Localization;
using Syn3Updater.Properties;
using SearchOption = System.IO.SearchOption;
//https://docs.microsoft.com/en-us/bingmaps/rest-services/common-parameters-and-types/supported-culture-codes

namespace Syn3Updater.Forms
{
    public partial class FrmMain : Form
    {
        public string Language = Settings.Default.Language;
        public FrmMain()
        {
            if (!string.IsNullOrEmpty(Language)) Thread.CurrentThread.CurrentUICulture = new CultureInfo(Language); //Launch the Form with the default/selected language
            InitializeComponent();
        }

        #region Form Variable Declarations
        private const string ApiBase = "https://api.cyanlabs.net/fordsyncdownloader/";
        private string _apiAppReleases = ApiBase + "items/releases?sort=-name&limit=-1&filter[status]=[published]";
        private string _apiMapReleases = ApiBase + "items/map_releases?sort=-name&limit=-1&filter[status]=[published]&filter[regions]=";
        private const string ApiAppReleaseSingle = ApiBase + "items/releases?sort=-name&limit=-1&fields=*.*.*&filter[name]=";
        private const string ApiMapReleaseSingle = ApiBase + "items/map_releases?sort=-name&limit=-1&fields=*.*.*&filter[name]=";

        private const string ReformatToolUrl = "https://cyanlabs.net/api/FordSyncDownloader/reformat.php";
        private const string SyncReformatTool = "1u5t-14g386-cb.tar.gz";
        private const string SyncReformatToolMd5 = "75E08C3EED8D2039BAF65B6156F79106";

        private const string DowngradePackageAppUrl = "https://ivsubinaries.azureedge.net/swparts/4U5T-14G381-AN_1552583626000.TAR.GZ";
        private const string DowngradePackageApp = "4U5T-14G381-AN_1552583626000.TAR.GZ";
        private const string DowngradePackageAppMd5 = "0553D1A474FBF9F0DB68A9C96FBDA7CB";

        private const string DowngradePackageToolUrl = "https://ivsubinaries.azureedge.net/swparts/GB5T-14G386-SC_85041.tar.gz";
        private const string DowngradePackageTool = "GB5T-14G386-SC_85041.tar.gz";
        private const string DowngradePackageToolMd5 = "E16F5E01D816E738E2B68592BDC22F3F";
        
        private const int SyncBlacklistedVersion = 3419274;
        private const int SyncReformatVersion = 3200000;
        
        private static readonly HttpClient Client = new HttpClient();
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private HttpClientDownloadWithProgress _httpclient;

        private readonly Queue<string> _downloadfiles = new Queue<string>();
        private readonly Dictionary<string, string> _dicDriveList = new Dictionary<string, string>();
        private readonly Dictionary<string,KeyValuePair<string,string>> _dicIvsus = new Dictionary<string, KeyValuePair<string, string>>();
        private KeyValuePair<string, string> _kvpDriveList;
        private int _prevprogressPercentageInt, _totalcount = -1, _pbCancel;
        private bool _downloadcomplete, _downloadonly, _cancelcopy, _canceldownload, _skipcheck, _appsselected;
        private Stopwatch _stopWatch;
        private ArrayList _ivsus;
        private string _partitionType, _driveId, _fileSystem, _volumeName, _stringReleasesJson, _stringMapReleasesJson, _stringDownloadJson, 
            _stringMapDownloadJson, _downloadpath, _stringCompatibility, _mode = "", _fileName = "", _published = "published", _version = "";
        private JsonReleases _jsonMapReleases, _jsonReleases;
        public static string Logoutput;
        #endregion

        #region Form Events
        private void FrmMain_Shown(object sender, EventArgs e)
        {
            FrmMain_Shown_Extra();
        }

        private void FrmMain_Shown_Extra()
        {
            lblVersion.Text =
                $@"This software nor CyanLabs is licensed or endorsed by Ford/FoMoCo - Version: {Assembly.GetExecutingAssembly().GetName().Version}";

            _version = Settings.Default.CurrentSyncVersion.ToString();
            _version = $"{_version[0]}.{_version[1]}.{_version.Substring(2, _version.Length - 2)}";

            lblConfiguration.Text = string.Format(strings.FrmMain_CurrentConfiguration, _version, Settings.Default.CurrentSyncRegion, (Settings.Default.CurrentSyncNav ? "Yes" : "No"), Settings.Default.DownloadPath);

            this.Size = new Size(620, 320);

            if (!Directory.Exists(Settings.Default.DownloadPath) && Settings.Default.DownloadPath == "")
            {
                FrmSetup frmSetup = new FrmSetup { Visible = true };
                frmSetup.FormClosing += SettingsFormClosing;
                this.Hide();
            }
            else if (!Directory.Exists(Settings.Default.DownloadPath) && Settings.Default.DownloadPath != "")
            {
                Directory.CreateDirectory(Settings.Default.DownloadPath);
            }

            _downloadpath = Settings.Default.DownloadPath;

            foreach (string arg in Environment.GetCommandLineArgs())
                switch (arg)
                {
                    case "/updated":
                        Settings.Default.Upgrade();
                        Settings.Default.Save();
                        break;
                    case "/all":
                        _published = "*";
                        break;
                    case "/debug":
                        _skipcheck = true;
                        break;
                }

            _apiMapReleases = _apiMapReleases.Replace("[published]", _published);
            _apiAppReleases = _apiAppReleases.Replace("[published]", _published);

            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                AutoUpdater.Start("https://cyanlabs.net/api/latest.php?product=" + Application.ProductName);

                HttpResponseMessage response = Client.GetAsync(_apiAppReleases).Result;
                _stringReleasesJson = response.Content.ReadAsStringAsync().Result;

                cmbDriveList.DisplayMember = "Value";
                cmbDriveList.ValueMember = "Key";
                RefreshUsb();
            }
            catch (WebException webex)
            {
                MessageBox.Show(strings.FrmMain_FrmMain_Shown_Extra_BackendError + Environment.NewLine + webex.Message, strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancelcopy = true;
            _tokenSource.Cancel();
            Settings.Default.Save();
        }
        #endregion

        private void cmbDriveList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbDriveList.SelectedIndex >= 1)
            {
                _kvpDriveList = (KeyValuePair<string, string>) cmbDriveList.SelectedItem;
                UpdateDriveInfo();
            }
            else
            {
                lblDriveInfo.Text = "";
            }
        }

        private void UpdateDriveInfo()
        {
            string partitionQueryText = $@"associators of {{{_kvpDriveList.Key}}} where AssocClass = Win32_DiskDriveToDiskPartition";
            ManagementObjectSearcher partitionQuery = new ManagementObjectSearcher(partitionQueryText);
            try
            {
                foreach (ManagementBaseObject o in partitionQuery.Get())
                {
                    ManagementObject p = (ManagementObject)o;
                    string logicalDriveQueryText = $@"associators of {{{p.Path.RelativePath}}} where AssocClass = Win32_LogicalDiskToPartition";
                    ManagementObjectSearcher logicalDriveQuery = new ManagementObjectSearcher(logicalDriveQueryText);
                    foreach (ManagementBaseObject managementBaseObject in logicalDriveQuery.Get())
                    {
                        ManagementObject ld = (ManagementObject)managementBaseObject;
                        _driveId = Convert.ToString(ld.Properties["DeviceId"].Value);
                        _partitionType = p.Properties["Type"].Value.ToString().Contains("GPT:") ? "GPT" : "MBR";

                        _fileSystem = Convert.ToString(ld.Properties["FileSystem"].Value);
                        _volumeName = Convert.ToString(ld.Properties["VolumeName"].Value);
                    }

                    Invoke(new Action(() => lblDriveInfo.Text = string.Format(strings.FrmMain_DriveInfo, _volumeName, _driveId, _fileSystem)));
                }
            }
            catch (ManagementException e)
            {
                MessageBox.Show(string.Format(strings.FrmMain_Exception, e.Message));
            }
        }

        private void btnRefreshUSB_Click(object sender, EventArgs e)
        {
            RefreshUsb();
        }

        private void RefreshUsb()
        {
            _dicDriveList.Clear();
            _dicDriveList.Add("", "");
            ManagementObjectSearcher driveQuery =
                new ManagementObjectSearcher("select * from Win32_DiskDrive WHERE InterfaceType='USB'");
            foreach (ManagementBaseObject o in driveQuery.Get())
            {
                ManagementObject d = (ManagementObject) o;
                string diskName = Convert.ToString(d.Properties["Caption"].Value);
                string friendlySize = Functions.BytesToString(Convert.ToInt64(d.Properties["Size"].Value));
                _dicDriveList.Add(d.Path.RelativePath, $"{diskName} (Size: {friendlySize})");
            }
            cmbDriveList.DataSource = new BindingSource(_dicDriveList, null);
        }

        private void cmbRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbRelease.Items.Clear();
            cmbMapVersion.Items.Clear();
            if (!Settings.Default.CurrentSyncNav)
            {
                cmbMapVersion.Items.Add(@"Non Nav APIM");
                cmbMapVersion.SelectedItem = @"Non Nav APIM";
            }
            else
            {
                cmbMapVersion.Items.Add(@"Keep Existing Maps");
                cmbMapVersion.SelectedItem = @"Keep Existing Maps";
            }

            _jsonReleases = JsonConvert.DeserializeObject<JsonReleases>(_stringReleasesJson);
            foreach (Data item in _jsonReleases.data)
                if (item.regions.Contains(cmbRegion.Text))
                    cmbRelease.Items.Add(item.name);
            if (cmbRegion.SelectedIndex >= 0) cmbRelease.Enabled = true;
        }

        private bool CancelledDownload()
        {
            //Check region is the same.
            if (cmbRegion.Text != Settings.Default.CurrentSyncRegion && _canceldownload == false)
            {
                DialogResult dialogOpenwebsite = MessageBox.Show(strings.FrmMain_CancelledDownload_RegionMismatch, strings.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogOpenwebsite != DialogResult.Yes) _canceldownload = true;
            }

            //No USB drive selected, download only?
            if (cmbDriveList.SelectedIndex == 0 && _canceldownload == false)
            {
                DialogResult dialogOpenwebsite = MessageBox.Show(strings.FrmMain_CancelledDownload_NoUSB, strings.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogOpenwebsite == DialogResult.Yes)
                    _downloadonly = true;
                else
                    _canceldownload = true;
            }

            if (!string.IsNullOrEmpty(_driveId))
            {
                if (_downloadpath.Contains(_driveId) && _canceldownload == false)
                    MessageBox.Show(strings.FrmMain_CancelledDownload_DownloadPathInvalid, strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            if (_appsselected == false && _canceldownload == false && (_mode == "reformat" || _mode == "downgrade"))
                MessageBox.Show(strings.FrmMain_CancelledDownload_NoApps, strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            // ReSharper disable once InvertIf
            if (_downloadonly == false && _canceldownload == false)
            {
                string selectedDisk = ((KeyValuePair<string, string>) cmbDriveList.SelectedItem).Value;
                DialogResult dialogResult = MessageBox.Show(string.Format(strings.FrmMain_CancelledDownload_FormatUSB, selectedDisk, _driveId), strings.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.No) _canceldownload = true;
            }
            return _canceldownload;
        }

        private void ResetControls()
        {
            _downloadonly = false;
            _canceldownload = false;
            _prevprogressPercentageInt = 0;
            _totalcount = -1;
            Logoutput = "";
            _tokenSource.Dispose();
            _tokenSource = new CancellationTokenSource();
            Invoke((MethodInvoker)delegate
            {
                this.Size = new Size(620, 320);
                barTotalDownloadProgress.Maximum = 0;
                btnContinue.Enabled = true;
                grpNewVersion.Enabled = true;
                grpUSB.Enabled = true;
                _canceldownload = false;
                _dicIvsus.Clear();
            });
        }

        private void DisableControls()
        {
            _prevprogressPercentageInt = 0;
            _totalcount = -1;
            Invoke((MethodInvoker)delegate
            {
                barTotalDownloadProgress.Maximum = 0;
                _dicIvsus.Clear();
                _tokenSource.Dispose();
                _tokenSource = new CancellationTokenSource();
                tabControl1.SelectedTab = tabStatus;
            });
            btnContinue.Enabled = false;
            grpNewVersion.Enabled = false;
            grpUSB.Enabled = false;
        }

        private void btnContinue_Click(object sender, EventArgs e)
        {
            ResetControls();
            this.Size = new Size(620, 550);

            //LESS THAN 3.2
            if (Settings.Default.CurrentSyncVersion < SyncReformatVersion)
            {
                _mode = @"reformat";
            }

            //Above 3.2 and  Below 3.4.19274
            else if (Settings.Default.CurrentSyncVersion >= SyncReformatVersion && Settings.Default.CurrentSyncVersion < SyncBlacklistedVersion)
            {
                //Update Nav?
                if (cmbMapVersion.Text == @"No Maps" || cmbMapVersion.Text == @"Non Nav APIM" || cmbMapVersion.Text == @"Keep Existing Maps")
                {
                    _mode = @"autoinstall";
                }
                else
                {
                    _mode = Settings.Default.ForceAutoinstall ? @"autoinstall" : @"reformat";
                }
            }

            //3.4.19274 or above
            else if (Settings.Default.CurrentSyncVersion >= SyncBlacklistedVersion)
            {
                //Update Nav?
                if (cmbMapVersion.Text == @"No Maps" || cmbMapVersion.Text == @"Non Nav APIM" || cmbMapVersion.Text == @"Keep Existing Maps")
                {
                    _mode = @"autoinstall";
                }
                else
                {
                    _mode = Settings.Default.ForceAutoinstall ? @"autoinstall" : @"downgrade";
                }
            }

            string release = cmbRelease.Text;
            if (_mode == @"downgrade")
            {
                lblMode1.Text = @"Downgrade Mode";
                lblMode2.ForeColor = Color.Cyan;
                lblMode1.ForeColor = Color.Cyan;
            }
            else if (_mode == @"autoinstall")
            {
                if (Settings.Default.ForceAutoinstall)
                {
                    lblMode1.Text = @"Autoinstall Mode (FORCED)";
                    lblMode2.ForeColor = Color.Red;
                    lblMode1.ForeColor = Color.Red;
                }
                else
                {
                    lblMode1.Text = @"Autoinstall Mode";
                    lblMode2.ForeColor = Color.LimeGreen;
                    lblMode1.ForeColor = Color.LimeGreen;
                }
            }
            else
            {
                lblMode1.Text = @"Reformat Mode";
                lblMode2.ForeColor = Color.Orange;
                lblMode1.ForeColor = Color.Orange;
            }

            tabControl1.SelectedTab = tabAutoInstall;
            lstIVSU.Items.Clear();

            if (_mode == @"downgrade")
            {
                string app = _downloadpath + DowngradePackageApp;
                if (!File.Exists(app) || CalculateMd5(app) != DowngradePackageAppMd5)
                {
                    _downloadfiles.Enqueue(new Uri(DowngradePackageAppUrl).ToString());
                    lstDownloadQueue.Items.Add(new Uri(DowngradePackageAppUrl).ToString());
                }
                string tool = _downloadpath + DowngradePackageTool;
                if (!File.Exists(tool) || CalculateMd5(tool) != DowngradePackageToolMd5)
                {
                    _downloadfiles.Enqueue(new Uri(DowngradePackageToolUrl).ToString());
                    lstDownloadQueue.Items.Add(new Uri(DowngradePackageToolUrl).ToString());
                }
            }

            if (_mode == @"reformat" || _mode == @"downgrade")
            {
                string reformattool = _downloadpath + SyncReformatTool;
                if (!File.Exists(reformattool) || CalculateMd5(reformattool) != SyncReformatToolMd5)
                {
                    _downloadfiles.Enqueue(new Uri(ReformatToolUrl).ToString());
                    lstDownloadQueue.Items.Add(new Uri(ReformatToolUrl).ToString());
                }
            }

            HttpResponseMessage response = Client.GetAsync(ApiAppReleaseSingle + release).Result;
            _stringDownloadJson = response.Content.ReadAsStringAsync().Result;

            response = Client.GetAsync(ApiMapReleaseSingle + cmbMapVersion.Text).Result;
            _stringMapDownloadJson = response.Content.ReadAsStringAsync().Result;

            JsonReleases jsonIvsUs = JsonConvert.DeserializeObject<JsonReleases>(_stringDownloadJson);
            JsonReleases jsonMapIvsUs = JsonConvert.DeserializeObject<JsonReleases>(_stringMapDownloadJson);

            foreach (Ivsus item in jsonIvsUs.data[0].ivsus)
                if (item.ivsu.regions.Contains(@"ALL") || item.ivsu.regions.Contains(cmbRegion.Text))
                    lstIVSU.Items.Add(new ListViewItem(new[]
                        {item.ivsu.type, item.ivsu.name, item.ivsu.version, item.ivsu.url, item.ivsu.md5}));
            
            if (cmbMapVersion.Text != @"No Maps" && cmbMapVersion.Text != @"Non Nav APIM" && cmbMapVersion.Text != @"Keep Existing Maps")
                foreach (Ivsus item in jsonMapIvsUs.data[0].ivsus)
                    if (item.map_ivsu.regions.Contains(@"ALL") || item.map_ivsu.regions.Contains(cmbRegion.Text))
                        lstIVSU.Items.Add(new ListViewItem(new[]{
                            item.map_ivsu.type, item.map_ivsu.name, cmbMapVersion.Text, item.map_ivsu.url, item.map_ivsu.md5
                        }));
            foreach (ListViewItem item in lstIVSU.Items)
                item.Checked = true;
        }


        private void btnStart_Click(object sender, EventArgs e)
        {
            DisableControls();

            foreach (ListViewItem item in lstIVSU.CheckedItems)
            {
                if (item.SubItems[0].Text == @"APPS")
                    _appsselected = true;
                lstDownloadQueue.Items.Add(item.SubItems[3].Text);
            }

            if (CancelledDownload())
                ResetControls();
            else
            {
                _ivsus = new ArrayList(lstIVSU.CheckedItems);
                UpdateLog(string.Format(strings.LogStart, cmbRelease.Text, cmbRegion.Text, cmbMapVersion.Text));

                Task t = new Task(ValidateDownloadedFiles);
                t.Start();
            }
        }

        private void btnShowConfiguration_Click(object sender, EventArgs e)
        {
            FrmSetup frmSetup = new FrmSetup {Visible = true};
            frmSetup.FormClosing += SettingsFormClosing;
            this.Hide();
        }

        private void SettingsFormClosing(object sender, FormClosingEventArgs e)
        {
            if (Settings.Default.TOCAccepted2)
            {
                this.Show();
                FrmMain_Shown_Extra();
            }
            else
            {
                FrmDisclaimer frmdisclaimer = new FrmDisclaimer {Visible = true};
                frmdisclaimer.FormClosing += DisclaimerFormClosing;
            }
        }

        private void DisclaimerFormClosing(object sender, FormClosingEventArgs e)
        {
            this.Show();
            FrmMain_Shown_Extra();
        }

        private void ValidateDownloadedFiles()
        {
            foreach (ListViewItem item in _ivsus)
            {
                _fileName = item.SubItems[3].Text.Substring(item.SubItems[3].Text.LastIndexOf("/", StringComparison.Ordinal) + 1,
                    item.SubItems[3].Text.Length - item.SubItems[3].Text.LastIndexOf("/", StringComparison.Ordinal) - 1);

                if (ValidateFile(item.SubItems[3].Text, _downloadpath + _fileName, item.SubItems[4].Text, false))
                    Invoke(new Action(() => lstDownloadQueue.Items.Remove(item.SubItems[3].Text)));
                else
                    _downloadfiles.Enqueue(item.SubItems[3].Text);

                if (!_dicIvsus.ContainsKey(_fileName)) _dicIvsus.Add(_fileName, new KeyValuePair<string, string>(item.SubItems[0].Text, item.SubItems[4].Text));
            }

            if (_totalcount == -1)
            {
                _totalcount = _downloadfiles.Count;
                Invoke(new Action(() => barTotalDownloadProgress.Maximum = 100 * _totalcount));
            }
            Invoke(new Action(() => btnCancel.Visible = true));
            DownloadFile();
        }

        private async void DownloadFile()
        {
            while (true)
            {
                if (_downloadfiles.Any())
                {
                    Invoke(new Action(() =>
                    {
                        int filecount = _downloadfiles.Count + 1;
                        lblFilesRemaining.Text = filecount == 1 ? "1 file to download" : filecount + " files to download";
                    }));
                    string downloadFileUrl = _downloadfiles.Dequeue();

                    _fileName = downloadFileUrl.Contains(@"reformat.php")
                        ? SyncReformatTool
                        : downloadFileUrl.Substring(downloadFileUrl.LastIndexOf("/", StringComparison.Ordinal) + 1,
                            downloadFileUrl.Length - downloadFileUrl.LastIndexOf("/", StringComparison.Ordinal) - 1);
                    UpdateLog(string.Format(strings.LogDownloading, _fileName));
                    string destinationFilePath = _downloadpath + _fileName;
                    _stopWatch = new Stopwatch();
                    _stopWatch.Start();
                    using (_httpclient = new HttpClientDownloadWithProgress(downloadFileUrl, destinationFilePath))
                    {
                        _httpclient.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                        {
                            long totalBytesDownloadedInt64 = Convert.ToInt64(totalBytesDownloaded);
                            long totalFileSizeInt64 = Convert.ToInt64(totalFileSize);
                            int progressPercentageInt = Convert.ToInt32(progressPercentage);
                            int progressTotalPercentage = progressPercentageInt + _prevprogressPercentageInt;
                            long bs;
                            if (_stopWatch.Elapsed.TotalSeconds >= 1)
                            {
                                bs = totalBytesDownloadedInt64 / Convert.ToInt64(_stopWatch.Elapsed.TotalSeconds);
                                long bytesRemaining = totalFileSizeInt64 - totalBytesDownloadedInt64;
                                // ReSharper disable once PossibleLossOfFraction
                                double timeRemainingInSeconds = bytesRemaining / bs;
                                TimeSpan remainingTime = TimeSpan.FromSeconds(timeRemainingInSeconds);

                                Invoke(new Action(() =>
                                {
                                    lblFileName.Text = string.Format(strings.Downloading, _fileName, Functions.BytesToString(totalBytesDownloadedInt64), Functions.BytesToString(totalFileSizeInt64));
                                    lblDownloadSize.Text = $@"{remainingTime.Minutes:00}m {remainingTime.Seconds:00}s @ {Functions.BytesToString(bs)}/S";
                                    barTotalDownloadProgress.Value = progressTotalPercentage;
                                    barDownloadProgress.Value = progressPercentageInt;
                                    TaskbarManager.Instance.SetProgressValue(progressTotalPercentage, 100 * _totalcount);
                                }));
                            }
                        };
                        await _httpclient.StartDownload(_tokenSource.Token);
                    }

                    _prevprogressPercentageInt += 100;
                    Invoke(new Action(() => lstDownloadQueue.Items.Remove(downloadFileUrl)));
                    continue;
                }

                if (!_downloadfiles.Any() && _downloadcomplete == false && _mode == "reformat")
                {
                    _downloadcomplete = true;
                    ValidateDownloadedFiles();
                }
                else if (!_downloadfiles.Any() && _downloadcomplete == false && _mode != "reformat")
                {
                    _downloadcomplete = true;
                    continue;
                }
                else if (!_downloadfiles.Any() && _downloadcomplete)
                {
                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
                    _downloadcomplete = false;

                    Invoke(new Action(() =>
                    {
                        btnCancel.Visible = false;
                        lblDownloadSize.Text = "";
                        lblFilesRemaining.Text = "";
                        grpNewVersion.Enabled = true;
                        grpUSB.Enabled = true;
                        TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
                    }));

                    if (_tokenSource.IsCancellationRequested)
                    {
                        UpdateLog(strings.LogDownloadsCancelled);
                    }
                    else
                    {
                        UpdateLog(strings.FrmMain_DownloadFile_AllDownloaded);

                        _prevprogressPercentageInt = 0;

                        if (_downloadonly)
                            MessageBox.Show(strings.FrmMain_DownloadFile_AllDownloaded, strings.Success, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        else
                            PrepareUsb();
                    }
                    ResetControls();
                }
                break;
            }
        }

        private void CopyFiles()
        {
            _cancelcopy = false;
            _stopWatch = new Stopwatch();
            _totalcount = _dicIvsus.Count;
            if (_totalcount == -1) Invoke(new Action(() => barTotalDownloadProgress.Maximum = 100 * _totalcount));
            try
            {
                foreach (KeyValuePair<string, KeyValuePair<string,string>> item in _dicIvsus)
                {
                    Invoke(new Action(() => lblFilesRemaining.Text = lstDownloadQueue.Items.Count == 1 ? "1 file left to copy" : lstDownloadQueue.Items.Count + " files left to copy"));
                    _fileName = item.Key;
                    _stopWatch.Restart();

                    for (int i = 0; i < 3; i++)
                    {
                        UpdateLog(string.Format(strings.LogCopying,_fileName));
                        bool copycomplete = Functions.CopyFileEx(_downloadpath + _fileName, _driveId + @"\SyncMyRide\" + _fileName, CopyProgressHandler,
                            IntPtr.Zero, ref _pbCancel, Functions.CopyFileFlags.CopyFileRestartable);
                        bool validfile = ValidateFile(_downloadpath + _fileName, _driveId + @"\SyncMyRide\" + _fileName, item.Value.Value, true); 
                        
                        if (copycomplete && validfile) break;
                        UpdateLog(string.Format(strings.LogFailed3, _fileName, i));
                        if (i == 2) MessageBox.Show(strings.FrmMain_CopyFiles_VerificationFailed, strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }

                    _prevprogressPercentageInt += 100;
                    Invoke(new Action(() => lstDownloadQueue.Items.Remove(_downloadpath + item.Key)));
                }
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e);
            }

            Invoke((MethodInvoker) delegate
            {
                if (_cancelcopy) return;
                barTotalDownloadProgress.Value = barTotalDownloadProgress.Maximum;
                TaskbarManager.Instance.SetProgressValue(100 * _totalcount, 100 * _totalcount);
                lblFilesRemaining.Text = "";
                UpdateLog(strings.FrmMain_CopyFiles_Completed);
                lblDownloadSize.Text = "";
                lblFileName.Text = strings.FrmMain_CopyFiles_Completed;
            });
            GenerateLog();

            DialogResult result = MessageBox.Show(strings.FrmMain_CopyFiles_CompletePopup, strings.Success, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
                Process.Start("https://cyanlabs.net/tutorials/update-ford-sync-3-2-2-3-0-to-version-3-4-all-years-3-4-19200/#" + _mode);

            result = MessageBox.Show(strings.FrmMain_CopyFiles_UpdateVersion, strings.Success, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                Invoke(new Action(() => Settings.Default.CurrentSyncVersion = Convert.ToInt32(cmbRelease.Text.Replace(".", "").Replace("Sync ", ""))));
                Settings.Default.Save();
            }
            Invoke(new Action(FrmMain_Shown_Extra));
        }

        #region Helper Functions

        private bool ValidateFile(string srcfile, string localfile, string md5, bool copy)
        {
            if (_skipcheck) return true;
            Invoke(new Action(() => lblFileName.Text = string.Format(strings.FrmMain_ValidateFile_Verifying, _fileName)));
            if (!File.Exists(localfile)) return false;
            UpdateLog(string.Format(strings.Verifying,_fileName),"INFO",true);

            string localMd5 = CalculateMd5(localfile);

            if (md5 == null)
            {
                UpdateLog(string.Format(strings.LogNoMD5, _fileName), "WARN",true);

                long filesize = new FileInfo(localfile).Length;
                if (copy)
                {
                    long srcfilesize = new FileInfo(srcfile).Length;

                    if (srcfilesize == filesize)
                    {
                        if (localMd5 == CalculateMd5(srcfile))
                        {
                            UpdateLog(string.Format(strings.Verified, _fileName));
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
                            UpdateLog(string.Format(strings.Verified,_fileName));
                            return true;
                        }
                    }
                }
            } 
            else if (string.Equals(localMd5, md5, StringComparison.CurrentCultureIgnoreCase))
            {
                UpdateLog(string.Format(strings.Verified, _fileName));
                return true;
            }

            Invoke(new Action(() => barTotalDownloadProgress.Maximum += 100));
            UpdateLog(string.Format(strings.LogFailedRetry, _fileName),"WARN");
            return false;
        }

        private void cmbMapVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            lstIVSU.Items.Clear();
            this.Size = new Size(620, 320);
            btnContinue.Enabled = (cmbMapVersion.Text != "" && cmbRelease.Text != "");
        }

        private void cmbRelease_SelectedIndexChanged(object sender, EventArgs e)
        {
            lstIVSU.Items.Clear();
            this.Size = new Size(620, 320);
            foreach (Data item in _jsonReleases.data)
                if (item.name == cmbRelease.Text)
                {
                    _stringCompatibility = item.version.Substring(0, 3);
                    if (item.notes != null)
                        txtReleaseNotes.Text = string.Format(strings.FrmMain_Notes, item.notes.Replace("\n", Environment.NewLine));
                }

            HttpResponseMessage response = Client.GetAsync(_apiMapReleases + cmbRegion.Text + "&filter[compatibility][contains]=" + _stringCompatibility).Result;
            _stringMapReleasesJson = response.Content.ReadAsStringAsync().Result;

            if (Settings.Default.CurrentSyncNav)
            {
                cmbMapVersion.Items.Clear();
                cmbMapVersion.Items.Add("No Maps");
                if (!Settings.Default.CurrentSyncNav)
                {
                    cmbMapVersion.Items.Add("Non Nav APIM");
                    cmbMapVersion.SelectedItem = "Non Nav APIM";
                }
                else
                {
                    cmbMapVersion.Items.Add("Keep Existing Maps");
                    cmbMapVersion.SelectedItem = "Keep Existing Maps";
                }

                _jsonMapReleases = JsonConvert.DeserializeObject<JsonReleases>(_stringMapReleasesJson);
                foreach (Data item in _jsonMapReleases.data) cmbMapVersion.Items.Add(item.name);
                if (cmbRelease.SelectedIndex >= 0 && Settings.Default.CurrentSyncNav) cmbMapVersion.Enabled = true;
            }
            btnContinue.Enabled = cmbMapVersion.Text != "" && cmbRelease.Text != "";
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            Settings.Default.ForceAutoinstall = false;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _tokenSource.Cancel();
            ResetControls();
        }

        private void cmbLocale_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChangeLanguage(cmbLocale.Text);
            Settings.Default.Save();
            FrmMain_Shown_Extra();

            DialogResult dialog = MessageBox.Show(strings.FrmMain_cmbLocale_Restart, strings.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialog == DialogResult.Yes)
                Application.Restart();
        }

        private void lblMode1_TextChanged(object sender, EventArgs e)
        {
            lblMode2.Text = lblMode1.Text;
        }

        private void UpdateLog(string text, string type = "INFO", bool hide = false)
        {
            Logoutput += $@"{type}: {text}" + Environment.NewLine;
            if (!hide)
            {
                if (lblCurrentTask.InvokeRequired)
                    Invoke(new Action(() => lblCurrentTask.Text = text));
                else
                    lblCurrentTask.Text = text;
            }
            Console.WriteLine($@"{type}: {text}");
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
                    Invoke(
                        new Action(() => barDownloadProgress.Value = ((int) ((double) read / size * 100))));
                } while (bytesRead != 0);

                hasher.TransformFinalBlock(buffer, 0, 0);
                return BitConverter.ToString(hasher.Hash).Replace("-", String.Empty);
            }
        }
        private void ChangeLanguage(string lang) //A function called to change the language
        {
            foreach (Control c in this.Controls)
            {
                ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmMain));
                resources.ApplyResources(c, c.Name, new CultureInfo(lang));
            }
        }

        private void GenerateLog()
            {
                Invoke((MethodInvoker) delegate
                {
                    string data = $@"CYANLABS - SYN3 UPDATER - V{Assembly.GetExecutingAssembly().GetName().Version}" + Environment.NewLine;
                    data += Environment.NewLine;
                    data += @"PREVIOUS CONFIGURATION" + Environment.NewLine;
                    data += @"Version: " + _version + Environment.NewLine;
                    data += @"Region: " + Settings.Default.CurrentSyncRegion + Environment.NewLine;
                    data += @"Navigation: " + Settings.Default.CurrentSyncNav + Environment.NewLine;
                    data += @"Mode: " + _mode + Environment.NewLine + (Settings.Default.ForceAutoinstall ? "Forced" : "") + Environment.NewLine;
                    data += Environment.NewLine;
                    data += @"USB DETAILS" + Environment.NewLine;
                    data += @"Model: " + cmbDriveList.Text + Environment.NewLine;
                    data += @"FileSystem: " + _fileSystem + Environment.NewLine;
                    data += @"Partition Type: " + _partitionType + Environment.NewLine;

                    if (File.Exists(_driveId + @"\reformat.lst"))
                    {
                        data += Environment.NewLine;
                        data += @"REFORMAT.LST" + Environment.NewLine;
                        data += File.ReadAllText(_driveId + @"\reformat.lst") + Environment.NewLine;
                    }

                    if (File.Exists(_driveId + @"\autoinstall.lst"))
                    {
                        data += Environment.NewLine;
                        data += @"AUTOINSTALL.LST" + Environment.NewLine;
                        data += File.ReadAllText(_driveId + @"\autoinstall.lst");
                    }

                    if (Directory.Exists(_driveId + @"\SyncMyRide"))
                    {
                        data += Environment.NewLine;
                        DirectoryInfo di = new DirectoryInfo(_driveId + @"\SyncMyRide");
                        FileInfo[] allFiles = di.GetFiles("*", SearchOption.AllDirectories);
                        data += @"SYNCMYRIDE FILES (" + allFiles.Length + ")" + Environment.NewLine;
                        foreach (FileInfo file in allFiles)
                            data += $"{file.Name} ({Functions.BytesToString(file.Length)})" + Environment.NewLine;
                    }

                    data += Environment.NewLine;
                    data += @"LOG" + Environment.NewLine;
                    data += Logoutput;
                    File.WriteAllText(_driveId + @"\log.txt", data);
                });
            }
            #endregion

            #region Ford lst Generation

            private void CreateReformat()
            {
                string cmbReleaseText = "", cmbRegionText = "";
                Invoke((MethodInvoker)delegate
                {
                    cmbReleaseText = cmbRelease.Text;
                    cmbRegionText = cmbRegion.Text;
                });
                string reformatlst = "";
                int i = 0;
                foreach (KeyValuePair<string, KeyValuePair<string, string>> item in _dicIvsus)
                {
                    if (item.Key == SyncReformatTool || (item.Key == DowngradePackageApp && cmbReleaseText != @"Sync 3.3.19052") || item.Key == DowngradePackageTool) continue;
                    i++;
                    reformatlst += item.Value.Key + @"=" + item.Key;
                    if (i != _dicIvsus.Count) reformatlst += Environment.NewLine;
                }

            UpdateLog(string.Format(strings.LogCreateReformat, _mode));

            File.WriteAllText(_driveId + @"\reformat.lst", reformatlst);
                string autoinstalllst = @"; CyanLabs Syn3Updater - " + _mode + @" Mode - " + cmbReleaseText + " " + cmbRegionText + Environment.NewLine + Environment.NewLine + @"[SYNCGen3.0_ALL_PRODUCT]" + Environment.NewLine;
                if (_mode == @"downgrade")
                {
                    autoinstalllst += string.Format(@"Item1 = TOOL - {0}\rOpen1 = SyncMyRide\{0}\r", DowngradePackageTool).Replace(@"\r", Environment.NewLine);
                    autoinstalllst += string.Format(@"Item2 = APP - {0}\rOpen2 = SyncMyRide\{0}\r", DowngradePackageApp).Replace(@"\r", Environment.NewLine);
                    autoinstalllst += @"Options = AutoInstall" + Environment.NewLine + @"[SYNCGen3.0_ALL]" + Environment.NewLine;
                    autoinstalllst += string.Format(@"Item1 = REFORMAT TOOL - {0}\rOpen1 = SyncMyRide\{0}\r", SyncReformatTool).Replace(@"\r", Environment.NewLine);
                    autoinstalllst += @"Options = AutoInstall,Include,Transaction" + Environment.NewLine;
                }
                else if (_mode == @"reformat")
                {
                    autoinstalllst += string.Format(@"Item1 = REFORMAT TOOL  - {0}\rOpen1 = SyncMyRide\{0}\r", SyncReformatTool).Replace(@"\r", Environment.NewLine);
                    autoinstalllst += @"Options = AutoInstall";
                }
                UpdateLog(string.Format(strings.LogCreateAutoinstall, _mode));

                File.WriteAllText(_driveId + @"\autoinstall.lst", autoinstalllst);
                File.Create(_driveId + @"\DONTINDX.MSA");
            }

            private void CreateAutoInstall()
            {
                string cmbReleaseText = "", cmbRegionText = "";
                Invoke((MethodInvoker)delegate
                {
                    cmbReleaseText = cmbRelease.Text;
                    cmbRegionText = cmbRegion.Text;
                });
                string autoinstalllst = @"; CyanLabs Syn3Updater - Autoinstall Mode - " +
                                        cmbReleaseText + " " + cmbRegionText +
                                        Environment.NewLine + Environment.NewLine + @"[SYNCGen3.0_ALL_PRODUCT]" +
                                        Environment.NewLine;

                string extrafiles = "";
                int baseint = 0, extraint = 0;
                foreach (KeyValuePair<string, KeyValuePair<string, string>> item in _dicIvsus)
                {
                    if (item.Value.Key == @"APPS" || item.Value.Key == @"VOICE" || item.Value.Key == @"GRACENOTES" ||
                        item.Value.Key == @"ENH_DAB" || item.Value.Key == @"MAP_LICENSE" || item.Value.Key == @"VOICE_NAV")
                    {
                        baseint++;
                        autoinstalllst += string.Format(@"Item{0} = {1} - {2}\rOpen{0} = SyncMyRide\{2}\r", baseint,
                                item.Value.Key, item.Key)
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
                                item.Value.Key, item.Key)
                            .Replace(@"\r", Environment.NewLine);
                        
                    }
                }

                if (extrafiles != "")
                {
                     extrafiles += @"Options = Delay,Include,Transaction";

                }
                autoinstalllst += @"Options = AutoInstall" + Environment.NewLine;
                autoinstalllst += extrafiles;
                UpdateLog(string.Format(strings.LogCreateAutoinstall, _mode));
                File.WriteAllText(_driveId + @"\autoinstall.lst", autoinstalllst);
                File.Create(_driveId + @"\DONTINDX.MSA");
            }

            #endregion

            private void PrepareUsb()
            {
                string drivenumber = _kvpDriveList.Key.Replace("Win32_DiskDrive.DeviceID=\"\\\\\\\\.\\\\PHYSICALDRIVE", "").Replace("\"", "");

                using (Process p = new Process())
                {
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.FileName = @"diskpart.exe";
                    p.StartInfo.CreateNoWindow = true;

                    UpdateLog(strings.LogFormat);

                    p.Start();
                    p.StandardInput.WriteLine("SELECT DISK=" + drivenumber);
                    p.StandardInput.WriteLine("CLEAN");
                    p.StandardInput.WriteLine("CONVERT MBR");
                    p.StandardInput.WriteLine("CREATE PARTITION PRIMARY");
                    p.StandardInput.WriteLine("FORMAT FS=EXFAT LABEL=\"CYANLABS\" QUICK");
                    p.StandardInput.WriteLine("ASSIGN");
                    p.StandardInput.WriteLine("EXIT");

                    p.WaitForExit();
                }

                UpdateDriveInfo();
                switch (_mode)
                {
                    case @"autoinstall":
                        CreateAutoInstall();
                        break;
                    case @"downgrade":
                        _dicIvsus.Add(DowngradePackageApp, new KeyValuePair<string, string>(@"APP", DowngradePackageAppMd5));
                        _dicIvsus.Add(DowngradePackageTool, new KeyValuePair<string, string>(@"TOOL", DowngradePackageToolMd5));
                        _dicIvsus.Add(SyncReformatTool, new KeyValuePair<string, string>(@"TOOL", SyncReformatToolMd5)); 
                        CreateReformat();
                        break;
                    case @"reformat":
                        _dicIvsus.Add(SyncReformatTool, new KeyValuePair<string, string>(@"TOOL",SyncReformatToolMd5));
                        CreateReformat();
                        break;
                }

                foreach (KeyValuePair<string, KeyValuePair<string, string>> item in _dicIvsus)
                    Invoke(new Action(() => lstDownloadQueue.Items.Add(_downloadpath + item.Key)));

                Directory.CreateDirectory(_driveId + @"\SyncMyRide\");

                CopyFiles();
            }

            private Functions.CopyProgressResult CopyProgressHandler(long total, long transferred, long streamSize,
                long streamByteTrans, uint dwStreamNumber, IntPtr hSourceFile, IntPtr hDestinationFile, IntPtr lpData)
            {
                try
                {
                    long totalBytesCopiedInt64 = Convert.ToInt64(transferred);
                    long totalFileSizeInt64 = Convert.ToInt64(total);
                    double progressPercentage = 100.0 * transferred / total;
                    int progressPercentageInt = Convert.ToInt32(progressPercentage);
                    int progressTotalPercentage = progressPercentageInt + _prevprogressPercentageInt;
                    long bs;
                    double timeRemainingInSeconds = 0;
                    if (_stopWatch.Elapsed.TotalSeconds >= 1)
                    {
                        bs = totalBytesCopiedInt64 / Convert.ToInt64(_stopWatch.Elapsed.TotalSeconds);
                        long bytesRemaining = totalFileSizeInt64 - totalBytesCopiedInt64;

                        if (bs != 0)
                            // ReSharper disable once PossibleLossOfFraction
                            timeRemainingInSeconds = bytesRemaining / bs;

                        TimeSpan remainingTime = TimeSpan.FromSeconds(timeRemainingInSeconds);

                        Invoke(new Action(() =>
                        {
                            lblFileName.Text = string.Format(strings.FrmMain_Copying, _fileName, Functions.BytesToString(totalBytesCopiedInt64), Functions.BytesToString(totalFileSizeInt64));
                            lblDownloadSize.Text = $@"{remainingTime.Minutes:00}m {remainingTime.Seconds:00}s @ {Functions.BytesToString(bs)}/S";
                            if (_totalcount > 0)
                            {
                                barTotalDownloadProgress.Maximum = 100 * _totalcount;
                                barTotalDownloadProgress.Value = progressTotalPercentage;
                                barDownloadProgress.Value = progressPercentageInt;
                                TaskbarManager.Instance.SetProgressValue(progressTotalPercentage, 100 * _totalcount);
                            }
                        }));
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                return _cancelcopy
                    ? Functions.CopyProgressResult.ProgressCancel
                    : Functions.CopyProgressResult.ProgressContinue;
            }

            private void btnLogo_Click(object sender, EventArgs e)
            {
                Process.Start("https://cyanlabs.net");
            }

            private void btnShowLog_Click(object sender, EventArgs e)
            {
                var frmLog = new FrmLog();
                frmLog.Show();
            }

            private void btnHelp_Click(object sender, EventArgs e)
            {
                Process.Start(@"https://cyanlabs.net/ford");
            }

            private void lblRegionInfo_Click(object sender, EventArgs e)
            {
                // ReSharper disable once AssignmentIsFullyDiscarded
                _ = new FrmRegion() {Visible = true};
            }

            private void panelTitleBar_MouseDown(object sender, MouseEventArgs e)
            {
                Functions.ReleaseCapture();
                Functions.SendMessage(Handle, 0x112, 0xf012, 0);
            }

            private void btnClose_MouseHover(object sender, EventArgs e)
            {
                btnClose.Image = Resources.close;
            }

            private void btnMinimize_MouseHover(object sender, EventArgs e)
            {
                btnMinimize.Image = Resources.minimize;
            }

            private void btnClose_Click(object sender, EventArgs e)
            {
                Environment.Exit(0);
            }

            private void btnWindowControls_MouseLeave(object sender, EventArgs e)
            {
                ((PictureBox) sender).Image = Resources.button;
            }

            private void btnMinimize_Click(object sender, EventArgs e)
            {
                WindowState = FormWindowState.Minimized;
            }
        }
    }