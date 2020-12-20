using Syn3Updater.Properties;

namespace Syn3Updater.Model
{
    public class LanguageAwareBaseViewModel : BaseViewModel
    {
        private string _language;

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

        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }
    }
}