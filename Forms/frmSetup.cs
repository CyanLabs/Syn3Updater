using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;
using Syn3Updater.Helpers;
using Syn3Updater.Properties;

namespace Syn3Updater.Forms
{
    public partial class FrmSetup : Form
    {
        public FrmSetup()
        {
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
            Application.Exit();
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
            _ = new frmRegion() { Visible = true };
        }

        private void btnChangeDownloadDirectory_Click(object sender, EventArgs e)
        {
            DialogResult result = folderDownloads.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (Directory.Exists(Settings.Default.DownloadPath))
                {
                    DialogResult dialogMovefiles = MessageBox.Show(
                        string.Format("Would you like to move existing files from\n\r\n\r{0}\n\rto\n\r{1}", Settings.Default.DownloadPath, folderDownloads.SelectedPath)
                        , "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information
                    );
                    try
                    {
                        if (dialogMovefiles == DialogResult.Yes) FileSystem.MoveDirectory(Settings.Default.DownloadPath, folderDownloads.SelectedPath, UIOption.AllDialogs);
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
    }
}
