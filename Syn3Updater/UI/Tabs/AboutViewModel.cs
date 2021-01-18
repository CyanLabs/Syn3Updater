using System.Diagnostics;
using System.Windows;
using Syn3Updater.Helper;
using Syn3Updater.Model;

namespace Syn3Updater.UI.Tabs
{
    internal class AboutViewmodel : LanguageAwareBaseViewModel
    {
        #region Properties & Fields

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

        private bool _disclaimerAccepted;

        public bool DisclaimerAccepted
        {
            get => _disclaimerAccepted;
            set
            {
                SetProperty(ref _disclaimerAccepted, value);
                ApplicationManager.Instance.Settings.DisclaimerAccepted = value;
                if (value) ApplicationManager.Instance.FireSettingsTabEvent();
            }
        }

        private Visibility _englishEndorsement;

        public Visibility EnglishEndorsement
        {
            get => _englishEndorsement;
            set
            {
                SetProperty(ref _englishEndorsement, value);
            }
        }

        #endregion

        #region Constructors

        public void Init()
        {
            DisclaimerAccepted = ApplicationManager.Instance.Settings.DisclaimerAccepted;
        }

        public void Reload()
        {
            EnglishEndorsement = ApplicationManager.Instance.Settings.Lang.Contains("en-") ? Visibility.Hidden : Visibility.Visible;
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
            Process.Start("https://www.paypal.com/paypalme/cyanlabs");
        }

        private void DocAction()
        {
            Process.Start("https://cyanlabs.net/applications/syn3-updater/");
        }

        private void FordHubAction()
        {
            Process.Start("https://cyanlabs.net/ford");
        }

        #endregion
    }
}