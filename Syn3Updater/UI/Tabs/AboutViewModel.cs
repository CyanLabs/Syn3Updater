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
                AppMan.App.MainSettings.DisclaimerAccepted = value;
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
            DisclaimerAccepted = AppMan.App.MainSettings.DisclaimerAccepted;
        }

        public void Reload()
        {
            EnglishEndorsement = AppMan.App.MainSettings.Lang.Contains("en-") ? Visibility.Hidden : Visibility.Visible;
        }

        private static void WebsiteAction()
        {
            SystemHelper.OpenWebPage("https://cyanlabs.net");
        }

        private static void ForumAction()
        {
            SystemHelper.OpenWebPage("https://community.cyanlabs.net");
        }

        private static void DonateAction()
        {
            SystemHelper.OpenWebPage("https://www.paypal.com/paypalme/cyanlabs");
        }

        private static void DocAction()
        {
            SystemHelper.OpenWebPage("https://cyanlabs.net/applications/syn3-updater/");
        }

        private static void FHubAction()
        {
            SystemHelper.OpenWebPage("https://cyanlabs.net/fhub");
        }

        #endregion
    }
}