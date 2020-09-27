namespace Syn3Updater.Forms
{
    partial class FrmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.lblTitle = new System.Windows.Forms.Label();
            this.panelTitleBar = new System.Windows.Forms.Panel();
            this.lblVersion = new System.Windows.Forms.Label();
            this.btnHelp = new System.Windows.Forms.Button();
            this.btnMinimize = new System.Windows.Forms.PictureBox();
            this.btnLogo = new System.Windows.Forms.PictureBox();
            this.btnClose = new System.Windows.Forms.PictureBox();
            this.grpUSB = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbDriveList = new System.Windows.Forms.ComboBox();
            this.btnRefreshUSB = new System.Windows.Forms.Button();
            this.lblDriveInfo = new System.Windows.Forms.Label();
            this.grpNewVersion = new System.Windows.Forms.GroupBox();
            this.txtReleaseNotes = new System.Windows.Forms.TextBox();
            this.lblMapVersion1 = new System.Windows.Forms.Label();
            this.cmbMapVersion = new System.Windows.Forms.ComboBox();
            this.cmbRelease = new System.Windows.Forms.ComboBox();
            this.cmbRegion = new System.Windows.Forms.ComboBox();
            this.lblRelease = new System.Windows.Forms.Label();
            this.lblRegionInfo = new System.Windows.Forms.Label();
            this.lblRegion = new System.Windows.Forms.Label();
            this.btnContinue = new System.Windows.Forms.Button();
            this.btnShowConfiguration = new System.Windows.Forms.Button();
            this.lblConfiguration = new System.Windows.Forms.Label();
            this.tabControl1 = new Syn3Updater.Controls.TabControlWithoutHeader();
            this.tabAutoInstall = new System.Windows.Forms.TabPage();
            this.lblMode1 = new System.Windows.Forms.Label();
            this.btnAutoinstall = new System.Windows.Forms.Button();
            this.lblManualWarning = new System.Windows.Forms.Label();
            this.lstIVSU = new System.Windows.Forms.ListView();
            this.lvIVSUsType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvIVSUsName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvIVSUsVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvIVSUsURL = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvIVSUsMD5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabStatus = new System.Windows.Forms.TabPage();
            this.lblMode2 = new System.Windows.Forms.Label();
            this.btnShowLog = new System.Windows.Forms.Button();
            this.barTotalDownloadProgress = new Syn3Updater.Controls.NewProgressBar();
            this.barDownloadProgress = new Syn3Updater.Controls.NewProgressBar();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblCurrentTask = new System.Windows.Forms.Label();
            this.lstDownloadQueue = new System.Windows.Forms.ListBox();
            this.lblDownloadSize = new System.Windows.Forms.Label();
            this.lblFileName = new System.Windows.Forms.Label();
            this.lblDownloadQueue = new System.Windows.Forms.Label();
            this.lblFilesRemaining = new System.Windows.Forms.Label();
            this.panelTitleBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnMinimize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).BeginInit();
            this.grpUSB.SuspendLayout();
            this.grpNewVersion.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabAutoInstall.SuspendLayout();
            this.tabStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            resources.ApplyResources(this.lblTitle, "lblTitle");
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTitleBar_MouseDown);
            // 
            // panelTitleBar
            // 
            resources.ApplyResources(this.panelTitleBar, "panelTitleBar");
            this.panelTitleBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.panelTitleBar.Controls.Add(this.lblVersion);
            this.panelTitleBar.Controls.Add(this.btnHelp);
            this.panelTitleBar.Controls.Add(this.btnMinimize);
            this.panelTitleBar.Controls.Add(this.btnLogo);
            this.panelTitleBar.Controls.Add(this.btnClose);
            this.panelTitleBar.Controls.Add(this.lblTitle);
            this.panelTitleBar.Name = "panelTitleBar";
            this.panelTitleBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTitleBar_MouseDown);
            // 
            // lblVersion
            // 
            resources.ApplyResources(this.lblVersion, "lblVersion");
            this.lblVersion.ForeColor = System.Drawing.Color.White;
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTitleBar_MouseDown);
            // 
            // btnHelp
            // 
            resources.ApplyResources(this.btnHelp, "btnHelp");
            this.btnHelp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnHelp.FlatAppearance.BorderSize = 0;
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.UseVisualStyleBackColor = false;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // btnMinimize
            // 
            resources.ApplyResources(this.btnMinimize, "btnMinimize");
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.TabStop = false;
            this.btnMinimize.Click += new System.EventHandler(this.btnMinimize_Click);
            this.btnMinimize.MouseLeave += new System.EventHandler(this.btnWindowControls_MouseLeave);
            this.btnMinimize.MouseHover += new System.EventHandler(this.btnMinimize_MouseHover);
            // 
            // btnLogo
            // 
            resources.ApplyResources(this.btnLogo, "btnLogo");
            this.btnLogo.BackColor = System.Drawing.Color.Transparent;
            this.btnLogo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLogo.Image = global::Syn3Updater.Properties.Resources.logo;
            this.btnLogo.Name = "btnLogo";
            this.btnLogo.TabStop = false;
            this.btnLogo.Click += new System.EventHandler(this.btnLogo_Click);
            this.btnLogo.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTitleBar_MouseDown);
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
            // grpUSB
            // 
            resources.ApplyResources(this.grpUSB, "grpUSB");
            this.grpUSB.Controls.Add(this.label1);
            this.grpUSB.Controls.Add(this.cmbDriveList);
            this.grpUSB.Controls.Add(this.btnRefreshUSB);
            this.grpUSB.Controls.Add(this.lblDriveInfo);
            this.grpUSB.ForeColor = System.Drawing.Color.White;
            this.grpUSB.Name = "grpUSB";
            this.grpUSB.TabStop = false;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.Name = "label1";
            // 
            // cmbDriveList
            // 
            resources.ApplyResources(this.cmbDriveList, "cmbDriveList");
            this.cmbDriveList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.cmbDriveList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDriveList.ForeColor = System.Drawing.Color.White;
            this.cmbDriveList.FormattingEnabled = true;
            this.cmbDriveList.Name = "cmbDriveList";
            this.cmbDriveList.SelectedIndexChanged += new System.EventHandler(this.cmbDriveList_SelectedIndexChanged);
            // 
            // btnRefreshUSB
            // 
            resources.ApplyResources(this.btnRefreshUSB, "btnRefreshUSB");
            this.btnRefreshUSB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.btnRefreshUSB.FlatAppearance.BorderSize = 0;
            this.btnRefreshUSB.Name = "btnRefreshUSB";
            this.btnRefreshUSB.UseVisualStyleBackColor = false;
            this.btnRefreshUSB.Click += new System.EventHandler(this.btnRefreshUSB_Click);
            // 
            // lblDriveInfo
            // 
            resources.ApplyResources(this.lblDriveInfo, "lblDriveInfo");
            this.lblDriveInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.lblDriveInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblDriveInfo.Name = "lblDriveInfo";
            // 
            // grpNewVersion
            // 
            resources.ApplyResources(this.grpNewVersion, "grpNewVersion");
            this.grpNewVersion.Controls.Add(this.txtReleaseNotes);
            this.grpNewVersion.Controls.Add(this.lblMapVersion1);
            this.grpNewVersion.Controls.Add(this.cmbMapVersion);
            this.grpNewVersion.Controls.Add(this.cmbRelease);
            this.grpNewVersion.Controls.Add(this.cmbRegion);
            this.grpNewVersion.Controls.Add(this.lblRelease);
            this.grpNewVersion.Controls.Add(this.lblRegionInfo);
            this.grpNewVersion.Controls.Add(this.lblRegion);
            this.grpNewVersion.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.grpNewVersion.ForeColor = System.Drawing.Color.White;
            this.grpNewVersion.Name = "grpNewVersion";
            this.grpNewVersion.TabStop = false;
            // 
            // txtReleaseNotes
            // 
            resources.ApplyResources(this.txtReleaseNotes, "txtReleaseNotes");
            this.txtReleaseNotes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.txtReleaseNotes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtReleaseNotes.ForeColor = System.Drawing.Color.White;
            this.txtReleaseNotes.Name = "txtReleaseNotes";
            this.txtReleaseNotes.ReadOnly = true;
            // 
            // lblMapVersion1
            // 
            resources.ApplyResources(this.lblMapVersion1, "lblMapVersion1");
            this.lblMapVersion1.Name = "lblMapVersion1";
            // 
            // cmbMapVersion
            // 
            resources.ApplyResources(this.cmbMapVersion, "cmbMapVersion");
            this.cmbMapVersion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.cmbMapVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMapVersion.ForeColor = System.Drawing.Color.White;
            this.cmbMapVersion.FormattingEnabled = true;
            this.cmbMapVersion.Name = "cmbMapVersion";
            this.cmbMapVersion.SelectedIndexChanged += new System.EventHandler(this.cmbMapVersion_SelectedIndexChanged);
            // 
            // cmbRelease
            // 
            resources.ApplyResources(this.cmbRelease, "cmbRelease");
            this.cmbRelease.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.cmbRelease.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRelease.ForeColor = System.Drawing.Color.White;
            this.cmbRelease.FormattingEnabled = true;
            this.cmbRelease.Name = "cmbRelease";
            this.cmbRelease.SelectedIndexChanged += new System.EventHandler(this.cmbRelease_SelectedIndexChanged);
            // 
            // cmbRegion
            // 
            resources.ApplyResources(this.cmbRegion, "cmbRegion");
            this.cmbRegion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.cmbRegion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRegion.ForeColor = System.Drawing.Color.White;
            this.cmbRegion.FormattingEnabled = true;
            this.cmbRegion.Items.AddRange(new object[] {
            resources.GetString("cmbRegion.Items"),
            resources.GetString("cmbRegion.Items1"),
            resources.GetString("cmbRegion.Items2"),
            resources.GetString("cmbRegion.Items3"),
            resources.GetString("cmbRegion.Items4")});
            this.cmbRegion.Name = "cmbRegion";
            this.cmbRegion.SelectedIndexChanged += new System.EventHandler(this.cmbRegion_SelectedIndexChanged);
            // 
            // lblRelease
            // 
            resources.ApplyResources(this.lblRelease, "lblRelease");
            this.lblRelease.Name = "lblRelease";
            // 
            // lblRegionInfo
            // 
            resources.ApplyResources(this.lblRegionInfo, "lblRegionInfo");
            this.lblRegionInfo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblRegionInfo.Name = "lblRegionInfo";
            this.lblRegionInfo.Click += new System.EventHandler(this.lblRegionInfo_Click);
            // 
            // lblRegion
            // 
            resources.ApplyResources(this.lblRegion, "lblRegion");
            this.lblRegion.BackColor = System.Drawing.Color.Transparent;
            this.lblRegion.Name = "lblRegion";
            // 
            // btnContinue
            // 
            resources.ApplyResources(this.btnContinue, "btnContinue");
            this.btnContinue.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.btnContinue.FlatAppearance.BorderSize = 0;
            this.btnContinue.Name = "btnContinue";
            this.btnContinue.UseVisualStyleBackColor = false;
            this.btnContinue.Click += new System.EventHandler(this.btnContinue_Click);
            // 
            // btnShowConfiguration
            // 
            resources.ApplyResources(this.btnShowConfiguration, "btnShowConfiguration");
            this.btnShowConfiguration.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.btnShowConfiguration.FlatAppearance.BorderSize = 0;
            this.btnShowConfiguration.Name = "btnShowConfiguration";
            this.btnShowConfiguration.UseVisualStyleBackColor = false;
            this.btnShowConfiguration.Click += new System.EventHandler(this.btnShowConfiguration_Click);
            // 
            // lblConfiguration
            // 
            resources.ApplyResources(this.lblConfiguration, "lblConfiguration");
            this.lblConfiguration.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.lblConfiguration.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblConfiguration.Name = "lblConfiguration";
            // 
            // tabControl1
            // 
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Controls.Add(this.tabAutoInstall);
            this.tabControl1.Controls.Add(this.tabStatus);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            // 
            // tabAutoInstall
            // 
            resources.ApplyResources(this.tabAutoInstall, "tabAutoInstall");
            this.tabAutoInstall.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.tabAutoInstall.Controls.Add(this.lblMode1);
            this.tabAutoInstall.Controls.Add(this.btnAutoinstall);
            this.tabAutoInstall.Controls.Add(this.lblManualWarning);
            this.tabAutoInstall.Controls.Add(this.lstIVSU);
            this.tabAutoInstall.Name = "tabAutoInstall";
            // 
            // lblMode1
            // 
            resources.ApplyResources(this.lblMode1, "lblMode1");
            this.lblMode1.Name = "lblMode1";
            this.lblMode1.TextChanged += new System.EventHandler(this.lblMode1_TextChanged);
            // 
            // btnAutoinstall
            // 
            resources.ApplyResources(this.btnAutoinstall, "btnAutoinstall");
            this.btnAutoinstall.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.btnAutoinstall.FlatAppearance.BorderSize = 0;
            this.btnAutoinstall.Name = "btnAutoinstall";
            this.btnAutoinstall.UseVisualStyleBackColor = false;
            this.btnAutoinstall.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // lblManualWarning
            // 
            resources.ApplyResources(this.lblManualWarning, "lblManualWarning");
            this.lblManualWarning.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(40)))), ((int)(((byte)(46)))));
            this.lblManualWarning.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblManualWarning.Name = "lblManualWarning";
            // 
            // lstIVSU
            // 
            resources.ApplyResources(this.lstIVSU, "lstIVSU");
            this.lstIVSU.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.lstIVSU.CheckBoxes = true;
            this.lstIVSU.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.lvIVSUsType,
            this.lvIVSUsName,
            this.lvIVSUsVersion,
            this.lvIVSUsURL,
            this.lvIVSUsMD5});
            this.lstIVSU.ForeColor = System.Drawing.Color.White;
            this.lstIVSU.FullRowSelect = true;
            this.lstIVSU.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstIVSU.HideSelection = false;
            this.lstIVSU.Name = "lstIVSU";
            this.lstIVSU.UseCompatibleStateImageBehavior = false;
            this.lstIVSU.View = System.Windows.Forms.View.Details;
            // 
            // lvIVSUsType
            // 
            resources.ApplyResources(this.lvIVSUsType, "lvIVSUsType");
            // 
            // lvIVSUsName
            // 
            resources.ApplyResources(this.lvIVSUsName, "lvIVSUsName");
            // 
            // lvIVSUsVersion
            // 
            resources.ApplyResources(this.lvIVSUsVersion, "lvIVSUsVersion");
            // 
            // lvIVSUsURL
            // 
            resources.ApplyResources(this.lvIVSUsURL, "lvIVSUsURL");
            // 
            // lvIVSUsMD5
            // 
            resources.ApplyResources(this.lvIVSUsMD5, "lvIVSUsMD5");
            // 
            // tabStatus
            // 
            resources.ApplyResources(this.tabStatus, "tabStatus");
            this.tabStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.tabStatus.Controls.Add(this.lblMode2);
            this.tabStatus.Controls.Add(this.btnShowLog);
            this.tabStatus.Controls.Add(this.barTotalDownloadProgress);
            this.tabStatus.Controls.Add(this.barDownloadProgress);
            this.tabStatus.Controls.Add(this.btnCancel);
            this.tabStatus.Controls.Add(this.lblCurrentTask);
            this.tabStatus.Controls.Add(this.lstDownloadQueue);
            this.tabStatus.Controls.Add(this.lblDownloadSize);
            this.tabStatus.Controls.Add(this.lblFileName);
            this.tabStatus.Controls.Add(this.lblDownloadQueue);
            this.tabStatus.Controls.Add(this.lblFilesRemaining);
            this.tabStatus.Name = "tabStatus";
            // 
            // lblMode2
            // 
            resources.ApplyResources(this.lblMode2, "lblMode2");
            this.lblMode2.Name = "lblMode2";
            // 
            // btnShowLog
            // 
            resources.ApplyResources(this.btnShowLog, "btnShowLog");
            this.btnShowLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.btnShowLog.FlatAppearance.BorderSize = 0;
            this.btnShowLog.Name = "btnShowLog";
            this.btnShowLog.UseVisualStyleBackColor = false;
            this.btnShowLog.Click += new System.EventHandler(this.btnShowLog_Click);
            // 
            // barTotalDownloadProgress
            // 
            resources.ApplyResources(this.barTotalDownloadProgress, "barTotalDownloadProgress");
            this.barTotalDownloadProgress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.barTotalDownloadProgress.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.barTotalDownloadProgress.Name = "barTotalDownloadProgress";
            // 
            // barDownloadProgress
            // 
            resources.ApplyResources(this.barDownloadProgress, "barDownloadProgress");
            this.barDownloadProgress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.barDownloadProgress.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.barDownloadProgress.Name = "barDownloadProgress";
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.BackColor = System.Drawing.Color.Maroon;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblCurrentTask
            // 
            resources.ApplyResources(this.lblCurrentTask, "lblCurrentTask");
            this.lblCurrentTask.Name = "lblCurrentTask";
            // 
            // lstDownloadQueue
            // 
            resources.ApplyResources(this.lstDownloadQueue, "lstDownloadQueue");
            this.lstDownloadQueue.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.lstDownloadQueue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstDownloadQueue.ForeColor = System.Drawing.Color.White;
            this.lstDownloadQueue.FormattingEnabled = true;
            this.lstDownloadQueue.Name = "lstDownloadQueue";
            this.lstDownloadQueue.SelectionMode = System.Windows.Forms.SelectionMode.None;
            // 
            // lblDownloadSize
            // 
            resources.ApplyResources(this.lblDownloadSize, "lblDownloadSize");
            this.lblDownloadSize.Name = "lblDownloadSize";
            // 
            // lblFileName
            // 
            resources.ApplyResources(this.lblFileName, "lblFileName");
            this.lblFileName.Name = "lblFileName";
            // 
            // lblDownloadQueue
            // 
            resources.ApplyResources(this.lblDownloadQueue, "lblDownloadQueue");
            this.lblDownloadQueue.Name = "lblDownloadQueue";
            // 
            // lblFilesRemaining
            // 
            resources.ApplyResources(this.lblFilesRemaining, "lblFilesRemaining");
            this.lblFilesRemaining.Name = "lblFilesRemaining";
            // 
            // FrmMain
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(40)))), ((int)(((byte)(46)))));
            this.Controls.Add(this.lblConfiguration);
            this.Controls.Add(this.btnShowConfiguration);
            this.Controls.Add(this.btnContinue);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.grpUSB);
            this.Controls.Add(this.grpNewVersion);
            this.Controls.Add(this.panelTitleBar);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmMain";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmMain_FormClosing);
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.Shown += new System.EventHandler(this.FrmMain_Shown);
            this.panelTitleBar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.btnMinimize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).EndInit();
            this.grpUSB.ResumeLayout(false);
            this.grpNewVersion.ResumeLayout(false);
            this.grpNewVersion.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabAutoInstall.ResumeLayout(false);
            this.tabAutoInstall.PerformLayout();
            this.tabStatus.ResumeLayout(false);
            this.tabStatus.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox btnLogo;
        private System.Windows.Forms.PictureBox btnClose;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel panelTitleBar;
        private System.Windows.Forms.PictureBox btnMinimize;
        private System.Windows.Forms.GroupBox grpUSB;
        private System.Windows.Forms.ComboBox cmbDriveList;
        private System.Windows.Forms.Button btnRefreshUSB;
        private System.Windows.Forms.Label lblDriveInfo;
        private System.Windows.Forms.GroupBox grpNewVersion;
        private System.Windows.Forms.TextBox txtReleaseNotes;
        private System.Windows.Forms.Label lblMapVersion1;
        private System.Windows.Forms.ComboBox cmbMapVersion;
        private System.Windows.Forms.ComboBox cmbRelease;
        private System.Windows.Forms.ComboBox cmbRegion;
        private System.Windows.Forms.Label lblRegionInfo;
        private System.Windows.Forms.Label lblRelease;
        private System.Windows.Forms.Label lblRegion;
        private System.Windows.Forms.Label label1;
        private Syn3Updater.Controls.TabControlWithoutHeader tabControl1;
        private System.Windows.Forms.TabPage tabAutoInstall;
        private System.Windows.Forms.Button btnContinue;
        private System.Windows.Forms.Button btnAutoinstall;
        private System.Windows.Forms.Label lblManualWarning;
        private System.Windows.Forms.ListView lstIVSU;
        private System.Windows.Forms.ColumnHeader lvIVSUsType;
        private System.Windows.Forms.ColumnHeader lvIVSUsName;
        private System.Windows.Forms.ColumnHeader lvIVSUsVersion;
        private System.Windows.Forms.ColumnHeader lvIVSUsURL;
        private System.Windows.Forms.ColumnHeader lvIVSUsMD5;
        private System.Windows.Forms.Button btnShowConfiguration;
        private System.Windows.Forms.Label lblConfiguration;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.TabPage tabStatus;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblCurrentTask;
        private System.Windows.Forms.ListBox lstDownloadQueue;
        private System.Windows.Forms.Label lblDownloadSize;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.Label lblDownloadQueue;
        private System.Windows.Forms.Label lblFilesRemaining;
        private Controls.NewProgressBar barTotalDownloadProgress;
        private Controls.NewProgressBar barDownloadProgress;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Button btnShowLog;
        private System.Windows.Forms.Label lblMode1;
        private System.Windows.Forms.Label lblMode2;
    }
}