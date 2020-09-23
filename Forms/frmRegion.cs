using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Syn3Updater.Helpers;
using Syn3Updater.Properties;

namespace Syn3Updater.Forms
{
    public partial class FrmRegion : Form
    {
        public string Language = Settings.Default.Language;
        public FrmRegion()
        {
            if (!string.IsNullOrEmpty(Language)) Thread.CurrentThread.CurrentUICulture = new CultureInfo(Language);
            InitializeComponent();
        }

        private void panelTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            Functions.ReleaseCapture();
            Functions.SendMessage(Handle, 0x112, 0xf012, 0);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void RegionInfo_Load(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName != "en")
            {
                webRegion.Navigate(
                    $"https://translate.google.co.uk/translate?hl=&sl=en&tl={Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName}&u=https%3A%2F%2Fcyanlabs.net%2Fapi%2FSyn3Updater%2Fregion.php");
            }
        }

        private void webRegion_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            // ReSharper disable once PossibleNullReferenceException
            if (webRegion.Document.GetElementsByTagName("style").Count < 1) return;
            HtmlElement style = webRegion.Document.GetElementsByTagName("style")[0];
            style.InnerText += "#wtgbr, #gt-c { display:none !important;} #contentframe {top:0px !important;}";
        }
    }
}
