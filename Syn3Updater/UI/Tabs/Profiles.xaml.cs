using System.ComponentModel;
using System.Windows;

namespace Cyanlabs.Syn3Updater.UI.Tabs
{
    /// <summary>
    ///     Interaction logic for Install.xaml
    /// </summary>
    public partial class Profiles
    {
        public Profiles()
        {
            InitializeComponent();
        }
        
        private void Profiles_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && !(bool)e.OldValue) (DataContext as ProfilesViewModel)?.Reload();
        }
    }
}