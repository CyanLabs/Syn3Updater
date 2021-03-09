namespace Cyanlabs.Launcher
{
    public class UpgradingViewModel : BaseViewModel
    {
        #region Properties & Fields
        private string _message;
        private int _percentage = 100;

        public string Message
        {
            get => _message;
            set => Set(ref _message, value);
        }

        public int Percentage
        {
            get => _percentage;
            set => Set(ref _percentage, value);
        }
        #endregion

    }
}