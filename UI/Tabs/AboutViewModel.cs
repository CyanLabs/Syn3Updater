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
                Properties.Settings.Default.Save();
            }
        }
    }
}