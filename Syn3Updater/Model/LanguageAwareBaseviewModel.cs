namespace Cyanlabs.Syn3Updater.Model
{
    /// <summary>
    ///     Extension to BaseviewModel to allow it to be LanguageAware
    /// </summary>
    public class LanguageAwareBaseViewModel : BaseViewModel
    {
        #region Constructors

        public LanguageAwareBaseViewModel()
        {
            try
            {
                if (ApplicationManager.Instance != null)
                    ApplicationManager.Instance.LanguageChangedEvent += (sender, args) =>
                    {
                        if (ApplicationManager.Instance.Settings.Lang != null) Language = ApplicationManager.Instance.Settings.Lang;
                    };
            }
            catch
            {
                // ignored
            }
        }

        #endregion

        #region Properties & Fields

        private string _language;

        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }

        #endregion
    }
}