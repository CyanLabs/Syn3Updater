using Syn3Updater.Model;

namespace Syn3Updater.UI
{
    public class LanguageAwareBaseViewModel : BaseViewModel
    {
        private string _language;

        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }

    }
}