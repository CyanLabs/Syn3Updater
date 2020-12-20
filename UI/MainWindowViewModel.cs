using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Syn3Updater.Model;

namespace Syn3Updater.UI
{
    public class MainWindowViewModel : LanguageAwareBaseViewModel
    {
        public MainWindowViewModel()
        {

            ApplicationManager.Instance.LanguageChangedEvent += delegate (object sender, EventArgs args)
            {
                ObservableCollection<TabItem> ti = new ObservableCollection<TabItem>
                {
                    new TabItem("0xE700","",""),
                    new TabItem("0xE946","About","about"),
                    new TabItem("0xE80F","Home","home" , true),
                    new TabItem("0xE896","Downloads","downloads"),
                    //TODO Implement Profiles and News in the future
                    //new TabItem("0xF163","Profiles","profiles"),
                    //new TabItem("0xF582","News","news"),
                    //new TabItem("0xEBE8","Crash","crashme")
                };

                foreach (TabItem tabItem in ti.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Key)))
                {
                    tabItem.Name = LanguageManager.GetValue("Main." + tabItem.Key, Language);
                }
                TabItems = ti;
            };

            ApplicationManager.Instance.ShowDownloadsTab += delegate { CurrentTab = "downloads"; };
            ApplicationManager.Instance.ShowSettingsTab += delegate { CurrentTab = "settings"; };
            ApplicationManager.Instance.ShowHomeTab += delegate { CurrentTab = "home"; };
        }

        private bool _hamburgerExtended;

        public bool HamburgerExtended
        {
            get => _hamburgerExtended;
            set => SetProperty(ref _hamburgerExtended, value);
        }

        private string _currentTab = "home";

        public string CurrentTab
        {
            get => _currentTab;
            set
            {
                if (value != "about" && !Properties.Settings.Default.DisclaimerAccepted)
                {
                    MessageBox.Show(LanguageManager.GetValue("MessageBox.DisclaimerNotAccepted"), "Syn3 Updater",MessageBoxButton.OK,MessageBoxImage.Warning);
                    value = "about";
                }
                else if(value == "home" && (Properties.Settings.Default.CurrentSyncRegion == "" || Properties.Settings.Default.CurrentSyncVersion == 0))
                {
                    MessageBox.Show(LanguageManager.GetValue("MessageBox.NoSyncVersionOrRegionSelected"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
                    value = "settings";
                }
                else if(value != "downloads" && ApplicationManager.Instance.IsDownloading)
                {
                    MessageBox.Show(LanguageManager.GetValue("MessageBox.DownloadInProgress"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
                    value = "downloads";
                }
                else if (value == "downloads" && ApplicationManager.Instance.IsDownloading == false)
                {
                    MessageBox.Show(LanguageManager.GetValue("MessageBox.NoDownloads"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Information);
                    value = "home";
                }
                else if (value == "crashme")
                {
                    int i = 11;
                    i -= 11;
                    Debug.WriteLine(11 / i);
                }
                SetProperty(ref _currentTab, value);
                foreach (TabItem tabItem in TabItems)
                {
                    tabItem.IsCurrent = tabItem.Key.ToLower() == value.ToLower();
                }
            }
        }

        private ObservableCollection<TabItem> _tabItems = new ObservableCollection<TabItem>();

        public ObservableCollection<TabItem> TabItems
        {
            get => _tabItems;
            set => SetProperty(ref _tabItems, value);
        }

        public class TabItem : LanguageAwareBaseViewModel
        {
            private string _name;
            public string Name { get => _name; set => SetProperty(ref _name, value); }

            private string _key;
            public string Key { get => _key; set => SetProperty(ref _key, value); }

            private string _icon;
            public string Icon { get => _icon; set => SetProperty(ref _icon, value); }

            private bool _isCurrent;
            public bool IsCurrent { get => _isCurrent; set => SetProperty(ref _isCurrent, value); }

            public TabItem(string icon, string name, string key, bool current = false)
            {
                Icon = ((char) (Convert.ToInt32(icon, 16))).ToString();
                Name = name;
                Key = key;
                IsCurrent = current;
            }
        }
    }
}
