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
                if (AppMan.App != null)
                    AppMan.App.LanguageChangedEvent += (sender, args) =>
                    {
                        if (AppMan.App.MainSettings.Lang != null) Language = AppMan.App.MainSettings.Lang;
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