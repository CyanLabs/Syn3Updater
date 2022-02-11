using Application = Microsoft.Maui.Controls.Application;

namespace Syn3Updater
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            AppMan.App.Initialize();
            MainPage = new AppShell();
        }
    }
}
