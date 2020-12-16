using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoUpdaterDotNET;
using Syn3Updater.Model;
using Syn3Updater.Properties;

namespace Syn3Updater.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel Vm => (MainWindowViewModel)DataContext;

        public MainWindow()
        {
            InitializeComponent();
            Title = "Syn3 Updater " + Assembly.GetEntryAssembly().GetName().Version;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            AutoUpdater.Start("https://cyanlabs.net/api/latest.php?product=" + Assembly.GetEntryAssembly().GetName().Name);
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as Grid).DataContext is MainWindowViewModel.TabItem lvm)
            {
                if (string.IsNullOrWhiteSpace(lvm.Key))
                {
                    Vm.HamburgerExtended = !Vm.HamburgerExtended;
                }
                else
                {
                    Vm.CurrentTab = lvm.Key;
                }
            }
        }

        private void Grid_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            Vm.CurrentTab = "settings";
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Settings.Default.Save();
        }
    }
}
