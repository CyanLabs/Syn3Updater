﻿using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
