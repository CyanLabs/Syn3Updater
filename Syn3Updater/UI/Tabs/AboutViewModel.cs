using System.Diagnostics;
using System.Windows;
using Cyanlabs.Syn3Updater.Helper;
using Cyanlabs.Syn3Updater.Model;

namespace Cyanlabs.Syn3Updater.UI.Tabs
{
    internal class AboutViewmodel : LanguageAwareBaseViewModel
    {
        #region Properties & Fields

        private ActionCommand _documentationButton;
        private ActionCommand _donateButton;
        private ActionCommand _fhubButton;
        private ActionCommand _forumButton;
        private ActionCommand _websiteButton;
        public ActionCommand WebsiteButton => _websiteButton ??= new ActionCommand(WebsiteAction);
        public ActionCommand ForumButton => _forumButton ??= new ActionCommand(ForumAction);
        public ActionCommand DonateButton => _donateButton ??= new ActionCommand(DonateAction);
        public ActionCommand DocButton => _documentationButton ??= new ActionCommand(DocAction);
        public ActionCommand FHubButton => _fhubButton ??= new ActionCommand(FHubAction);

        private bool _disclaimerAccepted;

        public bool DisclaimerAccepted
        {
            get => _disclaimerAccepted;
            set
            {
                SetProperty(ref _disclaimerAccepted, value);
                AppMan.App.Settings.DisclaimerAccepted = value;
                if (value) AppMan.App.FireSettingsTabEvent();
            }
        }

        private Visibility _englishEndorsement;

        public Visibility EnglishEndorsement
        {
            get => _englishEndorsement;
            set => SetProperty(ref _englishEndorsement, value);
        }

        #endregion

        #region Constructors

        public void Init()
        {
            DisclaimerAccepted = AppMan.App.Settings.DisclaimerAccepted;
        }

        public void Reload()
        {
            EnglishEndorsement = AppMan.App.Settings.Lang.Contains("en-") ? Visibility.Hidden : Visibility.Visible;
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

        private void FHubAction()
        {
            Process.Start("https://cyanlabs.net/fhub");
        }

        #endregion
    }
}