using System.ComponentModel;
using System.Windows;


namespace Cyanlabs.Syn3Updater.UI.Tabs
{
    /// <summary>
    ///     Interaction logic for Settings.xaml
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
            if ((bool) e.NewValue && !(bool) e.OldValue) (DataContext as SettingsViewModel)?.ReloadSettings();
        }

        // Yes it's in the Code Behind, i can't seem to get it working in the viewmodel by binding the Toggled event.
        private void My20Toggle_OnToggled(object sender, RoutedEventArgs e)
        {
             (DataContext as SettingsViewModel)?.UpdateMy20Toggle(My20Toggle.IsOn);
        }
        
        // Yes it's in the Code Behind, i can't seem to get it working in the viewmodel by binding the Toggled event.
        private void AdvancedModeToggle_OnToggled(object sender, RoutedEventArgs e)
        {
             (DataContext as SettingsViewModel)?.UpdateAdvancedModeToggle(AdvancedToggle.IsOn);
        }
    }
}