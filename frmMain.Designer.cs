namespace Sync3Updater
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
            this.lblRegionInfo = new System.Windows.Forms.Label();
            this.lblRelease = new System.Windows.Forms.Label();
            this.lblRegion = new System.Windows.Forms.Label();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.tabControl1 = new TabControlWithoutHeader();
            this.tabReformat = new System.Windows.Forms.TabPage();
            this.tabAutoInstall = new System.Windows.Forms.TabPage();
            this.button1 = new System.Windows.Forms.Button();
            this.lblManualWarning = new System.Windows.Forms.Label();
            this.lstIVSU = new System.Windows.Forms.ListView();
            this.lvIVSUsType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvIVSUsName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvIVSUsVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvIVSUsURL = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvIVSUsMD5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnContinueNewVersion = new System.Windows.Forms.Button();
            this.btnShowConfiguration = new System.Windows.Forms.Button();
            this.lblConfiguration = new System.Windows.Forms.Label();
            this.panelTitleBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnMinimize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).BeginInit();
            this.grpUSB.SuspendLayout();
            this.grpNewVersion.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabAutoInstall.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(193, 8);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(408, 30);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "SYNC 3 UPDATER";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTitleBar_MouseDown);
            // 
            // panelTitleBar
            // 
            this.panelTitleBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.panelTitleBar.Controls.Add(this.btnMinimize);
            this.panelTitleBar.Controls.Add(this.btnLogo);
            this.panelTitleBar.Controls.Add(this.btnClose);
            this.panelTitleBar.Controls.Add(this.lblTitle);
            this.panelTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTitleBar.Location = new System.Drawing.Point(0, 0);
            this.panelTitleBar.Name = "panelTitleBar";
            this.panelTitleBar.Size = new System.Drawing.Size(620, 49);
            this.panelTitleBar.TabIndex = 17;
            this.panelTitleBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTitleBar_MouseDown);
            // 
            // btnMinimize
            // 
            this.btnMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMinimize.Image = ((System.Drawing.Image)(resources.GetObject("btnMinimize.Image")));
            this.btnMinimize.Location = new System.Drawing.Point(587, 0);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Size = new System.Drawing.Size(16, 17);
            this.btnMinimize.TabIndex = 8;
            this.btnMinimize.TabStop = false;
            this.btnMinimize.Click += new System.EventHandler(this.btnMinimize_Click);
            this.btnMinimize.MouseLeave += new System.EventHandler(this.btnWindowControls_MouseLeave);
            this.btnMinimize.MouseHover += new System.EventHandler(this.btnMinimize_MouseHover);
            // 
            // btnLogo
            // 
            this.btnLogo.BackColor = System.Drawing.Color.Transparent;
            this.btnLogo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLogo.Image = global::Sync3Updater.Properties.Resources.logo;
            this.btnLogo.Location = new System.Drawing.Point(4, 2);
            this.btnLogo.Name = "btnLogo";
            this.btnLogo.Size = new System.Drawing.Size(191, 46);
            this.btnLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.btnLogo.TabIndex = 7;
            this.btnLogo.TabStop = false;
            this.btnLogo.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTitleBar_MouseDown);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Image = ((System.Drawing.Image)(resources.GetObject("btnClose.Image")));
            this.btnClose.Location = new System.Drawing.Point(604, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(16, 17);
            this.btnClose.TabIndex = 2;
            this.btnClose.TabStop = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.btnClose.MouseLeave += new System.EventHandler(this.btnWindowControls_MouseLeave);
            this.btnClose.MouseHover += new System.EventHandler(this.btnClose_MouseHover);
            // 
            // grpUSB
            // 
            this.grpUSB.Controls.Add(this.label1);
            this.grpUSB.Controls.Add(this.cmbDriveList);
            this.grpUSB.Controls.Add(this.btnRefreshUSB);
            this.grpUSB.Controls.Add(this.lblDriveInfo);
            this.grpUSB.ForeColor = System.Drawing.Color.White;
            this.grpUSB.Location = new System.Drawing.Point(16, 58);
            this.grpUSB.Name = "grpUSB";
            this.grpUSB.Size = new System.Drawing.Size(589, 91);
            this.grpUSB.TabIndex = 35;
            this.grpUSB.TabStop = false;
            this.grpUSB.Text = "Select a USB Drive";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.Location = new System.Drawing.Point(321, 12);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(3);
            this.label1.Size = new System.Drawing.Size(261, 73);
            this.label1.TabIndex = 19;
            this.label1.Text = "Recommended USB Flash Drive sizes\r\n\r\nNon Navigation APIM\'s should be find with 8G" +
    "B\r\nNA, ROW, ANZ APIM\'s will require atleast 16GB\r\nEU APIM\'s will require atleast" +
    " 32GB";
            // 
            // cmbDriveList
            // 
            this.cmbDriveList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.cmbDriveList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDriveList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbDriveList.ForeColor = System.Drawing.Color.White;
            this.cmbDriveList.FormattingEnabled = true;
            this.cmbDriveList.Location = new System.Drawing.Point(6, 19);
            this.cmbDriveList.Name = "cmbDriveList";
            this.cmbDriveList.Size = new System.Drawing.Size(296, 21);
            this.cmbDriveList.TabIndex = 9;
            // 
            // btnRefreshUSB
            // 
            this.btnRefreshUSB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.btnRefreshUSB.FlatAppearance.BorderSize = 0;
            this.btnRefreshUSB.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRefreshUSB.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.btnRefreshUSB.Location = new System.Drawing.Point(6, 43);
            this.btnRefreshUSB.Name = "btnRefreshUSB";
            this.btnRefreshUSB.Size = new System.Drawing.Size(112, 42);
            this.btnRefreshUSB.TabIndex = 18;
            this.btnRefreshUSB.Text = "Reload Devices";
            this.btnRefreshUSB.UseVisualStyleBackColor = false;
            // 
            // lblDriveInfo
            // 
            this.lblDriveInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.lblDriveInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblDriveInfo.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblDriveInfo.Location = new System.Drawing.Point(121, 43);
            this.lblDriveInfo.Name = "lblDriveInfo";
            this.lblDriveInfo.Size = new System.Drawing.Size(181, 42);
            this.lblDriveInfo.TabIndex = 14;
            this.lblDriveInfo.Text = "Drive Name:\r\nDrive Letter:\r\nFilesystem:";
            // 
            // grpNewVersion
            // 
            this.grpNewVersion.Controls.Add(this.txtReleaseNotes);
            this.grpNewVersion.Controls.Add(this.lblMapVersion1);
            this.grpNewVersion.Controls.Add(this.cmbMapVersion);
            this.grpNewVersion.Controls.Add(this.cmbRelease);
            this.grpNewVersion.Controls.Add(this.cmbRegion);
            this.grpNewVersion.Controls.Add(this.lblRegionInfo);
            this.grpNewVersion.Controls.Add(this.lblRelease);
            this.grpNewVersion.Controls.Add(this.lblRegion);
            this.grpNewVersion.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.grpNewVersion.ForeColor = System.Drawing.Color.White;
            this.grpNewVersion.Location = new System.Drawing.Point(16, 155);
            this.grpNewVersion.Name = "grpNewVersion";
            this.grpNewVersion.Size = new System.Drawing.Size(589, 124);
            this.grpNewVersion.TabIndex = 34;
            this.grpNewVersion.TabStop = false;
            this.grpNewVersion.Text = "New Sync 3 Version";
            // 
            // txtReleaseNotes
            // 
            this.txtReleaseNotes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.txtReleaseNotes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtReleaseNotes.ForeColor = System.Drawing.Color.White;
            this.txtReleaseNotes.Location = new System.Drawing.Point(272, 21);
            this.txtReleaseNotes.Multiline = true;
            this.txtReleaseNotes.Name = "txtReleaseNotes";
            this.txtReleaseNotes.ReadOnly = true;
            this.txtReleaseNotes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtReleaseNotes.Size = new System.Drawing.Size(305, 91);
            this.txtReleaseNotes.TabIndex = 23;
            this.txtReleaseNotes.Text = "Notes: ";
            // 
            // lblMapVersion1
            // 
            this.lblMapVersion1.Location = new System.Drawing.Point(3, 91);
            this.lblMapVersion1.Name = "lblMapVersion1";
            this.lblMapVersion1.Size = new System.Drawing.Size(87, 13);
            this.lblMapVersion1.TabIndex = 27;
            this.lblMapVersion1.Text = "Map Version:";
            this.lblMapVersion1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbMapVersion
            // 
            this.cmbMapVersion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.cmbMapVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMapVersion.Enabled = false;
            this.cmbMapVersion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbMapVersion.ForeColor = System.Drawing.Color.White;
            this.cmbMapVersion.FormattingEnabled = true;
            this.cmbMapVersion.Location = new System.Drawing.Point(92, 87);
            this.cmbMapVersion.Name = "cmbMapVersion";
            this.cmbMapVersion.Size = new System.Drawing.Size(151, 21);
            this.cmbMapVersion.TabIndex = 26;
            // 
            // cmbRelease
            // 
            this.cmbRelease.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.cmbRelease.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRelease.Enabled = false;
            this.cmbRelease.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbRelease.ForeColor = System.Drawing.Color.White;
            this.cmbRelease.FormattingEnabled = true;
            this.cmbRelease.Location = new System.Drawing.Point(92, 54);
            this.cmbRelease.Name = "cmbRelease";
            this.cmbRelease.Size = new System.Drawing.Size(151, 21);
            this.cmbRelease.TabIndex = 11;
            // 
            // cmbRegion
            // 
            this.cmbRegion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.cmbRegion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRegion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbRegion.ForeColor = System.Drawing.Color.White;
            this.cmbRegion.FormattingEnabled = true;
            this.cmbRegion.Items.AddRange(new object[] {
            "CN",
            "EU",
            "NA",
            "ANZ",
            "ROW"});
            this.cmbRegion.Location = new System.Drawing.Point(92, 21);
            this.cmbRegion.Name = "cmbRegion";
            this.cmbRegion.Size = new System.Drawing.Size(151, 21);
            this.cmbRegion.TabIndex = 15;
            // 
            // lblRegionInfo
            // 
            this.lblRegionInfo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblRegionInfo.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold);
            this.lblRegionInfo.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblRegionInfo.Location = new System.Drawing.Point(248, 15);
            this.lblRegionInfo.Name = "lblRegionInfo";
            this.lblRegionInfo.Size = new System.Drawing.Size(18, 30);
            this.lblRegionInfo.TabIndex = 22;
            this.lblRegionInfo.Text = "?";
            // 
            // lblRelease
            // 
            this.lblRelease.Location = new System.Drawing.Point(3, 57);
            this.lblRelease.Name = "lblRelease";
            this.lblRelease.Size = new System.Drawing.Size(86, 13);
            this.lblRelease.TabIndex = 13;
            this.lblRelease.Text = "Version:";
            this.lblRelease.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblRegion
            // 
            this.lblRegion.Location = new System.Drawing.Point(6, 25);
            this.lblRegion.Name = "lblRegion";
            this.lblRegion.Size = new System.Drawing.Size(84, 13);
            this.lblRegion.TabIndex = 14;
            this.lblRegion.Text = "Region:";
            this.lblRegion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(192, 74);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(192, 74);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(192, 74);
            this.tabPage3.TabIndex = 0;
            this.tabPage3.Text = "tabPage3";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(192, 74);
            this.tabPage4.TabIndex = 1;
            this.tabPage4.Text = "tabPage4";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl1.Controls.Add(this.tabReformat);
            this.tabControl1.Controls.Add(this.tabAutoInstall);
            this.tabControl1.ItemSize = new System.Drawing.Size(0, 1);
            this.tabControl1.Location = new System.Drawing.Point(16, 323);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(589, 316);
            this.tabControl1.TabIndex = 16;
            // 
            // tabReformat
            // 
            this.tabReformat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.tabReformat.Location = new System.Drawing.Point(0, 0);
            this.tabReformat.Name = "tabReformat";
            this.tabReformat.Size = new System.Drawing.Size(589, 316);
            this.tabReformat.TabIndex = 0;
            this.tabReformat.Text = "tabReformat";
            // 
            // tabAutoInstall
            // 
            this.tabAutoInstall.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.tabAutoInstall.Controls.Add(this.button1);
            this.tabAutoInstall.Controls.Add(this.lblManualWarning);
            this.tabAutoInstall.Controls.Add(this.lstIVSU);
            this.tabAutoInstall.Location = new System.Drawing.Point(0, 0);
            this.tabAutoInstall.Name = "tabAutoInstall";
            this.tabAutoInstall.Size = new System.Drawing.Size(589, 316);
            this.tabAutoInstall.TabIndex = 1;
            this.tabAutoInstall.Text = "tabAutoInstall";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button1.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.button1.Location = new System.Drawing.Point(448, 273);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(133, 31);
            this.button1.TabIndex = 37;
            this.button1.Text = "Continue";
            this.button1.UseVisualStyleBackColor = false;
            // 
            // lblManualWarning
            // 
            this.lblManualWarning.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(40)))), ((int)(((byte)(46)))));
            this.lblManualWarning.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblManualWarning.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblManualWarning.Location = new System.Drawing.Point(9, 11);
            this.lblManualWarning.Name = "lblManualWarning";
            this.lblManualWarning.Padding = new System.Windows.Forms.Padding(5);
            this.lblManualWarning.Size = new System.Drawing.Size(572, 60);
            this.lblManualWarning.TabIndex = 26;
            this.lblManualWarning.Text = "Press Start to begin the download/install, optionally deselect any IVSU\'s you don" +
    "\'t want.\r\n\r\nNOTE: This install mode was automatically chosen based on your confi" +
    "guration\r\n";
            this.lblManualWarning.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lstIVSU
            // 
            this.lstIVSU.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.lstIVSU.CheckBoxes = true;
            this.lstIVSU.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.lvIVSUsType,
            this.lvIVSUsName,
            this.lvIVSUsVersion,
            this.lvIVSUsURL,
            this.lvIVSUsMD5});
            this.lstIVSU.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lstIVSU.ForeColor = System.Drawing.Color.White;
            this.lstIVSU.FullRowSelect = true;
            this.lstIVSU.HideSelection = false;
            this.lstIVSU.Location = new System.Drawing.Point(9, 91);
            this.lstIVSU.Name = "lstIVSU";
            this.lstIVSU.Size = new System.Drawing.Size(572, 173);
            this.lstIVSU.TabIndex = 1;
            this.lstIVSU.UseCompatibleStateImageBehavior = false;
            this.lstIVSU.View = System.Windows.Forms.View.Details;
            // 
            // lvIVSUsType
            // 
            this.lvIVSUsType.Text = "Type";
            this.lvIVSUsType.Width = 138;
            // 
            // lvIVSUsName
            // 
            this.lvIVSUsName.Text = "Name";
            this.lvIVSUsName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.lvIVSUsName.Width = 212;
            // 
            // lvIVSUsVersion
            // 
            this.lvIVSUsVersion.Text = "Version";
            this.lvIVSUsVersion.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.lvIVSUsVersion.Width = 217;
            // 
            // lvIVSUsURL
            // 
            this.lvIVSUsURL.Text = "URL";
            this.lvIVSUsURL.Width = 0;
            // 
            // lvIVSUsMD5
            // 
            this.lvIVSUsMD5.Width = 0;
            // 
            // btnContinueNewVersion
            // 
            this.btnContinueNewVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnContinueNewVersion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.btnContinueNewVersion.FlatAppearance.BorderSize = 0;
            this.btnContinueNewVersion.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnContinueNewVersion.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.btnContinueNewVersion.Location = new System.Drawing.Point(472, 285);
            this.btnContinueNewVersion.Name = "btnContinueNewVersion";
            this.btnContinueNewVersion.Size = new System.Drawing.Size(133, 31);
            this.btnContinueNewVersion.TabIndex = 36;
            this.btnContinueNewVersion.Text = "Continue";
            this.btnContinueNewVersion.UseVisualStyleBackColor = false;
            this.btnContinueNewVersion.Click += new System.EventHandler(this.btnContinueNewVersion_Click);
            // 
            // btnShowConfiguration
            // 
            this.btnShowConfiguration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnShowConfiguration.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.btnShowConfiguration.FlatAppearance.BorderSize = 0;
            this.btnShowConfiguration.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnShowConfiguration.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.btnShowConfiguration.Location = new System.Drawing.Point(16, 285);
            this.btnShowConfiguration.Name = "btnShowConfiguration";
            this.btnShowConfiguration.Size = new System.Drawing.Size(105, 31);
            this.btnShowConfiguration.TabIndex = 37;
            this.btnShowConfiguration.Text = "Configuration";
            this.btnShowConfiguration.UseVisualStyleBackColor = false;
            this.btnShowConfiguration.Click += new System.EventHandler(this.btnShowConfiguration_Click);
            // 
            // lblConfiguration
            // 
            this.lblConfiguration.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.lblConfiguration.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblConfiguration.Location = new System.Drawing.Point(123, 285);
            this.lblConfiguration.Name = "lblConfiguration";
            this.lblConfiguration.Size = new System.Drawing.Size(300, 31);
            this.lblConfiguration.TabIndex = 38;
            this.lblConfiguration.Text = "Current Version: 3.4.19200 - Region: EU - Navigation: Yes\r\nDownload Path: ";
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(40)))), ((int)(((byte)(46)))));
            this.ClientSize = new System.Drawing.Size(620, 650);
            this.Controls.Add(this.lblConfiguration);
            this.Controls.Add(this.btnShowConfiguration);
            this.Controls.Add(this.btnContinueNewVersion);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.grpUSB);
            this.Controls.Add(this.grpNewVersion);
            this.Controls.Add(this.panelTitleBar);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cyanlabs Ford Sync Update Downloader";
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.Shown += new System.EventHandler(this.FrmMain_Shown);
            this.panelTitleBar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.btnMinimize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).EndInit();
            this.grpUSB.ResumeLayout(false);
            this.grpUSB.PerformLayout();
            this.grpNewVersion.ResumeLayout(false);
            this.grpNewVersion.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabAutoInstall.ResumeLayout(false);
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
        private TabControlWithoutHeader tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabReformat;
        private System.Windows.Forms.TabPage tabAutoInstall;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Button btnContinueNewVersion;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lblManualWarning;
        private System.Windows.Forms.ListView lstIVSU;
        private System.Windows.Forms.ColumnHeader lvIVSUsType;
        private System.Windows.Forms.ColumnHeader lvIVSUsName;
        private System.Windows.Forms.ColumnHeader lvIVSUsVersion;
        private System.Windows.Forms.ColumnHeader lvIVSUsURL;
        private System.Windows.Forms.ColumnHeader lvIVSUsMD5;
        private System.Windows.Forms.Button btnShowConfiguration;
        private System.Windows.Forms.Label lblConfiguration;
    }
}