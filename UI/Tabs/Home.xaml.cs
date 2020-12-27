using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace Syn3Updater.UI.Tabs
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
            if ((bool) e.NewValue && (bool) e.OldValue == false) (DataContext as HomeViewModel)?.ReloadSettings();
        }
    }
}