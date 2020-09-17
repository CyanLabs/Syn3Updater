namespace Syn3Updater
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
            this.txtCurrentSyncVersion = new System.Windows.Forms.MaskedTextBox();
            this.chkCurrentSyncNav = new System.Windows.Forms.CheckBox();
            this.lblCurrentSyncVersion = new System.Windows.Forms.Label();
            this.lblCurrentSyncRegion = new System.Windows.Forms.Label();
            this.cmbCurrentSyncRegion = new System.Windows.Forms.ComboBox();
            this.lblWarning1 = new System.Windows.Forms.LinkLabel();
            this.btnClose = new System.Windows.Forms.PictureBox();
            this.btnLogo = new System.Windows.Forms.PictureBox();
            this.panelTitleBar = new System.Windows.Forms.Panel();
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
            this.btnSetupContinue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSetupContinue.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.btnSetupContinue.FlatAppearance.BorderSize = 0;
            this.btnSetupContinue.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSetupContinue.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.btnSetupContinue.Location = new System.Drawing.Point(465, 221);
            this.btnSetupContinue.Name = "btnSetupContinue";
            this.btnSetupContinue.Size = new System.Drawing.Size(133, 52);
            this.btnSetupContinue.TabIndex = 17;
            this.btnSetupContinue.Text = "Save Configuration";
            this.btnSetupContinue.UseVisualStyleBackColor = false;
            this.btnSetupContinue.Click += new System.EventHandler(this.btnSetupContinue_Click);
            // 
            // grpDownload
            // 
            this.grpDownload.Controls.Add(this.txtDownloadPath);
            this.grpDownload.Controls.Add(this.btnChangeDownloadDirectory);
            this.grpDownload.ForeColor = System.Drawing.Color.White;
            this.grpDownload.Location = new System.Drawing.Point(9, 221);
            this.grpDownload.Name = "grpDownload";
            this.grpDownload.Size = new System.Drawing.Size(435, 52);
            this.grpDownload.TabIndex = 30;
            this.grpDownload.TabStop = false;
            this.grpDownload.Text = "Optional - Download Location";
            // 
            // txtDownloadPath
            // 
            this.txtDownloadPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.txtDownloadPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDownloadPath.ForeColor = System.Drawing.Color.White;
            this.txtDownloadPath.Location = new System.Drawing.Point(9, 19);
            this.txtDownloadPath.Name = "txtDownloadPath";
            this.txtDownloadPath.ReadOnly = true;
            this.txtDownloadPath.Size = new System.Drawing.Size(382, 22);
            this.txtDownloadPath.TabIndex = 2;
            // 
            // btnChangeDownloadDirectory
            // 
            this.btnChangeDownloadDirectory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.btnChangeDownloadDirectory.FlatAppearance.BorderSize = 0;
            this.btnChangeDownloadDirectory.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnChangeDownloadDirectory.ForeColor = System.Drawing.Color.White;
            this.btnChangeDownloadDirectory.Location = new System.Drawing.Point(392, 19);
            this.btnChangeDownloadDirectory.Name = "btnChangeDownloadDirectory";
            this.btnChangeDownloadDirectory.Size = new System.Drawing.Size(29, 23);
            this.btnChangeDownloadDirectory.TabIndex = 25;
            this.btnChangeDownloadDirectory.Text = "...";
            this.btnChangeDownloadDirectory.UseVisualStyleBackColor = false;
            this.btnChangeDownloadDirectory.Click += new System.EventHandler(this.btnChangeDownloadDirectory_Click);
            // 
            // grpExistingDetails
            // 
            this.grpExistingDetails.Controls.Add(this.txtCurrentSyncVersion);
            this.grpExistingDetails.Controls.Add(this.chkCurrentSyncNav);
            this.grpExistingDetails.Controls.Add(this.lblCurrentSyncVersion);
            this.grpExistingDetails.Controls.Add(this.lblCurrentSyncRegion);
            this.grpExistingDetails.Controls.Add(this.cmbCurrentSyncRegion);
            this.grpExistingDetails.ForeColor = System.Drawing.Color.White;
            this.grpExistingDetails.Location = new System.Drawing.Point(9, 158);
            this.grpExistingDetails.Name = "grpExistingDetails";
            this.grpExistingDetails.Size = new System.Drawing.Size(589, 52);
            this.grpExistingDetails.TabIndex = 29;
            this.grpExistingDetails.TabStop = false;
            this.grpExistingDetails.Text = "Current Version Information";
            // 
            // txtCurrentSyncVersion
            // 
            this.txtCurrentSyncVersion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.txtCurrentSyncVersion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCurrentSyncVersion.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Syn3Updater.Properties.Settings.Default, "CurrentSyncVersion", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtCurrentSyncVersion.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCurrentSyncVersion.ForeColor = System.Drawing.Color.White;
            this.txtCurrentSyncVersion.Location = new System.Drawing.Point(152, 18);
            this.txtCurrentSyncVersion.Mask = "#.#.#####";
            this.txtCurrentSyncVersion.Name = "txtCurrentSyncVersion";
            this.txtCurrentSyncVersion.Size = new System.Drawing.Size(69, 25);
            this.txtCurrentSyncVersion.TabIndex = 26;
            this.txtCurrentSyncVersion.Text = "3420136";
            this.txtCurrentSyncVersion.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtCurrentSyncVersion.TextMaskFormat = System.Windows.Forms.MaskFormat.ExcludePromptAndLiterals;
            // 
            // chkCurrentSyncNav
            // 
            this.chkCurrentSyncNav.Checked = global::Syn3Updater.Properties.Settings.Default.CurrentSyncNav;
            this.chkCurrentSyncNav.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCurrentSyncNav.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::Syn3Updater.Properties.Settings.Default, "CurrentSyncNav", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.chkCurrentSyncNav.Location = new System.Drawing.Point(479, 22);
            this.chkCurrentSyncNav.Name = "chkCurrentSyncNav";
            this.chkCurrentSyncNav.Size = new System.Drawing.Size(104, 17);
            this.chkCurrentSyncNav.TabIndex = 25;
            this.chkCurrentSyncNav.Text = "Navigation?";
            this.chkCurrentSyncNav.UseVisualStyleBackColor = true;
            // 
            // lblCurrentSyncVersion
            // 
            this.lblCurrentSyncVersion.BackColor = System.Drawing.Color.Transparent;
            this.lblCurrentSyncVersion.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblCurrentSyncVersion.Location = new System.Drawing.Point(9, 23);
            this.lblCurrentSyncVersion.Name = "lblCurrentSyncVersion";
            this.lblCurrentSyncVersion.Size = new System.Drawing.Size(147, 13);
            this.lblCurrentSyncVersion.TabIndex = 17;
            this.lblCurrentSyncVersion.Text = "Full Sync Version: ";
            this.lblCurrentSyncVersion.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblCurrentSyncRegion
            // 
            this.lblCurrentSyncRegion.BackColor = System.Drawing.Color.Transparent;
            this.lblCurrentSyncRegion.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblCurrentSyncRegion.Location = new System.Drawing.Point(276, 23);
            this.lblCurrentSyncRegion.Name = "lblCurrentSyncRegion";
            this.lblCurrentSyncRegion.Size = new System.Drawing.Size(85, 13);
            this.lblCurrentSyncRegion.TabIndex = 24;
            this.lblCurrentSyncRegion.Text = "APIM Region:";
            this.lblCurrentSyncRegion.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cmbCurrentSyncRegion
            // 
            this.cmbCurrentSyncRegion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.cmbCurrentSyncRegion.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Syn3Updater.Properties.Settings.Default, "CurrentSyncRegion", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.cmbCurrentSyncRegion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCurrentSyncRegion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbCurrentSyncRegion.ForeColor = System.Drawing.Color.White;
            this.cmbCurrentSyncRegion.FormattingEnabled = true;
            this.cmbCurrentSyncRegion.Items.AddRange(new object[] {
            "CN",
            "EU",
            "NA",
            "ANZ",
            "ROW"});
            this.cmbCurrentSyncRegion.Location = new System.Drawing.Point(366, 20);
            this.cmbCurrentSyncRegion.Name = "cmbCurrentSyncRegion";
            this.cmbCurrentSyncRegion.Size = new System.Drawing.Size(58, 21);
            this.cmbCurrentSyncRegion.TabIndex = 20;
            this.cmbCurrentSyncRegion.Text = global::Syn3Updater.Properties.Settings.Default.CurrentSyncRegion;
            // 
            // lblWarning1
            // 
            this.lblWarning1.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.lblWarning1.BackColor = System.Drawing.Color.DarkRed;
            this.lblWarning1.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.lblWarning1.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblWarning1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWarning1.LinkArea = new System.Windows.Forms.LinkArea(157, 22);
            this.lblWarning1.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lblWarning1.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.lblWarning1.Location = new System.Drawing.Point(0, 49);
            this.lblWarning1.Name = "lblWarning1";
            this.lblWarning1.Padding = new System.Windows.Forms.Padding(3);
            this.lblWarning1.Size = new System.Drawing.Size(608, 97);
            this.lblWarning1.TabIndex = 31;
            this.lblWarning1.TabStop = true;
            this.lblWarning1.Text = resources.GetString("lblWarning1.Text");
            this.lblWarning1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblWarning1.UseCompatibleTextRendering = true;
            this.lblWarning1.VisitedLinkColor = System.Drawing.Color.Silver;
            this.lblWarning1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblWarning1_LinkClicked);
            // 
            // btnClose
            // 
            this.btnClose.Image = ((System.Drawing.Image)(resources.GetObject("btnClose.Image")));
            this.btnClose.Location = new System.Drawing.Point(592, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(16, 17);
            this.btnClose.TabIndex = 2;
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
            this.btnLogo.Location = new System.Drawing.Point(4, 2);
            this.btnLogo.Name = "btnLogo";
            this.btnLogo.Size = new System.Drawing.Size(191, 46);
            this.btnLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.btnLogo.TabIndex = 7;
            this.btnLogo.TabStop = false;
            this.btnLogo.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTitleBar_MouseDown);
            // 
            // panelTitleBar
            // 
            this.panelTitleBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.panelTitleBar.Controls.Add(this.btnLogo);
            this.panelTitleBar.Controls.Add(this.btnClose);
            this.panelTitleBar.Controls.Add(this.lblTitle);
            this.panelTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTitleBar.Location = new System.Drawing.Point(0, 0);
            this.panelTitleBar.Name = "panelTitleBar";
            this.panelTitleBar.Size = new System.Drawing.Size(608, 49);
            this.panelTitleBar.TabIndex = 16;
            this.panelTitleBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTitleBar_MouseDown);
            // 
            // lblTitle
            // 
            this.lblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(193, 8);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(396, 30);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "SYNC 3 UPDATER - CONFIGURATION";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTitleBar_MouseDown);
            // 
            // folderDownloads
            // 
            this.folderDownloads.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // FrmSetup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(40)))), ((int)(((byte)(46)))));
            this.ClientSize = new System.Drawing.Size(608, 286);
            this.Controls.Add(this.lblWarning1);
            this.Controls.Add(this.grpDownload);
            this.Controls.Add(this.grpExistingDetails);
            this.Controls.Add(this.btnSetupContinue);
            this.Controls.Add(this.panelTitleBar);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmSetup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cyanlabs Ford Sync Update Downloader";
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
    }
}