using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;
using Sync3Updater.Helpers;
using Sync3Updater.Properties;

namespace Sync3Updater
{
    public partial class FrmMain : Form
    {
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

        private HttpClientDownloadWithProgress _httpclient;
        private readonly Queue<string> _downloadfiles = new Queue<string>();
        private readonly Dictionary<string, string> _dicDriveList = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _dicIvsUs = new Dictionary<string, string>();
        private KeyValuePair<string, string> _kvpDriveList;
        private int _prevprogressPercentageInt, _totalcount = -1;
        private bool _downloadcomplete, _boolShowAllReleases, _downloadonly, _manualmode, _cancelcopy;
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

        private JsonReleases _jsonReleases;
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
            Application.Exit();
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

        private void FrmMain_Shown(object sender, EventArgs e)
        {
            lblConfiguration.Text =
                $"Current Version: {Settings.Default.CurrentSyncVersion} Region: {Settings.Default.CurrentSyncRegion} Navigation: {(Settings.Default.CurrentSyncNav ? "Yes" : "No")}" + Environment.NewLine + $"Download Path: { Settings.Default.DownloadPath}";
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
        }

        private void btnContinueNewVersion_Click(object sender, EventArgs e)
        {
            //check if old versions is x or x, new version is x or x, if nav etc.
            this.Size = new Size(620,650);
        }

        private void btnShowConfiguration_Click(object sender, EventArgs e)
        {
            _ = new FrmSetup() { Visible = true };
            this.Hide();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        { 
            if (Settings.Default.DownloadPath == "")
            {
                _downloadpath = KnownFolders.GetPath(KnownFolder.Downloads) + @"\Sync3Updater";
            }
            else
            {
                if (!Directory.Exists(Settings.Default.DownloadPath) && Settings.Default.DownloadPath != "")
                {
                    MessageBox.Show(strings.Download_Directory_Does_Not_Exist_Message, strings.Error, MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    _ = new FrmSetup() { Visible = true };
                }
                else
                {
                    _downloadpath = Settings.Default.DownloadPath;
                }
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

                txtDownloadPath.Text = _downloadpath;
                if (!Directory.Exists(_downloadpath)) Directory.CreateDirectory(_downloadpath);

                if (Directory.Exists(_downloadpath + @"\temp"))
                {
                    DirectoryInfo d = new DirectoryInfo(_downloadpath + "temp");
                    foreach (FileInfo file in d.GetFiles("*"))
                        FileSystem.MoveFile(_downloadpath + @"temp\" + file.Name, _downloadpath + file.Name, UIOption.AllDialogs);
                }

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

                if (Settings.Default.TOCAccepted2)
                {
                    tabctrlSteps.SelectedTab = tabStep2;
                    btnDisclaimerContinue.Enabled = chkDisclaimerConfirm.Checked;
                    btnStep2.Enabled = chkDisclaimerConfirm.Checked;
                }

                cmbDriveList.DisplayMember = "Value";
                cmbDriveList.ValueMember = "Key";
                RefreshUsb();
            }
            catch (WebException webex)
            {
                MessageBox.Show(strings.No_Internet_Access_Message + Environment.NewLine + webex.Message, strings.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }
    }
}
public partial class TabControlWithoutHeader : TabControl
{
    public TabControlWithoutHeader()
    {
        this.Multiline = true;
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 0x1328)
            m.Result = new IntPtr(1);
        else
            base.WndProc(ref m);
    }
}
