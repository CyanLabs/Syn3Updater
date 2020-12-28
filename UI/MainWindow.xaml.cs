using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoUpdaterDotNET;
using ModernWpf;

namespace Syn3Updater.UI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Methods
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                Title = $"Syn3 Updater {Assembly.GetEntryAssembly()?.GetName().Version}";
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }
            catch (System.NullReferenceException e)
            {

            }
        }

        private MainWindowViewModel Vm => (MainWindowViewModel) DataContext;

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as Grid)?.DataContext is MainWindowViewModel.TabItem lvm)
            {
                if (string.IsNullOrWhiteSpace(lvm.Key))
                    Vm.HamburgerExtended = !Vm.HamburgerExtended;
                else
                    Vm.CurrentTab = lvm.Key;
            }
        }

        private void Grid_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            Vm.CurrentTab = "settings";
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ApplicationManager.Instance.Exit();
        }

        #endregion
    }
}