using ReactiveUI;

namespace Syn3Updater.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private int _selectedTab;

        public int SelectedTab
        {
            get => _selectedTab;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedTab, value);
            }
        }
        public MainWindowViewModel()
        {
            AppMan.App.ShowHomeTab += delegate { SelectedTab = 0; };
            AppMan.App.ShowDownloadsTab += delegate { SelectedTab = 1; };
        }
    }
}
