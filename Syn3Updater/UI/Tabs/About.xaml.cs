using System.ComponentModel;
using System.Windows;

namespace Syn3Updater.UI.Tabs
{
    /// <summary>
    ///     Interaction logic for Install.xaml
    /// </summary>
    public partial class About
    {
        public About()
        {
            InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this)) (DataContext as AboutViewmodel)?.Init();
        }

        private void About_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && (bool)e.OldValue == false) (DataContext as AboutViewmodel)?.Reload();
        }
    }
}