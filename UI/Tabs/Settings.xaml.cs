using System.ComponentModel;
using System.Windows.Controls;

namespace Syn3Updater.UI.Tabs
{
    /// <summary>
    /// Interaction logic for Install.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        public Settings()
        {
            InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                (this.DataContext as SettingsViewModel)?.Init();
            }
        }
    }
}
