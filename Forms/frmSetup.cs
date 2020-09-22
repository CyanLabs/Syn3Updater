using System;
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
            try
            {
                Application.Exit();
            }
            catch (InvalidOperationException exception)
            {
                Console.WriteLine(exception);
            }
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

        private void lblWarning1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _ = new FrmRegion() { Visible = true };
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
        }

        private void chkForceAutoinstall_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkForceAutoinstall.Checked)
            {
                chkForceAutoinstall.Checked = false;
                return;
            }
            DialogResult dialog = MessageBox.Show(strings.FrmSetup_ForceAutoInstall, strings.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            chkForceAutoinstall.Checked = dialog == DialogResult.Yes;
        }
    }
}
