using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
    }
}
