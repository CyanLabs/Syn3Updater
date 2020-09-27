﻿namespace Syn3Updater.Forms
{
    partial class FrmSetup
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmSetup));
            this.btnSetupContinue = new System.Windows.Forms.Button();
            this.grpDownload = new System.Windows.Forms.GroupBox();
            this.txtDownloadPath = new System.Windows.Forms.TextBox();
            this.btnChangeDownloadDirectory = new System.Windows.Forms.Button();
            this.grpExistingDetails = new System.Windows.Forms.GroupBox();
            this.chkForceAutoinstall = new System.Windows.Forms.CheckBox();
            this.txtCurrentSyncVersion = new System.Windows.Forms.MaskedTextBox();
            this.chkCurrentSyncNav = new System.Windows.Forms.CheckBox();
            this.lblCurrentSyncVersion = new System.Windows.Forms.Label();
            this.lblCurrentSyncRegion = new System.Windows.Forms.Label();
            this.cmbCurrentSyncRegion = new System.Windows.Forms.ComboBox();
            this.lblWarning1 = new System.Windows.Forms.LinkLabel();
            this.btnClose = new System.Windows.Forms.PictureBox();
            this.btnLogo = new System.Windows.Forms.PictureBox();
            this.panelTitleBar = new System.Windows.Forms.Panel();
            this.cmbLocale = new System.Windows.Forms.ComboBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.folderDownloads = new System.Windows.Forms.FolderBrowserDialog();
            this.grpDownload.SuspendLayout();
            this.grpExistingDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnLogo)).BeginInit();
            this.panelTitleBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSetupContinue
            // 
            resources.ApplyResources(this.btnSetupContinue, "btnSetupContinue");
            this.btnSetupContinue.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.btnSetupContinue.FlatAppearance.BorderSize = 0;
            this.btnSetupContinue.Name = "btnSetupContinue";
            this.btnSetupContinue.UseVisualStyleBackColor = false;
            this.btnSetupContinue.Click += new System.EventHandler(this.btnSetupContinue_Click);
            // 
            // grpDownload
            // 
            this.grpDownload.Controls.Add(this.txtDownloadPath);
            this.grpDownload.Controls.Add(this.btnChangeDownloadDirectory);
            this.grpDownload.ForeColor = System.Drawing.Color.White;
            resources.ApplyResources(this.grpDownload, "grpDownload");
            this.grpDownload.Name = "grpDownload";
            this.grpDownload.TabStop = false;
            // 
            // txtDownloadPath
            // 
            this.txtDownloadPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.txtDownloadPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDownloadPath.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Syn3Updater.Properties.Settings.Default, "DownloadPath", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtDownloadPath.ForeColor = System.Drawing.Color.White;
            resources.ApplyResources(this.txtDownloadPath, "txtDownloadPath");
            this.txtDownloadPath.Name = "txtDownloadPath";
            this.txtDownloadPath.ReadOnly = true;
            this.txtDownloadPath.Text = global::Syn3Updater.Properties.Settings.Default.DownloadPath;
            // 
            // btnChangeDownloadDirectory
            // 
            this.btnChangeDownloadDirectory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.btnChangeDownloadDirectory.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(this.btnChangeDownloadDirectory, "btnChangeDownloadDirectory");
            this.btnChangeDownloadDirectory.ForeColor = System.Drawing.Color.White;
            this.btnChangeDownloadDirectory.Name = "btnChangeDownloadDirectory";
            this.btnChangeDownloadDirectory.UseVisualStyleBackColor = false;
            this.btnChangeDownloadDirectory.Click += new System.EventHandler(this.btnChangeDownloadDirectory_Click);
            // 
            // grpExistingDetails
            // 
            this.grpExistingDetails.Controls.Add(this.chkForceAutoinstall);
            this.grpExistingDetails.Controls.Add(this.txtCurrentSyncVersion);
            this.grpExistingDetails.Controls.Add(this.chkCurrentSyncNav);
            this.grpExistingDetails.Controls.Add(this.lblCurrentSyncVersion);
            this.grpExistingDetails.Controls.Add(this.lblCurrentSyncRegion);
            this.grpExistingDetails.Controls.Add(this.cmbCurrentSyncRegion);
            this.grpExistingDetails.ForeColor = System.Drawing.Color.White;
            resources.ApplyResources(this.grpExistingDetails, "grpExistingDetails");
            this.grpExistingDetails.Name = "grpExistingDetails";
            this.grpExistingDetails.TabStop = false;
            // 
            // chkForceAutoinstall
            // 
            this.chkForceAutoinstall.Checked = global::Syn3Updater.Properties.Settings.Default.ForceAutoinstall;
            this.chkForceAutoinstall.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::Syn3Updater.Properties.Settings.Default, "ForceAutoinstall", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.chkForceAutoinstall.ForeColor = System.Drawing.Color.Red;
            resources.ApplyResources(this.chkForceAutoinstall, "chkForceAutoinstall");
            this.chkForceAutoinstall.Name = "chkForceAutoinstall";
            this.chkForceAutoinstall.UseVisualStyleBackColor = true;
            this.chkForceAutoinstall.CheckedChanged += new System.EventHandler(this.chkForceAutoinstall_CheckedChanged);
            // 
            // txtCurrentSyncVersion
            // 
            this.txtCurrentSyncVersion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.txtCurrentSyncVersion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCurrentSyncVersion.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Syn3Updater.Properties.Settings.Default, "CurrentSyncVersion", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            resources.ApplyResources(this.txtCurrentSyncVersion, "txtCurrentSyncVersion");
            this.txtCurrentSyncVersion.ForeColor = System.Drawing.Color.White;
            this.txtCurrentSyncVersion.Name = "txtCurrentSyncVersion";
            this.txtCurrentSyncVersion.TextMaskFormat = System.Windows.Forms.MaskFormat.ExcludePromptAndLiterals;
            // 
            // chkCurrentSyncNav
            // 
            this.chkCurrentSyncNav.Checked = global::Syn3Updater.Properties.Settings.Default.CurrentSyncNav;
            this.chkCurrentSyncNav.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCurrentSyncNav.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::Syn3Updater.Properties.Settings.Default, "CurrentSyncNav", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            resources.ApplyResources(this.chkCurrentSyncNav, "chkCurrentSyncNav");
            this.chkCurrentSyncNav.Name = "chkCurrentSyncNav";
            this.chkCurrentSyncNav.UseVisualStyleBackColor = true;
            // 
            // lblCurrentSyncVersion
            // 
            this.lblCurrentSyncVersion.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.lblCurrentSyncVersion, "lblCurrentSyncVersion");
            this.lblCurrentSyncVersion.Name = "lblCurrentSyncVersion";
            // 
            // lblCurrentSyncRegion
            // 
            this.lblCurrentSyncRegion.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.lblCurrentSyncRegion, "lblCurrentSyncRegion");
            this.lblCurrentSyncRegion.Name = "lblCurrentSyncRegion";
            // 
            // cmbCurrentSyncRegion
            // 
            this.cmbCurrentSyncRegion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.cmbCurrentSyncRegion.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Syn3Updater.Properties.Settings.Default, "CurrentSyncRegion", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.cmbCurrentSyncRegion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cmbCurrentSyncRegion, "cmbCurrentSyncRegion");
            this.cmbCurrentSyncRegion.ForeColor = System.Drawing.Color.White;
            this.cmbCurrentSyncRegion.FormattingEnabled = true;
            this.cmbCurrentSyncRegion.Items.AddRange(new object[] {
            resources.GetString("cmbCurrentSyncRegion.Items"),
            resources.GetString("cmbCurrentSyncRegion.Items1"),
            resources.GetString("cmbCurrentSyncRegion.Items2"),
            resources.GetString("cmbCurrentSyncRegion.Items3"),
            resources.GetString("cmbCurrentSyncRegion.Items4")});
            this.cmbCurrentSyncRegion.Name = "cmbCurrentSyncRegion";
            this.cmbCurrentSyncRegion.Text = global::Syn3Updater.Properties.Settings.Default.CurrentSyncRegion;
            this.cmbCurrentSyncRegion.SelectedIndexChanged += new System.EventHandler(this.cmbCurrentSyncRegion_SelectedIndexChanged);
            // 
            // lblWarning1
            // 
            this.lblWarning1.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.lblWarning1.BackColor = System.Drawing.Color.DarkRed;
            this.lblWarning1.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            resources.ApplyResources(this.lblWarning1, "lblWarning1");
            this.lblWarning1.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lblWarning1.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.lblWarning1.Name = "lblWarning1";
            this.lblWarning1.TabStop = true;
            this.lblWarning1.UseCompatibleTextRendering = true;
            this.lblWarning1.VisitedLinkColor = System.Drawing.Color.Silver;
            this.lblWarning1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblWarning1_LinkClicked);
            // 
            // btnClose
            // 
            resources.ApplyResources(this.btnClose, "btnClose");
            this.btnClose.Name = "btnClose";
            this.btnClose.TabStop = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.btnClose.MouseLeave += new System.EventHandler(this.btnWindowControls_MouseLeave);
            this.btnClose.MouseHover += new System.EventHandler(this.btnClose_MouseHover);
            // 
            // btnLogo
            // 
            this.btnLogo.BackColor = System.Drawing.Color.Transparent;
            this.btnLogo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLogo.Image = global::Syn3Updater.Properties.Resources.logo;
            resources.ApplyResources(this.btnLogo, "btnLogo");
            this.btnLogo.Name = "btnLogo";
            this.btnLogo.TabStop = false;
            this.btnLogo.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTitleBar_MouseDown);
            // 
            // panelTitleBar
            // 
            this.panelTitleBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.panelTitleBar.Controls.Add(this.cmbLocale);
            this.panelTitleBar.Controls.Add(this.btnLogo);
            this.panelTitleBar.Controls.Add(this.btnClose);
            this.panelTitleBar.Controls.Add(this.lblTitle);
            resources.ApplyResources(this.panelTitleBar, "panelTitleBar");
            this.panelTitleBar.Name = "panelTitleBar";
            this.panelTitleBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTitleBar_MouseDown);
            // 
            // cmbLocale
            // 
            this.cmbLocale.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.cmbLocale.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cmbLocale, "cmbLocale");
            this.cmbLocale.ForeColor = System.Drawing.Color.White;
            this.cmbLocale.FormattingEnabled = true;
            this.cmbLocale.Items.AddRange(new object[] {
            resources.GetString("cmbLocale.Items"),
            resources.GetString("cmbLocale.Items1"),
            resources.GetString("cmbLocale.Items2"),
            resources.GetString("cmbLocale.Items3"),
            resources.GetString("cmbLocale.Items4")});
            this.cmbLocale.Name = "cmbLocale";
            this.cmbLocale.SelectedIndexChanged += new System.EventHandler(this.cmbLocale_SelectedIndexChanged);
            // 
            // lblTitle
            // 
            resources.ApplyResources(this.lblTitle, "lblTitle");
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTitleBar_MouseDown);
            // 
            // folderDownloads
            // 
            this.folderDownloads.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // FrmSetup
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(40)))), ((int)(((byte)(46)))));
            this.Controls.Add(this.lblWarning1);
            this.Controls.Add(this.grpDownload);
            this.Controls.Add(this.grpExistingDetails);
            this.Controls.Add(this.btnSetupContinue);
            this.Controls.Add(this.panelTitleBar);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmSetup";
            this.TopMost = true;
            this.Shown += new System.EventHandler(this.FrmSetup_Shown);
            this.grpDownload.ResumeLayout(false);
            this.grpDownload.PerformLayout();
            this.grpExistingDetails.ResumeLayout(false);
            this.grpExistingDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnLogo)).EndInit();
            this.panelTitleBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnSetupContinue;
        private System.Windows.Forms.GroupBox grpDownload;
        private System.Windows.Forms.TextBox txtDownloadPath;
        private System.Windows.Forms.Button btnChangeDownloadDirectory;
        private System.Windows.Forms.GroupBox grpExistingDetails;
        private System.Windows.Forms.CheckBox chkCurrentSyncNav;
        private System.Windows.Forms.Label lblCurrentSyncVersion;
        private System.Windows.Forms.Label lblCurrentSyncRegion;
        private System.Windows.Forms.ComboBox cmbCurrentSyncRegion;
        private System.Windows.Forms.LinkLabel lblWarning1;
        private System.Windows.Forms.PictureBox btnClose;
        private System.Windows.Forms.PictureBox btnLogo;
        private System.Windows.Forms.Panel panelTitleBar;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.MaskedTextBox txtCurrentSyncVersion;
        private System.Windows.Forms.FolderBrowserDialog folderDownloads;
        private System.Windows.Forms.CheckBox chkForceAutoinstall;
        private System.Windows.Forms.ComboBox cmbLocale;
    }
}