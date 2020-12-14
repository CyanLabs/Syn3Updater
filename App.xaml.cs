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
        //void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        //{
            //ApplicationManager.Logger.CrashWindow(e.Exception);
            //e.Handled = true;
        //}

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //if (!Debugger.IsAttached)
            {
                DispatcherUnhandledException += App_DispatcherUnhandledException;
            }

        }

        public static readonly SimpleLogger Logger = new SimpleLogger();
        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.CrashWindow(e.Exception);
            e.Handled = true;
        }

        #endregion

    }
}