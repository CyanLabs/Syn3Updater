using Syn3Updater.Model;

namespace Syn3Updater.UI
{
    public class LanguageAwareBaseViewModel : BaseViewModel
    {
        public LanguageAwareBaseViewModel()
        {
            //try
            //{
            //    if (ApplicationManager.Instance != null)
            //    {
            //        ApplicationManager.Instance.LanguageChangedEvent += (sender, args) =>
            //        {
            //            if (ApplicationManager.Instance?.NGSettings != null)
            //            {
            //                Language = ApplicationManager.Instance.NGSettings.Lang;
            //            }
            //        };
            //    }
            //}
            //catch
            //{
            //}
        }
        private string _language;

        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }

    }
}