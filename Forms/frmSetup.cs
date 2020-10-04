using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;
using Syn3Updater.Helpers;
using Syn3Updater.Localization;
using Syn3Updater.Properties;

namespace Syn3Updater.Forms
{
    public partial class FrmSetup : Form
    {
        public string Language = Settings.Default.Language;
        public FrmSetup()
        {
            if (!string.IsNullOrEmpty(Language)) Thread.CurrentThread.CurrentUICulture = new CultureInfo(Language);
            InitializeComponent();
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

        private void btnClose_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void btnWindowControls_MouseLeave(object sender, EventArgs e)
        {
            ((PictureBox)sender).Image = Resources.button;
        }

        private void btnSetupContinue_Click(object sender, EventArgs e)
        {
            Settings.Default.SetupCompleted = true;
            Settings.Default.CurrentSyncVersion = Convert.ToInt32(txtCurrentSyncVersion.Text);
            Settings.Default.Save();
            this.Close();
        }

        private void btnChangeDownloadDirectory_Click(object sender, EventArgs e)
        {
            DialogResult result = folderDownloads.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (Directory.Exists(Settings.Default.DownloadPath))
                {
                    DialogResult dialogMovefiles = MessageBox.Show(
                        string.Format(strings.FrmSetup_btnChangeDownloadDirectory_ChangeDirectory, Settings.Default.DownloadPath, folderDownloads.SelectedPath)
                        , strings.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Information
                    );
                    try
                    {
                        if (dialogMovefiles == DialogResult.Yes) FileSystem.MoveDirectory(txtDownloadPath.Text, folderDownloads.SelectedPath, UIOption.AllDialogs);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
                Settings.Default.DownloadPath = folderDownloads.SelectedPath + @"\";
                Settings.Default.Save();
            }
        }

        private void FrmSetup_Shown(object sender, EventArgs e)
        {
            if (!Settings.Default.SetupCompleted)
                Settings.Default.DownloadPath = KnownFolders.GetPath(KnownFolder.Downloads) + @"\Syn3Updater\";
            txtDownloadPath.Text = Settings.Default.DownloadPath;
            this.cmbLocale.SelectedIndexChanged -= new EventHandler(cmbLocale_SelectedIndexChanged);
            cmbLocale.Text = string.IsNullOrEmpty(Settings.Default.Language) ? Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToUpper() : Settings.Default.Language;
            this.cmbLocale.SelectedIndexChanged += new EventHandler(cmbLocale_SelectedIndexChanged);
            if (!string.IsNullOrEmpty(cmbCurrentSyncRegion.Text))
            {
                btnSetupContinue.Enabled = true;
            }
            if (!string.IsNullOrEmpty(Settings.Default.ForcedInstallMode))
                cmbOverride.Text = "automatic";
        }

        private void ChangeLanguage(string lang) //A function called to change the language
        {
            foreach (Control c in this.Controls)
            {
                ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmMain));
                resources.ApplyResources(c, c.Name, new CultureInfo(lang));
            }
        }

        private void cmbLocale_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Default.Language = cmbLocale.Text;
            Settings.Default.Save();

            MessageBox.Show(strings.FrmMain_cmbLocale_Restart, strings.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Application.ExitThread();
        }

        private void cmbCurrentSyncRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(cmbCurrentSyncRegion.Text))
            {
                btnSetupContinue.Enabled = true;
            }
        }

        private void lblWarning1_Click(object sender, EventArgs e)
        {
            this.TopMost = false;
            _ = new FrmRegion() { Visible = true };
        }

        private void cmbOverride_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbOverride.SelectedIndex != 0) MessageBox.Show(strings.FrmSetup_ForceAutoInstall, strings.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btnPurchaseActivate_Click(object sender, EventArgs e)
        {
            MessageBox.Show(strings.FrmSetup_LicenseKeyActivated, @"Syn3 Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void txtReleaseKey_TextChanged(object sender, EventArgs e)
        {
            btnPurchaseActivate.Enabled = !string.IsNullOrEmpty(txtReleaseKey.Text);
        }
    }
}
