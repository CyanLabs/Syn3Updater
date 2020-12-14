using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Syn3Updater.UI;

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