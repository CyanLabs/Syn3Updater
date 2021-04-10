using System.Windows;
using System.Windows.Threading;

namespace Cyanlabs.Syn3Updater
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        #region Methods

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            {
                DispatcherUnhandledException += App_DispatcherUnhandledException;
            }

            AppMan.App.Initialize();
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            AppMan.Logger.CrashWindow(e.Exception);
            e.Handled = true;
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            //  throw new NotImplementedException();
        }

        #endregion
    }
}