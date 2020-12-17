using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace Syn3Updater.UI.Tabs
{
    /// <summary>
    /// Interaction logic for Install.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        public Home()
        {
            InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                (this.DataContext as HomeViewModel)?.Init();
            }
        }
    }
}
