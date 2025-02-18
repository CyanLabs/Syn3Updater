﻿using System.ComponentModel;
using System.Windows;

namespace Cyanlabs.Syn3Updater.UI.Tabs
{
    /// <summary>
    ///     Interaction logic for Install.xaml
    /// </summary>
    public partial class Home
    {
        public Home()
        {
            InitializeComponent();
        }

        private void Home_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(AppMan.App.DriveLetter == "invalid") (DataContext as HomeViewModel)?.ReloadUSB();
            if ((bool) e.NewValue && !(bool) e.OldValue) (DataContext as HomeViewModel)?.ReloadSettings();
        }
    }
}