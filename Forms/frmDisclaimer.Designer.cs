namespace Syn3Updater.Forms
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
            resources.ApplyResources(this.webDisclaimer, "webDisclaimer");
            this.webDisclaimer.Name = "webDisclaimer";
            this.webDisclaimer.ScriptErrorsSuppressed = true;
            this.webDisclaimer.Url = new System.Uri("https://cyanlabs.net/api/Syn3Updater/disclaimer.php", System.UriKind.Absolute);
            this.webDisclaimer.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webDisclaimer_DocumentCompleted);
            // 
            // panelTitleBar
            // 
            this.panelTitleBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.panelTitleBar.Controls.Add(this.btnLogo);
            this.panelTitleBar.Controls.Add(this.btnClose);
            this.panelTitleBar.Controls.Add(this.lblTitle);
            resources.ApplyResources(this.panelTitleBar, "panelTitleBar");
            this.panelTitleBar.Name = "panelTitleBar";
            this.panelTitleBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTitleBar_MouseDown);
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
            // btnClose
            // 
            resources.ApplyResources(this.btnClose, "btnClose");
            this.btnClose.Name = "btnClose";
            this.btnClose.TabStop = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.btnClose.MouseLeave += new System.EventHandler(this.btnWindowControls_MouseLeave);
            this.btnClose.MouseHover += new System.EventHandler(this.btnClose_MouseHover);
            // 
            // lblTitle
            // 
            resources.ApplyResources(this.lblTitle, "lblTitle");
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTitleBar_MouseDown);
            // 
            // chkDisclaimerConfirm
            // 
            this.chkDisclaimerConfirm.Checked = global::Syn3Updater.Properties.Settings.Default.TOCAccepted2;
            this.chkDisclaimerConfirm.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::Syn3Updater.Properties.Settings.Default, "TOCAccepted2", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            resources.ApplyResources(this.chkDisclaimerConfirm, "chkDisclaimerConfirm");
            this.chkDisclaimerConfirm.Name = "chkDisclaimerConfirm";
            this.chkDisclaimerConfirm.UseVisualStyleBackColor = true;
            this.chkDisclaimerConfirm.CheckedChanged += new System.EventHandler(this.chkDisclaimerConfirm_CheckedChanged);
            // 
            // btnDisclaimerContinue
            // 
            this.btnDisclaimerContinue.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(38)))));
            resources.ApplyResources(this.btnDisclaimerContinue, "btnDisclaimerContinue");
            this.btnDisclaimerContinue.FlatAppearance.BorderSize = 0;
            this.btnDisclaimerContinue.Name = "btnDisclaimerContinue";
            this.btnDisclaimerContinue.UseVisualStyleBackColor = false;
            this.btnDisclaimerContinue.Click += new System.EventHandler(this.btnDisclaimerContinue_Click);
            // 
            // FrmDisclaimer
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(40)))), ((int)(((byte)(46)))));
            this.Controls.Add(this.chkDisclaimerConfirm);
            this.Controls.Add(this.btnDisclaimerContinue);
            this.Controls.Add(this.panelTitleBar);
            this.Controls.Add(this.webDisclaimer);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmDisclaimer";
            this.Shown += new System.EventHandler(this.FrmDisclaimer_Shown);
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

