namespace Sync3Updater
{
    partial class FrmDisclaimer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmDisclaimer));
            this.webDisclaimer = new System.Windows.Forms.WebBrowser();
            this.panelTitleBar = new System.Windows.Forms.Panel();
            this.btnLogo = new System.Windows.Forms.PictureBox();
            this.btnClose = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.chkDisclaimerConfirm = new System.Windows.Forms.CheckBox();
            this.btnDisclaimerContinue = new System.Windows.Forms.Button();
            this.panelTitleBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).BeginInit();
            this.SuspendLayout();
            // 
            // webDisclaimer
            // 
            this.webDisclaimer.IsWebBrowserContextMenuEnabled = false;
            this.webDisclaimer.Location = new System.Drawing.Point(0, 42);
            this.webDisclaimer.MinimumSize = new System.Drawing.Size(20, 20);
            this.webDisclaimer.Name = "webDisclaimer";
            this.webDisclaimer.ScriptErrorsSuppressed = true;
            this.webDisclaimer.Size = new System.Drawing.Size(667, 402);
            this.webDisclaimer.TabIndex = 14;
            this.webDisclaimer.Url = new System.Uri("https://cyanlabs.net/api/FordSyncDownloader/disclaimer.php", System.UriKind.Absolute);
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
            this.panelTitleBar.Size = new System.Drawing.Size(667, 49);
            this.panelTitleBar.TabIndex = 15;
            this.panelTitleBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTitleBar_MouseDown);
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
            this.btnClose.Image = ((System.Drawing.Image)(resources.GetObject("btnClose.Image")));
            this.btnClose.Location = new System.Drawing.Point(651, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(16, 17);
            this.btnClose.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.btnClose.TabIndex = 2;
            this.btnClose.TabStop = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.btnClose.MouseLeave += new System.EventHandler(this.btnWindowControls_MouseLeave);
            this.btnClose.MouseHover += new System.EventHandler(this.btnClose_MouseHover);
            // 
            // lblTitle
            // 
            this.lblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(201, 8);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(428, 30);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "SYNC 3 UPDATER - DISCLAIMER\r\n";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTitleBar_MouseDown);
            // 
            // chkDisclaimerConfirm
            // 
            this.chkDisclaimerConfirm.Checked = global::Sync3Updater.Properties.Settings.Default.TOCAccepted2;
            this.chkDisclaimerConfirm.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::Sync3Updater.Properties.Settings.Default, "TOCAccepted2", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.chkDisclaimerConfirm.Location = new System.Drawing.Point(47, 454);
            this.chkDisclaimerConfirm.Name = "chkDisclaimerConfirm";
            this.chkDisclaimerConfirm.Size = new System.Drawing.Size(513, 39);
            this.chkDisclaimerConfirm.TabIndex = 17;
            this.chkDisclaimerConfirm.Text = "I understand and accept the risks outlined in the disclaimer above.\r\nCyanlabs is " +
    "not responsible for any undesired outcome of this operation.";
            this.chkDisclaimerConfirm.UseVisualStyleBackColor = true;
            this.chkDisclaimerConfirm.CheckedChanged += new System.EventHandler(this.chkDisclaimerConfirm_CheckedChanged);
            // 
            // btnDisclaimerContinue
            // 
            this.btnDisclaimerContinue.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            this.btnDisclaimerContinue.Enabled = false;
            this.btnDisclaimerContinue.FlatAppearance.BorderSize = 0;
            this.btnDisclaimerContinue.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnDisclaimerContinue.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.btnDisclaimerContinue.Location = new System.Drawing.Point(566, 455);
            this.btnDisclaimerContinue.Name = "btnDisclaimerContinue";
            this.btnDisclaimerContinue.Size = new System.Drawing.Size(91, 36);
            this.btnDisclaimerContinue.TabIndex = 16;
            this.btnDisclaimerContinue.Text = "Continue";
            this.btnDisclaimerContinue.UseVisualStyleBackColor = false;
            this.btnDisclaimerContinue.Click += new System.EventHandler(this.btnDisclaimerContinue_Click);
            // 
            // frmDisclaimer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(40)))), ((int)(((byte)(46)))));
            this.ClientSize = new System.Drawing.Size(667, 500);
            this.Controls.Add(this.chkDisclaimerConfirm);
            this.Controls.Add(this.btnDisclaimerContinue);
            this.Controls.Add(this.panelTitleBar);
            this.Controls.Add(this.webDisclaimer);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmDisclaimer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cyanlabs Ford Sync Update Downloader";
            this.panelTitleBar.ResumeLayout(false);
            this.panelTitleBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser webDisclaimer;
        private System.Windows.Forms.Panel panelTitleBar;
        private System.Windows.Forms.PictureBox btnClose;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.CheckBox chkDisclaimerConfirm;
        private System.Windows.Forms.Button btnDisclaimerContinue;
        private System.Windows.Forms.PictureBox btnLogo;
    }
}

