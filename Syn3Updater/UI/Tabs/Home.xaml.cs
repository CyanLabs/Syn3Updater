﻿using System.ComponentModel;
using System.Windows;
using Cyanlabs.Syn3Updater.Helper;

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
            if (!DesignerProperties.GetIsInDesignMode(this)) (DataContext as HomeViewModel)?.Init();
        }

        private void Home_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && !(bool)e.OldValue) (DataContext as HomeViewModel)?.ReloadSettings();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FileHelper.ExtractMultiPackage(@"D:\Syn3Updater\FordCaribbean19Q4.tgz");
        }
    }
}