﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoUpdaterDotNET;
using Microsoft.WindowsAPICodePack.Taskbar;
using Newtonsoft.Json;
using Sync3Updater.Helpers;
using Sync3Updater.Localization;
using Sync3Updater.Properties;
using SearchOption = System.IO.SearchOption;

//https://docs.microsoft.com/en-us/bingmaps/rest-services/common-parameters-and-types/supported-culture-codes



namespace Sync3Updater
{
    public partial class FrmMain : Form
    {
        private static readonly HttpClient Client = new HttpClient();
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public FrmMain()
        {
            InitializeComponent();
        }

        #region Form Variable Declarations

        // URL's
        private const string UrlApiBase = "https://api.cyanlabs.net/fordsyncdownloader/";
        private const string UrlApiAppReleases = UrlApiBase + "items/releases?sort=-name&filter[status]=published";
        private const string UrlApiAppReleasesAll = UrlApiBase + "items/releases?sort=-name";
        private const string UrlApiAppReleaseSingle = UrlApiBase + "items/releases?sort=-name&fields=*.*.*&filter[name]=";
        private const string UrlApiMapReleases = UrlApiBase + "items/map_releases?sort=-name&filter[status]=published&filter[regions]=";
        private const string UrlApiMapReleasesAll = UrlApiBase + "items/map_releases?sort=-name&filter[regions]=";
        private const string UrlApiMapReleaseSingle = UrlApiBase + "items/map_releases?sort=-name&fields=*.*.*&filter[name]=";
        private const string UrlReformatTool = "https://www.dropbox.com/s/1dm3bdagyj6pn6l/1u5t-14g386-cb.tar.gz?dl=1";
        private const int SyncBlacklistedVersion = 3419274;
        private const int SyncReformatVersion = 3200000;
        private HttpClientDownloadWithProgress _httpclient;
        private readonly Queue<string> _downloadfiles = new Queue<string>();
        private readonly Dictionary<string, string> _dicDriveList = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _dicIvsUs = new Dictionary<string, string>();
        private KeyValuePair<string, string> _kvpDriveList;
        private int _prevprogressPercentageInt, _totalcount = -1;
        private bool _downloadcomplete, _boolShowAllReleases, _downloadonly, _cancelcopy;
        private string _mode = "";
        private Stopwatch _stopWatch;
        private string _fileName = "";

        private string _partitionType,
            _driveId,
            _fileSystem,
            _volumeName,
            _stringReleasesJson,
            _stringMapReleasesJson,
            _stringDownloadJson,
            _stringMapDownloadJson,
            _downloadpath,
            _stringCompatibility,
            _cmbSyncVersionText;

        List<string> _validatedIvsUsList = new List<string>();

        private void btnLogo_Click(object sender, EventArgs e)
        {
            Process.Start("https://cyanlabs.net");
        }

        private void cmbRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbRelease.Items.Clear();
            cmbMapVersion.Items.Clear();
            if (!Settings.Default.CurrentSyncNav)
            {
                cmbMapVersion.Items.Add("Non Nav APIM Selected");
                cmbMapVersion.SelectedItem = "Non Nav APIM Selected";
            }
            _jsonReleases = JsonConvert.DeserializeObject<JsonReleases>(_stringReleasesJson);
            foreach (Data item in _jsonReleases.data)
                if (item.regions.Contains(cmbRegion.Text))
                    cmbRelease.Items.Add(item.name);
            if (cmbRegion.SelectedIndex >= 0) cmbRelease.Enabled = true;
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancelcopy = true;
            Settings.Default.Save();
        }

        private JsonReleases _jsonReleases;

