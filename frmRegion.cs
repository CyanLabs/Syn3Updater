using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sync3Updater.Properties;

namespace Sync3Updater
{
    public partial class frmRegion : Form
    {
        public frmRegion()
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

        private void btnMinimize_MouseHover(object sender, EventArgs e)
        {
            btnClose.Image = Resources.minimize;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnWindowControls_MouseLeave(object sender, EventArgs e)
        {
            ((PictureBox)sender).Image = Resources.button;
        }
        private void btnMinimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }
        #endregion


        private void RegionInfo_Load(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToString() != "en")
            {
                webRegion.Navigate(
                    $"https://translate.google.co.uk/translate?hl=&sl=en&tl={Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToString()}&u=https%3A%2F%2Fcyanlabs.net%2Fapi%2FFordSyncDownloader%2Fregion.php");
            }
        }

        private void webRegion_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webRegion.Document.GetElementsByTagName("style").Count < 1) return;
            HtmlElement style = webRegion.Document.GetElementsByTagName("style")[0];
            style.InnerText += "#wtgbr, #gt-c { display:none !important;} #contentframe {top:0px !important;}";
        }
    }
}
