using System.ComponentModel;
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
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                (this.DataContext as DownloadViewModel)?.Init();
            }
        }
    }
}
