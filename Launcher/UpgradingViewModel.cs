namespace Cyanlabs.Launcher
{
    public class UpgradingViewModel : BaseViewModel
    {
        private string _message;
        public string Message
        {
            get => _message;
            set => Set(ref _message,value);
        }

        private int _percentage = 100;
        public int Percentage
        {
            get => _percentage;
            set => Set(ref _percentage, value);
        }
    }
}
