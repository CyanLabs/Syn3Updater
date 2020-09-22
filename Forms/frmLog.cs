using System;
using System.Windows.Forms;
using Syn3Updater.Forms;

namespace Syn3Updater
{
    public partial class frmLog : Form
    {
        public frmLog()
        {
            InitializeComponent();
        }

        private FrmMain _frmMain;
        public void frmLog_Load(object sender, EventArgs e)
        {
            _frmMain = new FrmMain();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            txtLog.Text = FrmMain.Logoutput;
        }

        private void txtLog_TextChanged(object sender, EventArgs e)
        {
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.ScrollToCaret();
        }
    }
}
