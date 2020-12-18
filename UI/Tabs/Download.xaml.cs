using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Syn3Updater.UI.Tabs
{
    /// <summary>
    /// Interaction logic for Install.xaml
    /// </summary>
    public partial class Download : UserControl
    {
        public Download()
        {
            InitializeComponent();
            
        }

        private void Download_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true && (bool)e.OldValue == false)
            {
                (this.DataContext as DownloadViewModel)?.Init();
            }
        }
    }
}
