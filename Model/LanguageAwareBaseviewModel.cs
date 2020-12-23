using Syn3Updater.Properties;

namespace Syn3Updater.Model
{
    public class LanguageAwareBaseViewModel : BaseViewModel
    {
        #region Properties & Fields
        private string _language;
        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }
        #endregion

        #region Constructors
        public LanguageAwareBaseViewModel()
        {
            try
            {
                if (ApplicationManager.Instance != null)
                    ApplicationManager.Instance.LanguageChangedEvent += (sender, args) =>
                    {
                        if (Settings.Default.Lang != null) Language = Settings.Default.Lang;
                    };
            }
            catch
            {
                // ignored
            }
        }
        #endregion
    }
}