        private void cmbDriveList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbDriveList.SelectedIndex >= 1)
            {
                _kvpDriveList = (KeyValuePair<string, string>)cmbDriveList.SelectedItem;
                UpdateDriveInfo();
            }
            else
            {
                lblDriveInfo.Text = "Drive Name:" + Environment.NewLine + "Drive Letter:" + Environment.NewLine + "Filesystem:";
            }
        }

        private void UpdateDriveInfo()
        {
            string partitionQueryText =
                $"associators of {{{_kvpDriveList.Key}}} where AssocClass = Win32_DiskDriveToDiskPartition";
            ManagementObjectSearcher partitionQuery = new ManagementObjectSearcher(partitionQueryText);
            foreach (ManagementBaseObject o in partitionQuery.Get())
            {
                ManagementObject p = (ManagementObject)o;
                string logicalDriveQueryText =
                    $"associators of {{{p.Path.RelativePath}}} where AssocClass = Win32_LogicalDiskToPartition";
                ManagementObjectSearcher logicalDriveQuery = new ManagementObjectSearcher(logicalDriveQueryText);
                foreach (ManagementBaseObject managementBaseObject in logicalDriveQuery.Get())
                {
                    ManagementObject ld = (ManagementObject)managementBaseObject;
                    _driveId = Convert.ToString(ld.Properties["DeviceId"].Value);
                    _partitionType = p.Properties["Type"].Value.ToString().Contains("GPT:") ? "GPT" : "MBR";

                    _fileSystem = Convert.ToString(ld.Properties["FileSystem"].Value);
                    _volumeName = Convert.ToString(ld.Properties["VolumeName"].Value);
                }

                BeginInvoke(new Action(() =>
                    lblDriveInfo.Text =
                        string.Format(
                            "Drive Name:" + @" {0}" + Environment.NewLine + "Drive Letter:" + @" {1}" + Environment.NewLine + "Filesystem:" +
                            @" {2}", _volumeName, _driveId, _fileSystem, _partitionType)));
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
            ManagementObjectSearcher driveQuery = new ManagementObjectSearcher("select * from Win32_DiskDrive WHERE MediaType='Removable media'");
            foreach (ManagementBaseObject o in driveQuery.Get())
            {
                ManagementObject d = (ManagementObject)o;
                string diskName = Convert.ToString(d.Properties["Caption"].Value);
                string friendlySize = BytesToString(Convert.ToInt64(d.Properties["Size"].Value));
                _dicDriveList.Add(d.Path.RelativePath, $"{diskName} (Size: {friendlySize})");
            }

            cmbDriveList.DataSource = new BindingSource(_dicDriveList, null);
        }

        private void btnManualConfiguration_Click(object sender, EventArgs e)
        {
            _mode = "autoinstall";
                lstIVSU.Items.Clear();
                HttpResponseMessage response = Client.GetAsync(UrlApiAppReleaseSingle + cmbRelease.Text).Result;
                _stringDownloadJson = response.Content.ReadAsStringAsync().Result;
                JsonReleases jsonIvsUs = JsonConvert.DeserializeObject<JsonReleases>(_stringDownloadJson);
                foreach (Ivsus item in jsonIvsUs.data[0].ivsus)
                    if (item.ivsu.regions.Contains("ALL") || item.ivsu.regions.Contains(cmbRegion.Text))
                        if (item.ivsu.type != "MAP" && item.ivsu.type != "MAP_LICENSE" && item.ivsu.type != "VOICE_NAV")
                            lstIVSU.Items.Add(new ListViewItem(new[] { item.ivsu.type, item.ivsu.name, item.ivsu.version, item.ivsu.url, item.ivsu.md5 }));
        }

        private void btnAutoinstallStart_Click(object sender, EventArgs e)
        {
            _prevprogressPercentageInt = 0;
            grpUSB.Enabled = false;
            grpNewVersion.Enabled = false;
            Directory.CreateDirectory(_downloadpath);
            _dicIvsUs.Clear();
            lblTotalDownloadProgress.Text = strings.Total_Download_Progress;
            barTotalDownloadProgress.Maximum = 0;

            foreach (ListViewItem item in lstIVSU.CheckedItems)
            {
                _fileName = item.SubItems[3].Text.Substring(item.SubItems[3].Text.LastIndexOf("/", StringComparison.Ordinal) + 1,
                    item.SubItems[3].Text.Length - item.SubItems[3].Text.LastIndexOf("/", StringComparison.Ordinal) - 1);
                if (!ValidateDownloadedFile(item.SubItems[3].Text, _downloadpath + _fileName, item.SubItems[4].ToString().ToLower()))
                {
                    barTotalDownloadProgress.Maximum += 100;
                    _downloadfiles.Enqueue(item.SubItems[3].Text);
                    BeginInvoke(new Action(() => lstDownloadQueue.Items.Add(item.SubItems[3].Text)));
                }

                _dicIvsUs.Add(_fileName, item.SubItems[0].Text);
            }
            _tokenSource.Dispose();
            _tokenSource = new CancellationTokenSource();
            _totalcount = _downloadfiles.Count;
            Task t = new Task(DownloadFile);
            t.Start();
        }

        private JsonReleases _jsonMapReleases;
        #endregion

        #region Borderless Window Move / Close/Minimize
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private static extern void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private static extern void SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void panelTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(Handle, 0x112, 0xf012, 0);
        }

        private void btnClose_MouseHover(object sender, EventArgs e)
        {
            btnClose.Image = Resources.close;
        }

        private void btnMinimize_MouseHover(object sender, EventArgs e)
        {
            btnClose.Image = Resources.minimize;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void btnWindowControls_MouseLeave(object sender, EventArgs e)
        {
            ((PictureBox)sender).Image = Resources.button;
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }
        #endregion

        private void FrmMain_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists(Settings.Default.DownloadPath) && Settings.Default.DownloadPath != "")
            {
                MessageBox.Show("Download directory does not exist, please select a new location", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _ = new FrmSetup() { Visible = true };
            } 
            else if (!Directory.Exists(Settings.Default.DownloadPath) && Settings.Default.DownloadPath == "")
            {
                Directory.CreateDirectory(_downloadpath);
            }
            else
            {
                _downloadpath = Settings.Default.DownloadPath;
            }

            foreach (string arg in Environment.GetCommandLineArgs())
                switch (arg)
                {
                    case "/updated":
                        Settings.Default.Upgrade();
                        Settings.Default.Save();
                        break;
                    case "/all":
                        _boolShowAllReleases = true;
                        break;
                }

            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                AutoUpdater.Start("https://cyanlabs.net/api/latest.php?product=" + Application.ProductName);

                if (Settings.Default.ShowAllReleases || _boolShowAllReleases)
                {
                    HttpResponseMessage response = Client.GetAsync(UrlApiAppReleasesAll).Result;
                    _stringReleasesJson = response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    HttpResponseMessage response = Client.GetAsync(UrlApiAppReleases).Result;
                    _stringReleasesJson = response.Content.ReadAsStringAsync().Result;
                }

                cmbDriveList.DisplayMember = "Value";
                cmbDriveList.ValueMember = "Key";
                RefreshUsb();
            }
            catch (WebException webex)
            {
                MessageBox.Show("There was an issue trying to retrieve the list of files from the online server, please check your internet connection and make sure the application has internet access" + Environment.NewLine + webex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }

        private void FrmMain_Shown(object sender, EventArgs e)
        {
            lblVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            lblConfiguration.Text =
                $"Current Version: {Settings.Default.CurrentSyncVersion} Region: {Settings.Default.CurrentSyncRegion} Navigation: {(Settings.Default.CurrentSyncNav ? "Yes" : "No")}" + 
                Environment.NewLine + $"Download Path: { Settings.Default.DownloadPath}";
            
            this.Size = new Size(620, 320);

            if (!Settings.Default.TOCAccepted2)
            {
                _ = new FrmDisclaimer { Visible = true };
                this.Hide();
            }
            else if (!Settings.Default.SetupCompleted)
            {
                _ = new FrmSetup { Visible = true };
                this.Hide();
            }

            if (!Settings.Default.CurrentSyncNav)
            {
                cmbMapVersion.Items.Add("Non Nav APIM Selected");
                cmbMapVersion.SelectedItem = "Non Nav APIM Selected";
                cmbMapVersion.Enabled = false;
            }
        }

        bool canceldownload = false;
        private void btnContinueNewVersion_Click(object sender, EventArgs e)
        {
            canceldownload = false;

            if (cmbRegion.Text != Settings.Default.CurrentSyncRegion && canceldownload == false)
            {
                DialogResult dialogOpenwebsite = MessageBox.Show(
                    strings.Region_Mismatch_Message, strings.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning
                );
                if (dialogOpenwebsite != DialogResult.Yes) canceldownload = true;
            }

            if (cmbMapVersion.Text != @"No Maps" && cmbMapVersion.Text != @"Non Nav APIM Selected" && Settings.Default.CurrentSyncNav == false && canceldownload == false &&
                Settings.Default.CurrentSyncVersion.ToString() != @"SYNC 3.4.19274+")
            {
                DialogResult dialogOpenwebsite = MessageBox.Show(
                    strings.Nav_On_Non_Nav_Message, strings.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning
                );
                if (dialogOpenwebsite != DialogResult.No) canceldownload = true;
            }

            if (cmbMapVersion.Text == @"No Maps" && Settings.Default.CurrentSyncNav && canceldownload == false &&
                Settings.Default.CurrentSyncVersion.ToString() != @"SYNC 3.4.19274+")
            {
                DialogResult dialogOpenwebsite = MessageBox.Show(
                    strings.No_Maps_Nav_Message,
                    strings.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning
                );
                if (dialogOpenwebsite == DialogResult.No) canceldownload = true;
            }

            if (cmbDriveList.SelectedIndex == 0 && canceldownload == false)
            {
                DialogResult dialogOpenwebsite = MessageBox.Show(
                    strings.No_USB_Selected_Message,
                    strings.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning
                );
                if (dialogOpenwebsite == DialogResult.Yes)
                    _downloadonly = true;
                else
                    canceldownload = true;
            }

            if (canceldownload == false && _downloadonly == false)
            {
                string selectedDisk = ((KeyValuePair<string, string>)cmbDriveList.SelectedItem).Value;
                DialogResult dialogResult = MessageBox.Show(
                    string.Format(
                        strings.USB_Format_Message_1 + Environment.NewLine + Environment.NewLine + @"{0}" + Environment.NewLine +
                        strings.DriveInfo_2 + @" {1}", selectedDisk, _driveId),
                    strings.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (dialogResult == DialogResult.No) canceldownload = true;
            }

            if (canceldownload) return;

            this.Size = new Size(620, 650);
            btnContinueNewVersion.Enabled = false;
            grpNewVersion.Enabled = false;
            grpUSB.Enabled = false;
            btnCancel.Enabled = true;
            barTotalDownloadProgress.Maximum = 0;
            _prevprogressPercentageInt = 0;
            txtProcessLog.Clear();
            _cmbSyncVersionText = cmbRelease.Text;

            //LESS THAN 3.2
            if (Settings.Default.CurrentSyncVersion < SyncReformatVersion)
            {
                Reformat();
            }

            //Between 3.2 and 3.4.19274
            else if (Settings.Default.CurrentSyncVersion >= SyncReformatVersion && Settings.Default.CurrentSyncVersion < SyncBlacklistedVersion)
            {
                //Update Nav?
                if (cmbMapVersion.Text == "No Maps" || cmbMapVersion.Text == "Non Nav APIM Selected" || cmbMapVersion.Text == "Keep Existing Maps")
                {
                    Autoinstall();
                }
                else
                {
                    Reformat();
                }
            }

            //3.4.19274 or above
            else if (Settings.Default.CurrentSyncVersion >= SyncBlacklistedVersion)
            {
                //Update Nav?
                if (cmbMapVersion.Text == "No Maps" || cmbMapVersion.Text == "Non Nav APIM Selected" || cmbMapVersion.Text == "Keep Existing Maps")
                {
                    Autoinstall();
                }
                else
                {
                    Downgrade();
                }
            }
        }

        private void Autoinstall()
        {
            _mode = "autoinstall";
            tabControl1.SelectedTab = tabAutoInstall;
            lstIVSU.Items.Clear();
            HttpResponseMessage response = Client.GetAsync(UrlApiAppReleaseSingle + cmbRelease.Text).Result;
            _stringDownloadJson = response.Content.ReadAsStringAsync().Result;
            JsonReleases jsonIvsUs = JsonConvert.DeserializeObject<JsonReleases>(_stringDownloadJson);
            foreach (Ivsus item in jsonIvsUs.data[0].ivsus)
                if (item.ivsu.regions.Contains("ALL") || item.ivsu.regions.Contains(cmbRegion.Text))
                    if (item.ivsu.type != "MAP" && item.ivsu.type != "MAP_LICENSE" && item.ivsu.type != "VOICE_NAV")
                        lstIVSU.Items.Add(new ListViewItem(new[] { item.ivsu.type, item.ivsu.name, item.ivsu.version, item.ivsu.url, item.ivsu.md5 }));
            foreach (ListViewItem item in lstIVSU.Items)
                item.Checked = true;
        }

        private void Downgrade()
        {
            lblManualWarning.Text = strings.lblDowngrade_Text;

            _mode = "downgrade";
            tabControl1.SelectedTab = tabAutoInstall;
            lstIVSU.Items.Clear();
            HttpResponseMessage response = Client.GetAsync(UrlApiAppReleaseSingle + "DOWNGRADE").Result;
            _stringDownloadJson = response.Content.ReadAsStringAsync().Result;
            JsonReleases jsonIvsUs = JsonConvert.DeserializeObject<JsonReleases>(_stringDownloadJson);
            foreach (Ivsus item in jsonIvsUs.data[0].ivsus)
                if (item.ivsu.regions.Contains("ALL") || item.ivsu.regions.Contains(cmbRegion.Text))
                    if (item.ivsu.type != "MAP" && item.ivsu.type != "MAP_LICENSE" && item.ivsu.type != "VOICE_NAV")
                        lstIVSU.Items.Add(new ListViewItem(new[] { item.ivsu.type, item.ivsu.name, item.ivsu.version, item.ivsu.url, item.ivsu.md5 }));
            foreach (ListViewItem item in lstIVSU.Items)
                item.Checked = true;
        }

        private void Reformat()
        {
            _mode = "reformat";
            _totalcount = -1;
            
            string reformattool = _downloadpath + @"1u5t-14g386-cb.tar.gz";
            if (!File.Exists(reformattool) || CalculateMd5(reformattool) != "75E08C3EED8D2039BAF65B6156F79106")
                _downloadfiles.Enqueue(new Uri(UrlReformatTool).ToString());

            Directory.CreateDirectory(_downloadpath);

            HttpResponseMessage response = Client.GetAsync(UrlApiAppReleaseSingle + cmbRelease.Text).Result;
            _stringDownloadJson = response.Content.ReadAsStringAsync().Result;
            response = Client.GetAsync(UrlApiMapReleaseSingle + cmbMapVersion.Text).Result;
            _stringMapDownloadJson = response.Content.ReadAsStringAsync().Result;
            _dicIvsUs.Clear();
            lblTotalDownloadProgress.Text = strings.Total_Download_Progress;
            _tokenSource.Dispose();
            _tokenSource = new CancellationTokenSource();
            UpdateLog($"INFO: Starting process ({cmbRelease.Text} - {cmbRegion.Text} - {cmbMapVersion.Text})");
            Task t = new Task(ValidateDownloadedFiles);
            t.Start();
        }

        private void btnShowConfiguration_Click(object sender, EventArgs e)
        {
            _ = new FrmSetup() { Visible = true };
            this.Hide();
        }

        private void ValidateDownloadedFiles()
        {
            string cmbRegionText = "", cmbMapVersionText = "";
            Invoke((MethodInvoker)delegate
            {
                cmbRegionText = cmbRegion.Text;
                cmbMapVersionText = cmbMapVersion.Text;
            });
            JsonReleases jsonIvsUs = JsonConvert.DeserializeObject<JsonReleases>(_stringDownloadJson);
            foreach (var item in jsonIvsUs.data[0].ivsus)
            {
                if (!item.ivsu.regions.Contains("ALL") && !item.ivsu.regions.Contains(cmbRegionText)) continue;
                if (item.ivsu.type == "MAP" || item.ivsu.type == "MAP_LICENSE" || item.ivsu.type == "VOICE_NAV") continue;
                _fileName = item.ivsu.url.Substring(item.ivsu.url.LastIndexOf("/", StringComparison.Ordinal) + 1,
                    item.ivsu.url.Length - item.ivsu.url.LastIndexOf("/", StringComparison.Ordinal) - 1);

                if (!_validatedIvsUsList.Contains(_fileName))
                {
                    if (!ValidateDownloadedFile(item.ivsu.url, _downloadpath + _fileName, item.ivsu.md5))
                    {
                        _downloadfiles.Enqueue(item.ivsu.url);
                        BeginInvoke(new Action(() => lstDownloadQueue.Items.Add(item.ivsu.url)));
                    }
                }
                if (!_dicIvsUs.ContainsKey(_fileName)) _dicIvsUs.Add(_fileName, item.ivsu.type);
            }

            if (cmbMapVersionText != "No Maps")
            {
                JsonReleases jsonMapIvsUs = JsonConvert.DeserializeObject<JsonReleases>(_stringMapDownloadJson);
                foreach (Ivsus item in jsonMapIvsUs.data[0].ivsus)
                {
                    _fileName = item.map_ivsu.url.Substring(item.map_ivsu.url.LastIndexOf("/", StringComparison.Ordinal) + 1,
                        item.map_ivsu.url.Length - item.map_ivsu.url.LastIndexOf("/", StringComparison.Ordinal) - 1);

                    if (!_validatedIvsUsList.Contains(_fileName))
                    {
                        if (!ValidateDownloadedFile(item.map_ivsu.url, _downloadpath + _fileName, item.map_ivsu.md5))
                        {
                            _downloadfiles.Enqueue(item.map_ivsu.url);
                            BeginInvoke(new Action(() => lstDownloadQueue.Items.Add(item.map_ivsu.url)));
                        }
                    }

                    if (!_dicIvsUs.ContainsKey(_fileName)) _dicIvsUs.Add(_fileName, item.map_ivsu.type);
                }
            }

            if (_totalcount == -1)
            {
                _totalcount = _downloadfiles.Count;
                BeginInvoke(new Action(() => barTotalDownloadProgress.Maximum = 100 * _totalcount));
            }

            Task t = new Task(DownloadFile);
            t.Start();
        }

        private async void DownloadFile()
        {
            while (true)
            {
                if (_downloadfiles.Any())
                {
                    BeginInvoke(new Action(() =>
                    {
                        int filecount = _downloadfiles.Count + 1;
                        lblFilesRemaining.Text = filecount == 1 ? "1 file to download" : filecount + " files to download";
                        Console.WriteLine(filecount);
                    }));
                    string downloadFileUrl = _downloadfiles.Dequeue();

                    _fileName = downloadFileUrl.Contains("dropbox")
                        ? @"1u5t-14g386-cb.tar.gz"
                        : downloadFileUrl.Substring(downloadFileUrl.LastIndexOf("/", StringComparison.Ordinal) + 1,
                            downloadFileUrl.Length - downloadFileUrl.LastIndexOf("/", StringComparison.Ordinal) - 1);
                    UpdateLog("INFO: Downloading " + _fileName);
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

                                BeginInvoke(new Action(() =>
                                {
                                    lblFileName.Text = string.Format(strings.Downloading + @" {0} ({1}/{2})", _fileName,
                                        BytesToString(totalBytesDownloadedInt64), BytesToString(totalFileSizeInt64));
                                    lblDownloadSize.Text = $@"{remainingTime.Minutes:00}m {remainingTime.Seconds:00}s @ {BytesToString(bs)}/S";
                                    barTotalDownloadProgress.Value = progressTotalPercentage;
                                    barDownloadProgress.Value = progressPercentageInt;
                                    TaskbarManager.Instance.SetProgressValue(progressTotalPercentage, 100 * _totalcount);
                                }));
                            }
                        };
                        await _httpclient.StartDownload(_tokenSource.Token);
                    }

                    _prevprogressPercentageInt += 100;
                    BeginInvoke(new Action(() => lstDownloadQueue.Items.Remove(downloadFileUrl)));
                    continue;
                }

                if (!_downloadfiles.Any() && _downloadcomplete == false && _mode == "reformat")
                {
                    _downloadcomplete = true;
                    ValidateDownloadedFiles();
                }
                else if (!_downloadfiles.Any() && _downloadcomplete == false && _mode == "autoinstall")
                {
                    _downloadcomplete = true;
                    continue;
                }
                else if (!_downloadfiles.Any() && _downloadcomplete)
                {
                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
                    _downloadcomplete = false;

                    BeginInvoke(new Action(() =>
                    {
                        btnCancel.Enabled = false;
                        lblFileName.Text = "";
                        lblDownloadSize.Text = "";
                        lblFilesRemaining.Text = "";
                        grpNewVersion.Enabled = true;
                        TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
                    }));

                    if (_tokenSource.IsCancellationRequested)
                    {
                        UpdateLog("INFO: Downloads cancelled!");
                    }
                    else
                    {
                        UpdateLog("INFO: All downloads completed and verified!");

                        _prevprogressPercentageInt = 0;

                        if (_downloadonly)
                            MessageBox.Show(strings.All_Downloads_Complete_Message, strings.Success, MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        else
                            PrepareUsb();
                    }
                }

                break;
            }
        }

        private void CopyFiles()
        {
            _cancelcopy = false;
            _stopWatch = new Stopwatch();
            _totalcount = _dicIvsUs.Count;
            if (_totalcount == -1) BeginInvoke(new Action(() => barTotalDownloadProgress.Maximum = 100 * _totalcount));
            try
            {
                foreach (KeyValuePair<string, string> item in _dicIvsUs)
                {
                    BeginInvoke(new Action(() =>
                        lblFilesRemaining.Text = lstDownloadQueue.Items.Count == 1 ? "1 file left to copy" : lstDownloadQueue.Items.Count + " files left to copy"));
                    _fileName = item.Key;
                    _stopWatch.Restart();
                    do
                    {
                        if (_cancelcopy) break;
                        UpdateLog("INFO: " + strings.Copying + " " + _fileName);
                        CopyFileEx(_downloadpath + item.Key, _driveId + @"\SyncMyRide\" + item.Key, CopyProgressHandler, IntPtr.Zero, ref _pbCancel,
                            CopyFileFlags.CopyFileRestartable);
                    } while (!ValidateCopiedFile(_downloadpath + item.Key, _driveId + @"\SyncMyRide\" + item.Key));
                    _prevprogressPercentageInt += 100;
                    BeginInvoke(new Action(() => lstDownloadQueue.Items.Remove(_downloadpath + item.Key)));
                }
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e);
            }


            Invoke((MethodInvoker)delegate
            {
                if (_cancelcopy == false)
                {
                    barTotalDownloadProgress.Value = barTotalDownloadProgress.Maximum;
                    TaskbarManager.Instance.SetProgressValue(100 * _totalcount, 100 * _totalcount);
                    lblFilesRemaining.Text = "";
                    UpdateLog("INFO: ALL FILES DOWNLOADED AND COPIED TO THE USB DRIVE SUCCESSFULLY!");
                    lblDownloadSize.Text = "";
                    lblFileName.Text = strings.Process_Complete;
                    grpUSB.Enabled = true;
                    grpNewVersion.Enabled = true;
                }

            });
            GenerateLog();
            if (_mode == "autoinstall")
            {
                MessageBox.Show(
                    strings.Manual_Mode_USB_Message,
                    strings.Success, MessageBoxButtons.OK, MessageBoxIcon.Information
                );
            }
            else if (_mode == "reformat")
            {
                DialogResult dialogOpenwebsite = MessageBox.Show(
                    strings.Process_Complete_Message, strings.Success, MessageBoxButtons.YesNo, MessageBoxIcon.Information
                );

                if (dialogOpenwebsite == DialogResult.Yes)
                    Process.Start(_cmbSyncVersionText.Contains("DOWNGRADE")
                        ? "https://cyanlabs.net/tutorials/update-ford-sync-3-2-2-3-0-to-version-3-4-all-years-3-4-19200/#downgradingcar"
                        : "https://cyanlabs.net/tutorials/update-ford-sync-3-2-2-3-0-to-version-3-4-all-years-3-4-19200/#updating");
            }
            else
            {

            }
        }

        #region Helper Functions

        private bool ValidateDownloadedFile(string newfileurl, string existingfile, string md5)
        {
            if (!File.Exists(existingfile)) return false;
            UpdateLog("INFO: Verifying integrity of " + _fileName);
            if (md5 == null)
            {
                UpdateLog("WARN: No MD5 value found in database, unable to verify " + _fileName);
                UpdateLog("WARN: Doing a simple filesize comparison instead");
                using (HttpClient client = new HttpClient())
                {
                    long newfilesize = -1;
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, new Uri(newfileurl));

                    if (long.TryParse(client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result.Content.Headers.ContentLength.ToString(),
                        out long contentLength))
                        newfilesize = contentLength;

                    long filesize = new FileInfo(existingfile).Length;
                    if (newfilesize == filesize)
                    {
                        UpdateLog("INFO: Successfully Verified " + _fileName);
                        _validatedIvsUsList.Add(_fileName);
                        return true;
                    }
                }
            }
            if (string.Equals(CalculateMd5(existingfile), md5, StringComparison.CurrentCultureIgnoreCase))
            {
                UpdateLog("INFO: Successfully Verified " + _fileName);
                _validatedIvsUsList.Add(_fileName);
                return true;
            }
            BeginInvoke(new Action(() => barTotalDownloadProgress.Maximum += 100));
            UpdateLog("WARN: Failed to verify " + _fileName + " redownloading file");
            return false;
        }

        private void cmbMapVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbMapVersion.Text != "" && cmbRelease.Text != "")
            {
                btnContinueNewVersion.Enabled = true;
            }
            else
            {
                btnContinueNewVersion.Enabled = false;
            }
        }

        private void btnAutoinstall_Click(object sender, EventArgs e)
        {
            _prevprogressPercentageInt = 0;
            grpUSB.Enabled = false;
            grpNewVersion.Enabled = false;
            Directory.CreateDirectory(_downloadpath);
            _dicIvsUs.Clear();
            tabControl1.SelectedTab = tabStatus;
            lblTotalDownloadProgress.Text = strings.Total_Download_Progress;
            barTotalDownloadProgress.Maximum = 0;

            foreach (ListViewItem item in lstIVSU.CheckedItems)
            {
                _fileName = item.SubItems[3].Text.Substring(item.SubItems[3].Text.LastIndexOf("/", StringComparison.Ordinal) + 1,
                    item.SubItems[3].Text.Length - item.SubItems[3].Text.LastIndexOf("/", StringComparison.Ordinal) - 1);
                if (!ValidateDownloadedFile(item.SubItems[3].Text, _downloadpath + _fileName, item.SubItems[4].Text.ToString().ToLower()))
                {
                    barTotalDownloadProgress.Maximum += 100;
                    _downloadfiles.Enqueue(item.SubItems[3].Text);
                    BeginInvoke(new Action(() => lstDownloadQueue.Items.Add(item.SubItems[3].Text)));
                }

                _dicIvsUs.Add(_fileName, item.SubItems[0].Text);
            }
            _tokenSource.Dispose();
            _tokenSource = new CancellationTokenSource();
            _totalcount = _downloadfiles.Count;
            Task t = new Task(DownloadFile);
            t.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(CalculateMd5(@"E:\Downloads\Sync3Updater\5U5T-14G381-ED_1591670365000.TAR.GZ"));
        }

        private bool ValidateCopiedFile(string downloadedfile, string usbfile)
        {
            if (!File.Exists(downloadedfile) || !File.Exists(usbfile)) return false;

            long downloadedfilesize = new FileInfo(downloadedfile).Length;
            long copiedfilesize = new FileInfo(usbfile).Length;

            if (downloadedfilesize == copiedfilesize)
            {
                UpdateLog("INFO: Verifying integrity of " + _fileName + " (this could take some time)");
                string downloadedfilemd5 = CalculateMd5(downloadedfile);
                string usbfilemd5 = CalculateMd5(usbfile);
                UpdateLog("INFO: Successfully Verified " + _fileName);
                return downloadedfilemd5 == usbfilemd5;
            }
            BeginInvoke(new Action(() => barTotalDownloadProgress.Maximum += 100));
            UpdateLog("WARN: Failed to verify " + _fileName + " redownloading file");
            return false;
        }

        private void cmbRelease_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            foreach (Data item in _jsonReleases.data)
                if (item.name == cmbRelease.Text)
                {
                    _stringCompatibility = item.version.Substring(0, 3);
                    if (item.notes != null) txtReleaseNotes.Text = strings.Notes + @" " + item.notes.Replace("\n", Environment.NewLine);
                }

            if (Settings.Default.ShowAllReleases || _boolShowAllReleases)
            {
                HttpResponseMessage response = Client.GetAsync(UrlApiMapReleasesAll + cmbRegion.Text + "&filter[compatibility][contains]=" + _stringCompatibility).Result;
                _stringMapReleasesJson = response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                HttpResponseMessage response = Client.GetAsync(UrlApiMapReleases + cmbRegion.Text + "&filter[compatibility][contains]=" + _stringCompatibility).Result;
                _stringMapReleasesJson = response.Content.ReadAsStringAsync().Result;
            }

            if (Settings.Default.CurrentSyncNav)
            {
                cmbMapVersion.Items.Clear();
                cmbMapVersion.Items.Add("No Maps");

                _jsonMapReleases = JsonConvert.DeserializeObject<JsonReleases>(_stringMapReleasesJson);
                foreach (Data item in _jsonMapReleases.data) cmbMapVersion.Items.Add(item.name);
                if (cmbRelease.SelectedIndex >= 0 && Settings.Default.CurrentSyncNav) cmbMapVersion.Enabled = true;
            }

            if (cmbMapVersion.Text != "" && cmbRelease.Text != "")
            {
                btnContinueNewVersion.Enabled = true;
            }
            else
            {
                btnContinueNewVersion.Enabled = false;
            }

        }

        private void txtProcessLog_TextChanged(object sender, EventArgs e)
        {
            txtProcessLog.SelectionStart = txtProcessLog.Text.Length;
            txtProcessLog.ScrollToCaret();
        }

        private void UpdateLog(string text)
        {
            if (txtProcessLog.InvokeRequired)
                BeginInvoke(new Action(() => txtProcessLog.AppendText(text + Environment.NewLine)));
            else
                txtProcessLog.AppendText(text + Environment.NewLine);
            Console.WriteLine(text);
        }


        private static string CalculateMd5(string filename)
        {
            using (var stream = new BufferedStream(File.OpenRead(filename), 100000))
            {
                return BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(stream)).Replace("-", string.Empty);
            }
        }

        private static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0) return "0" + suf[0];

            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return Math.Sign(byteCount) * num + suf[place];
        }

        //private void LocalizationSetup()
        //{
        //    if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName != "en")
        //    {
        //        webDisclaimer.Navigate(
        //            $"https://translate.google.co.uk/translate?hl=&sl=en&tl={Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName}&u=https%3A%2F%2Fcyanlabs.net%2Fapi%2FFordSyncDownloader%2Fdisclaimer.php");
        //    }
        //    cmbLocale.Text = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
        //    btnCommunityForumlink.Text = strings.btnCommunityForumlink_Text;
        //    btnDisclaimerContinue.Text = strings.btnDisclaimerContinue_Text;
        //    btnDonate.Text = strings.btnDonate_Text;
        //    btnFormatUSB.Text = strings.btnFormatUSB_Text;
        //    btnIVSUDBLink.Text = strings.btnIVSUDBLink_Text;
        //    btnLink1.Text = strings.btnLink1_Text;
        //    btnLink2.Text = strings.btnLink2_Text;
        //    btnRefreshUSB.Text = strings.btnRefreshUSB_Text;
        //    btnStep1.Text = strings.btnStep1_Text;
        //    btnStep2.Text = strings.btnStep2_Text;
        //    btnStep3.Text = strings.btnStep3_Text;
        //    btnManualConfiguration.Text = strings.button1_Text;
        //    chkDisclaimerConfirm.Text = strings.chkDisclaimerConfirm_Text;
        //    chkSyncNav.Text = strings.chkSyncNav_Text;
        //    grpLog.Text = strings.groupBox1_Text;
        //    grpDownload.Text = strings.grpDownload_Text;
        //    grpExistingDetails.Text = strings.grpExistingDetails_Text;
        //    grpNewVersion.Text = strings.grpNewVersion_Text;
        //    grpUSB.Text = strings.grpUSB_Text;
        //    lblMapVersion1.Text = strings.label1_Text;
        //    lblManualWarning.Text = strings.label4_Text;
        //    lblDowngrade.Text = strings.lblDowngrade_Text;
        //    lblDownloadDirectory.Text = strings.lblDownloadDirectory_Text;
        //    lblDownloadQueue.Text = strings.lblDownloadQueue_Text;
        //    lblDriveInfo.Text = strings.lblDownloadQueue_Text;
        //    lblGetDetails.Text = strings.lblGetDetails_Text;
        //    lblHelp.Text = strings.lblHelp_Text;
        //    lblIssueDesc.Text = strings.lblIssueDesc_Text;
        //    lblLinks.Text = strings.lblLinks_Text;
        //    lblRegion.Text = strings.lblRegion_Text;
        //    lblSyncRegion.Text = strings.lblRegion_Text;
        //    lblRelease.Text = strings.Version;
        //    lblSyncVersion.Text = strings.Version;
        //    lblTotalDownloadProgress.Text = strings.lblTotalDownloadProgress_Text;
        //    lvIVSUsName.Text = strings.lvIVSUsName_Text;
        //    lvIVSUsType.Text = strings.lvIVSUsType_Text;
        //    lvIVSUsVersion.Text = strings.lvIVSUsVersion_Text;
        //    btnUploadLog.Text = strings.btnUploadLog;
        //    btnCancel.Text = strings.btnCancel_text;
        //}

        #region CopyFileEx

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CopyFileEx(string lpExistingFileName, string lpNewFileName,
            CopyProgressRoutine lpProgressRoutine, IntPtr lpData, ref int pbCancel,
            CopyFileFlags dwCopyFlags);

        private delegate CopyProgressResult CopyProgressRoutine(long totalFileSize, long totalBytesTransferred,
            long streamSize, long streamBytesTransferred, uint dwStreamNumber, IntPtr hSourceFile,
            IntPtr hDestinationFile, IntPtr lpData);

        private int _pbCancel;

        [Flags]
        private enum CopyFileFlags : uint
        {
            CopyFileRestartable = 0x00000002
        }

        private enum CopyProgressResult : uint
        {
            ProgressContinue = 0,
            ProgressCancel = 1
        }

        #endregion

        private void GenerateLog()
        {
            Invoke((MethodInvoker)delegate
            {
                string data = "CYANLABS - FORD SYNC DOWNLOADER - V" + Assembly.GetExecutingAssembly().GetName().Version + Environment.NewLine;
                data += Environment.NewLine;
                data += "PREVIOUS CONFIGURATION\n";
                data += "Version: " + Settings.Default.CurrentSyncVersion.ToString() + Environment.NewLine;
                data += "Region: " + Settings.Default.CurrentSyncRegion + Environment.NewLine;
                data += "Navigation: " + Settings.Default.CurrentSyncNav + Environment.NewLine;
                data += "Mode: " + _mode + Environment.NewLine; 
                data += Environment.NewLine;
                data += "USB DETAILS\n";
                data += "Model: " + cmbDriveList.Text + Environment.NewLine;
                data += "FileSystem: " + _fileSystem + Environment.NewLine;
                data += "Partition Type: " + _partitionType + Environment.NewLine;

                if (File.Exists(_driveId + @"\reformat.lst"))
                {
                    data += Environment.NewLine;
                    data += "REFORMAT.LST" + Environment.NewLine;
                    data += File.ReadAllText(_driveId + @"\reformat.lst") + Environment.NewLine;
                }

                if (File.Exists(_driveId + @"\autoinstall.lst"))
                {
                    data += Environment.NewLine;
                    data += "AUTOINSTALL.LST" + Environment.NewLine;
                    data += File.ReadAllText(_driveId + @"\autoinstall.lst");
                }

                if (Directory.Exists(_driveId + @"\SyncMyRide"))
                {
                    data += Environment.NewLine;
                    DirectoryInfo di = new DirectoryInfo(_driveId + @"\SyncMyRide");
                    FileInfo[] allFiles = di.GetFiles("*", SearchOption.AllDirectories);
                    data += "SYNCMYRIDE FILES (" + allFiles.Length + ")" + Environment.NewLine;
                    foreach (FileInfo file in allFiles) data += $"{file.Name} ({BytesToString(file.Length)})" + Environment.NewLine;
                }

                data += Environment.NewLine;
                data += "LOG" + Environment.NewLine;
                data += txtProcessLog.Text;
                File.WriteAllText(_driveId + @"\log.txt", data);
            });
        }
        #endregion

        #region Ford lst Generation

        private void CreateDowngrade()
        {
            string cmbReleaseText = "", cmbRegionText = "";
            Invoke((MethodInvoker)delegate
            {
                cmbReleaseText = cmbRelease.Text;
                cmbRegionText = cmbRegion.Text;
            });
            string autoinstalllst = "; Generated by CyanLabs Ford Sync Downloader - " +
                                    cmbReleaseText + " " + cmbRegionText +
                                    Environment.NewLine + Environment.NewLine + "[SYNCGen3.0_ALL]" + Environment.NewLine;

            int i = 0;
            foreach (KeyValuePair<string, string> item in _dicIvsUs)
            {
                i++;
                autoinstalllst += string.Format(@"Item{0} = {1} - SyncMyRide\{2}\rOpen{0} = SyncMyRide\{2}\r", i, item.Value, item.Key)
                    .Replace(@"\r", Environment.NewLine);
            }

            autoinstalllst += Environment.NewLine + "Options = AutoInstall" + Environment.NewLine;
            UpdateLog("INFO: Creating autoinstall.lst on root of selected USB Drive for 'Downgrade'");
            File.WriteAllText(_driveId + @"\autoinstall.lst", autoinstalllst);
            File.Create(_driveId + @"\DONTINDX.MSA");
        }

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
            foreach (KeyValuePair<string, string> item in _dicIvsUs)
            {
                if (item.Key == "1u5t-14g386-cb.tar.gz") continue;
                i++;
                reformatlst += item.Value + "=" + item.Key;
                if (i != _dicIvsUs.Count) reformatlst += Environment.NewLine;
            }
            UpdateLog("INFO: Creating reformat.lst on root of selected USB Drive");

            File.WriteAllText(_driveId + @"\reformat.lst", reformatlst);

            string autoinstalllst = "; Generated by CyanLabs Ford Sync Downloader - " +
                                    cmbReleaseText + " " + cmbRegionText +
                                    Environment.NewLine + Environment.NewLine + "[SYNCGen3.0_ALL]" + Environment.NewLine;

            autoinstalllst +=
                @"Item1 = 1U5T-14G386-CB - SyncMyRide\1U5T-14G386-CB.tar.gz" + Environment.NewLine +
                @"Open1 = SyncMyRide\1u5t-14g386-cb.tar.gz" + Environment.NewLine + Environment.NewLine +
                "Options = AutoInstall" + Environment.NewLine;

            UpdateLog("INFO: Creating autoinstall.lst on root of selected USB Drive for " + cmbReleaseText);
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
            string autoinstalllst = "; Generated by CyanLabs Ford Sync Downloader - Autoinstall Mode" +
                                    cmbReleaseText + " " + cmbRegionText +
                                    Environment.NewLine + Environment.NewLine + "[SYNCGen3.0_ALL]" + Environment.NewLine;
            int i = 0;
            foreach (KeyValuePair<string, string> item in _dicIvsUs)
            {
                i++;
                autoinstalllst += string.Format(@"Item{0} = {1} - SyncMyRide\{2}\rOpen{0} = SyncMyRide\{2}\r", i, item.Value, item.Key)
                    .Replace(@"\r", Environment.NewLine);
            }

            autoinstalllst += Environment.NewLine + "Options = AutoInstall" + Environment.NewLine;
            UpdateLog("INFO: Creating autoinstall.lst on root of selected USB Drive for custom install");
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

                UpdateLog("INFO: Re-creating partition table as MBR and ExFat on selected USB drive");

                p.Start();
                p.StandardInput.WriteLine("SELECT DISK=" + drivenumber);
                p.StandardInput.WriteLine("CLEAN");
                p.StandardInput.WriteLine("CONVERT MBR");
                p.StandardInput.WriteLine("CREATE PARTITION PRIMARY");
                p.StandardInput.WriteLine("FORMAT FS=EXFAT LABEL=\"CYANLABS\" QUICK");
                p.StandardInput.WriteLine("ASSIGN");
                p.StandardInput.WriteLine("EXIT");

                UpdateLog("INFO: Creating and formatting the ExFat partition on selected USB drive");

                p.WaitForExit();
            }

            UpdateDriveInfo();
            if (_mode == "")
            {
                CreateAutoInstall();
            }
            else
            {
                if (_cmbSyncVersionText.Contains("DOWNGRADE1"))
                {
                    CreateDowngrade();
                }
                else
                {
                    _dicIvsUs.Add("1u5t-14g386-cb.tar.gz", "TOOL");
                    CreateReformat();
                }
            }

            foreach (KeyValuePair<string, string> item in _dicIvsUs) BeginInvoke(new Action(() => lstDownloadQueue.Items.Add(_downloadpath + item.Key)));
            BeginInvoke(new Action(() => lblTotalDownloadProgress.Text = strings.Form1_PrepareUsb_Total_Copy_Progress));
            Directory.CreateDirectory(_driveId + @"\SyncMyRide\");

            CopyFiles();
        }

        private CopyProgressResult CopyProgressHandler(long total, long transferred, long streamSize,
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

                    BeginInvoke(new Action(() =>
                    {
                        lblFileName.Text = string.Format(strings.Copying + @" {0} ({1}/{2})", _fileName, BytesToString(totalBytesCopiedInt64),
                            BytesToString(totalFileSizeInt64));
                        lblDownloadSize.Text =
                            $@"{remainingTime.Minutes:00}m {remainingTime.Seconds:00}s @ {BytesToString(bs)}/S";
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

            return _cancelcopy ? CopyProgressResult.ProgressCancel : CopyProgressResult.ProgressContinue;
        }


    }
}
