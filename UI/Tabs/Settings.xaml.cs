using System.ComponentModel;
using System.Windows;

namespace Syn3Updater.UI.Tabs
{
    /// <summary>
    ///     Interaction logic for Install.xaml
    /// </summary>
    public partial class Settings
    {
        public Settings()
        {
            InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this)) (DataContext as SettingsViewModel)?.Init();
        }

        private void Settings_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool) e.NewValue && (bool) e.OldValue == false) (DataContext as SettingsViewModel)?.ReloadSettings();
        }
    }
}