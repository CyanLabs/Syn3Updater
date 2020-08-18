﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;
using Sync3Updater.Helpers;
using Sync3Updater.Localization;
using Sync3Updater.Properties;

namespace Sync3Updater
{
    public partial class FrmSetup : Form
    {
        public FrmSetup()
        {
            InitializeComponent();
        }


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

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnWindowControls_MouseLeave(object sender, EventArgs e)
        {
            ((PictureBox)sender).Image = Resources.button;
        }
        #endregion

        private void btnSetupContinue_Click(object sender, EventArgs e)
        {
            Settings.Default.SetupCompleted = true;
            Settings.Default.Save();
            Application.Restart();
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
                        string.Format(
                            strings.Move_Existing_Files_Message + Environment.NewLine + Environment.NewLine + @"{0}" +
                            Environment.NewLine + strings.Move_Existing_Files_Message_2 + Environment.NewLine + @"{1}", Settings.Default.DownloadPath,
                            folderDownloads.SelectedPath)
                        , strings.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Information
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
                txtDownloadPath.Text = Settings.Default.DownloadPath;
                Settings.Default.DownloadPath = Settings.Default.DownloadPath;
                Settings.Default.Save();
            }
        }

        private void FrmSetup_Shown(object sender, EventArgs e)
        {
            txtDownloadPath.Text = Settings.Default.DownloadPath;
            if (!Settings.Default.SetupCompleted)
            {
                Settings.Default.DownloadPath = KnownFolders.GetPath(KnownFolder.Downloads) + @"\Sync3Updater\";
                
            }
        }
    }
}
