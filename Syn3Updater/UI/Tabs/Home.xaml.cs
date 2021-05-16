using System.ComponentModel;
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
            if ((bool)e.OldValue == false && (bool)e.NewValue)
            {
                (DataContext as HomeViewModel)?.ReloadSettings();
            }
        }
    }
}