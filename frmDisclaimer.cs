using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sync3Updater.Properties;

namespace Sync3Updater
{
    public partial class FrmDisclaimer : Form
    {
        public FrmDisclaimer()
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


        private void btnDisclaimerContinue_Click(object sender, EventArgs e)
        {
            Settings.Default.TOCAccepted2 = true;
            Settings.Default.Save();
            _ = new FrmSetup { Visible = true };
            this.Close();
        }

        private void chkDisclaimerConfirm_CheckedChanged(object sender, EventArgs e)
        {
            btnDisclaimerContinue.Enabled = chkDisclaimerConfirm.Checked;
        }

        private void FrmDisclaimer_Shown(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName != "en")
            {
                webDisclaimer.Navigate(
                    $"https://translate.google.co.uk/translate?hl=&sl=en&tl={Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName}&u=https%3A%2F%2Fcyanlabs.net%2Fapi%2FFordSyncDownloader%2Fdisclaimer.php");
            }
        }

        private void webDisclaimer_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webDisclaimer.Document == null || webDisclaimer.Document.GetElementsByTagName("style").Count < 1) return;
            HtmlElement style = webDisclaimer.Document.GetElementsByTagName("style")[0];
            style.InnerText += "#wtgbr, #gt-c { display:none !important;} #contentframe {top:0px !important;}";
        }
    }
}
