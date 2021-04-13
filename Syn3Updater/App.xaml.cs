using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
            if (SingleInstance.AlreadyRunning())
                App.Current.Shutdown(); // Just shutdown the current application,if any instance found.  
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

        public sealed class SingleInstance
        {
            public static bool AlreadyRunning()
            {
                bool running = false;
                try
                {
                    // Getting collection of process  
                    Process currentProcess = Process.GetCurrentProcess();

                    // Check with other process already running   
                    foreach (var p in Process.GetProcesses())
                    {
                        if (p.Id != currentProcess.Id) // Check running process   
                        {
                            if (p.ProcessName.Equals(currentProcess.ProcessName))
                            {
                                running = true;
                                IntPtr hFound = p.MainWindowHandle;
                                User32API.ShowWindow(hFound, 9);
                                User32API.SetForegroundWindow(hFound); // Activate the window, if process is already running  
                                break;
                            }
                        }
                    }
                }
                catch
                {
                    // ignored
                }
                return running;
            }
        }
        #endregion
    }
    public class User32API
    {
        [DllImport("User32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
