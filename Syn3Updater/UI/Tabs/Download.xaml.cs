using System.Windows;

namespace Cyanlabs.Syn3Updater.UI.Tabs
{
    /// <summary>
    ///     Interaction logic for Install.xaml
    /// </summary>
    public partial class Download
    {
        public Download()
        {
            InitializeComponent();
        }

        private void Download_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool) e.NewValue && (bool) e.OldValue == false) (DataContext as DownloadViewModel)?.Init();
        }
    }
}