using System;
using System.Security.Principal;
using ReactiveUI;

namespace Syn3Updater.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private int _selectedTab;

        public int SelectedTab
        {
            get => _selectedTab;
            set => this.RaiseAndSetIfChanged(ref _selectedTab, value);
        }
        
        private bool _runAsAdmin;

        public bool RunAsAdmin
        {
            get => _runAsAdmin;
            set => this.RaiseAndSetIfChanged(ref _runAsAdmin, value);
        }
        public MainWindowViewModel()
        {
            AppMan.App.ShowHomeTab += delegate { SelectedTab = 0; };
            AppMan.App.ShowDownloadsTab += delegate { SelectedTab = 1; };
            
            //TODO Close application if not administrator after X time
            if (OperatingSystem.IsWindows()) {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    WindowsPrincipal principal = new(identity);
                    RunAsAdmin = !principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
        }
    }
}
