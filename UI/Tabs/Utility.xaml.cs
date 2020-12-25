using System.ComponentModel;
using System.Windows;

namespace Syn3Updater.UI.Tabs
{
    /// <summary>
    ///     Interaction logic for Install.xaml
    /// </summary>
    public partial class Utility
    {
        public Utility()
        {
            InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this)) (DataContext as UtilityViewModel)?.Init();
        }

        private void Utility_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool) e.NewValue && (bool) e.OldValue == false) (DataContext as UtilityViewModel)?.ReloadSettings();
        }
    }
}