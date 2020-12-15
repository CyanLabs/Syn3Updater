using System.ComponentModel;
using System.Windows.Controls;

namespace Syn3Updater.UI.Tabs
{
    /// <summary>
    /// Interaction logic for Install.xaml
    /// </summary>
    public partial class About : UserControl
    {
        public About()
        {
            InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                (this.DataContext as AboutViewmodel)?.Init();
            }
        }
    }
}
