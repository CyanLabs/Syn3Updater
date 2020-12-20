using System.Diagnostics;
using Syn3Updater.Helper;
using Syn3Updater.Model;

namespace Syn3Updater.UI.Tabs
{
    public class AboutViewmodel : LanguageAwareBaseViewModel
    {
        private bool _disclaimerAccepted;

        private ActionCommand _documentationButton;

        private ActionCommand _donateButton;

        private ActionCommand _fordhubButton;

        private ActionCommand _forumButton;

        private ActionCommand _websiteButton;
        public ActionCommand WebsiteButton => _websiteButton ?? (_websiteButton = new ActionCommand(WebsiteAction));
        public ActionCommand ForumButton => _forumButton ?? (_forumButton = new ActionCommand(ForumAction));
        public ActionCommand DonateButton => _donateButton ?? (_donateButton = new ActionCommand(DonateAction));
        public ActionCommand DocButton => _documentationButton ?? (_documentationButton = new ActionCommand(DocAction));
        public ActionCommand FordHubButton => _fordhubButton ?? (_fordhubButton = new ActionCommand(FordHubAction));

        public bool DisclaimerAccepted
        {
            get => _disclaimerAccepted;
            set
            {
                SetProperty(ref _disclaimerAccepted, value);
                Properties.Settings.Default.DisclaimerAccepted = value;
                if (value) ApplicationManager.Instance.FireSettingsTabEvent();
            }
        }

        public void Init()
        {
            DisclaimerAccepted = Properties.Settings.Default.DisclaimerAccepted;
        }

        private void WebsiteAction()
        {
            Process.Start("https://cyanlabs.net");
        }

        private void ForumAction()
        {
            Process.Start("https://community.cyanlabs.net");
        }

        private void DonateAction()
        {
            Process.Start("https://paypal.me/cyanlabs");
        }

        private void DocAction()
        {
            Process.Start("https://cyanlabs.net/applications/syn3-updater/");
        }

        private void FordHubAction()
        {
            Process.Start("https://cyanlabs.net/ford");
        }
    }
}