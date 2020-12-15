using Syn3Updater.Model;
using Syn3Updater.Properties;

namespace Syn3Updater.UI
{
    public class LanguageAwareBaseViewModel : BaseViewModel
    {
        public LanguageAwareBaseViewModel()
        {
            try
            {
                if (ApplicationManager.Instance != null)
                {
                    ApplicationManager.Instance.LanguageChangedEvent += (sender, args) =>
                    {
                        if (Settings.Default.Lang != null)
                        {
                            Language = Settings.Default.Lang;
                        }
                    };
                }
            }
            catch
            {
            }
        }
        private string language;

        public string Language
        {
            get => language;
            set => SetProperty(ref language, value);
        }

    }
}