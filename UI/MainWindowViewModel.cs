using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Syn3Updater.Model;
using Syn3Updater.UI.Tabs;
using Settings = Syn3Updater.Properties.Settings;

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
                    new TabItem("0xE74C","Home","home"),
                    new TabItem("0xE896","Downloads","downloads"),
                    //TODO Implement Profiles in the future
                    //new TabItem("0xF163","Profiles","profiles"),
                    new TabItem("0xF582","News","news"),
                    new TabItem("0xEBE8","Crash","crashme")
                };

                foreach (TabItem tabItem in ti.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Key)))
                {
                    tabItem.Name = LanguageManager.GetValue("Main." + tabItem.Key, Language);
                }

                TabItems = ti;
            };
            CurrentTab = Settings.Default.DisclaimerAccepted ? "home" : "about";

            ApplicationManager.Instance.ShowDownloadsTab += delegate (object sender, EventArgs args)
            {
                CurrentTab = "downloads";
            };

            ApplicationManager.Instance.ShowHomeTab += delegate (object sender, EventArgs args)
            {
                CurrentTab = "home";
            };
        }

        private bool _hamburgerExtended;

        public bool HamburgerExtended
        {
            get => _hamburgerExtended;
            set => SetProperty(ref _hamburgerExtended, value);
        }

        private bool _showModal;

        public bool ShowModal
        {
            get => _showModal;
            set => SetProperty(ref _showModal, value);
        }

        private string _modalText = "Please Wait";

        public string ModalText
        {
            get => _modalText;
            set => SetProperty(ref _modalText, value);
        }

        private bool _showModalTextBox;

        public bool ShowModalTextBox
        {
            get => _showModalTextBox;
            set => SetProperty(ref _showModalTextBox, value);
        }

        private bool _showModalCloseButton;

        public bool ShowModalCloseButton
        {
            get => _showModalCloseButton;
            set => SetProperty(ref _showModalCloseButton, value);
        }

        private string _currentTab = "home";

        public string CurrentTab
        {
            get => _currentTab;
            set
            {
                if (value != "about" && !Settings.Default.DisclaimerAccepted)
                {
                    MessageBox.Show(LanguageManager.GetValue("MessageBox.DisclaimerNotAccepted"), "Syn3 Updater",MessageBoxButton.OK,MessageBoxImage.Warning);
                }
                else if(value == "home" && (Settings.Default.CurrentSyncRegion == "" || Settings.Default.CurrentSyncVersion == 0))
                {
                    MessageBox.Show(LanguageManager.GetValue("MessageBox.NoSyncVersionOrRegionSelected"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
                    SetProperty(ref _currentTab, "settings");
                }
                else if(value == "home" && ApplicationManager.Instance._downloadfiles.Count != 0)
                {
                    MessageBox.Show(LanguageManager.GetValue("MessageBox.DownloadInProgress"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                //else if (value == "downloads" && ApplicationManager.Instance._downloadfiles.Count == 0)
                //{
                //    MessageBox.Show(LanguageManager.GetValue("MessageBox.NoDownloads"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Information);
                //    SetProperty(ref _currentTab, "home");
                //}
                else if (value == "crashme")
                {
                    int i = 11;
                    i -= 11;
                    Debug.WriteLine(11 / i);
                }
                else
                {
                    SetProperty(ref _currentTab, value);
                }
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

        private int _modalPercentage;
        public int ModalPercentage { get => _modalPercentage; set => SetProperty(ref _modalPercentage, value); }

        private bool _modalShowPercentage;

        public bool ModalShowPercentage
        {
            get => _modalShowPercentage;
            set => SetProperty(ref _modalShowPercentage, value);
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
