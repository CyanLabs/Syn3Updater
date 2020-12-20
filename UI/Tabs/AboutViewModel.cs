using System.Diagnostics;
using Syn3Updater.Helper;

namespace Syn3Updater.UI.Tabs
{
    public class AboutViewmodel : LanguageAwareBaseViewModel
    {
        public void Init()
        {
            DisclaimerAccepted = Properties.Settings.Default.DisclaimerAccepted;
        }

        private bool _disclaimerAccepted;

        public bool DisclaimerAccepted
        {
            get => _disclaimerAccepted;
            set
            {
                SetProperty(ref _disclaimerAccepted, value);
                Properties.Settings.Default.DisclaimerAccepted = value;
                if (value)
                {
                    ApplicationManager.Instance.FireSettingsTabEvent();
                }
            }
        }

        private ActionCommand _websiteButton;
        public ActionCommand WebsiteButton => _websiteButton ?? (_websiteButton = new ActionCommand(WebsiteAction));

        private void WebsiteAction()
        {
            Process.Start("https://cyanlabs.net");
        }

        private ActionCommand _forumButton;
        public ActionCommand ForumButton => _forumButton ?? (_forumButton = new ActionCommand(ForumAction));

        private void ForumAction()
        {
            Process.Start("https://community.cyanlabs.net");
        }

        private ActionCommand _donateButton;
        public ActionCommand DonateButton => _donateButton ?? (_donateButton = new ActionCommand(DonateAction));

        private void DonateAction()
        {
            Process.Start("https://paypal.me/cyanlabs");
        }

        private ActionCommand _documentationButton;
        public ActionCommand DocButton => _documentationButton ?? (_documentationButton = new ActionCommand(DocAction));

        private void DocAction()
        {
            Process.Start("https://cyanlabs.net/applications/syn3-updater/");
        }

        private ActionCommand _fordhubButton;
        public ActionCommand FordHubButton => _fordhubButton ?? (_fordhubButton = new ActionCommand(FordHubAction));

        private void FordHubAction()
        {
            Process.Start("https://cyanlabs.net/ford");
        }
    }
}