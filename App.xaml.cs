using System.Windows;
using System.Windows.Threading;

namespace Syn3Updater
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        #region Methods
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //if (!Debugger.IsAttached)
            {
                this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            }

            ApplicationManager.Instance.Initialize();
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ApplicationManager.Logger.CrashWindow(e.Exception);
            e.Handled = true;
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            //  throw new NotImplementedException();
        }
        #endregion
    }
}