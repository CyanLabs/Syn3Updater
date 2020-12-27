using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using FontAwesome5;
using ModernWpf;
using Syn3Updater.Model;
using Syn3Updater.Properties;
using MessageBox = ModernWpf.MessageBox;

namespace Syn3Updater.UI
{
    public class MainWindowViewModel : LanguageAwareBaseViewModel
    {
        #region Constructors

        public MainWindowViewModel()
        {
            switch (Settings.Default.Theme)
            {
                case "Dark":
                    SourceChord.FluentWPF.ResourceDictionaryEx.GlobalTheme = SourceChord.FluentWPF.ElementTheme.Dark;
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                    break;
                case "Light":
                    SourceChord.FluentWPF.ResourceDictionaryEx.GlobalTheme = SourceChord.FluentWPF.ElementTheme.Light;
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                    break;
                case "System":
                    SourceChord.FluentWPF.ResourceDictionaryEx.GlobalTheme = SourceChord.FluentWPF.ElementTheme.Default;
                    break;
                default:
                    SourceChord.FluentWPF.ResourceDictionaryEx.GlobalTheme = SourceChord.FluentWPF.ElementTheme.Dark;
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                    break;
            }
            ApplicationManager.Instance.LanguageChangedEvent += delegate
            {
                ObservableCollection<TabItem> ti = new ObservableCollection<TabItem>
                {
                    new TabItem(EFontAwesomeIcon.Solid_InfoCircle, "About", "about"),
                    new TabItem(EFontAwesomeIcon.Solid_Home, "Home", "home", true),
                    new TabItem(EFontAwesomeIcon.Solid_Tools, "Utility", "utility"),
                    new TabItem(EFontAwesomeIcon.Solid_Download, "Downloads", "downloads")
                    //TODO Implement Profiles and News in the future
                    //new TabItem("0xF163","Profiles","profiles"),
                    //new TabItem("0xF582","News","news"),
                    //new TabItem("0xEBE8","Crash","crashme")
                };

                foreach (TabItem tabItem in ti.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Key)))
                    tabItem.Name = LanguageManager.GetValue($"Main.{tabItem.Key}", Language);
                TabItems = ti;
            };

            ApplicationManager.Instance.ShowDownloadsTab += delegate { CurrentTab = "downloads"; };
            ApplicationManager.Instance.ShowSettingsTab += delegate { CurrentTab = "settings"; };
            ApplicationManager.Instance.ShowHomeTab += delegate { CurrentTab = "home"; };
            ApplicationManager.Instance.ShowUtilityTab += delegate { CurrentTab = "utility"; };
        }

        #endregion

        #region Properties & Fields

        private string _currentTab = "home";
        private bool _hamburgerExtended;
        private ObservableCollection<TabItem> _tabItems = new ObservableCollection<TabItem>();

        public bool HamburgerExtended
        {
            get => _hamburgerExtended;
            set => SetProperty(ref _hamburgerExtended, value);
        }

        public string CurrentTab
        {
            get => _currentTab;
            set
            {
                if (value != "about" && !Settings.Default.DisclaimerAccepted)
                {
                    ModernWpf.MessageBox.Show(LanguageManager.GetValue("MessageBox.DisclaimerNotAccepted"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
                    value = "about";
                }
                else if (value == "home" && (Settings.Default.CurrentSyncRegion == "" || Settings.Default.CurrentSyncVersion == 0))
                {
                    ModernWpf.MessageBox.Show(LanguageManager.GetValue("MessageBox.NoSyncVersionOrRegionSelected"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
                    value = "settings";
                }
                else if (value != "downloads" && ApplicationManager.Instance.IsDownloading)
                {
                    ModernWpf.MessageBox.Show(LanguageManager.GetValue("MessageBox.DownloadInProgress"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
                    value = "downloads";
                }
                else if (value == "crashme")
                {
                    int i = 11;
                    i -= 11;
                    // ReSharper disable once IntDivisionByZero
                    Debug.WriteLine(11 / i);
                }

                SetProperty(ref _currentTab, value);
                foreach (TabItem tabItem in TabItems) tabItem.IsCurrent = tabItem.Key.ToLower() == value.ToLower();
            }
        }

        public ObservableCollection<TabItem> TabItems
        {
            get => _tabItems;
            set => SetProperty(ref _tabItems, value);
        }

        public class TabItem : LanguageAwareBaseViewModel
        {
            private string _icon;
            private bool _isCurrent;
            private string _key;
            private string _name;

            public TabItem(FontAwesome5.EFontAwesomeIcon icon, string name, string key, bool current = false)
            {
                Icon = icon.ToString();
                Name = name;
                Key = key;
                IsCurrent = current;
            }

            public string Name
            {
                get => _name;
                set => SetProperty(ref _name, value);
            }

            public string Key
            {
                get => _key;
                set => SetProperty(ref _key, value);
            }

            public string Icon
            {
                get => _icon;
                set => SetProperty(ref _icon, value);
            }

            public bool IsCurrent
            {
                get => _isCurrent;
                set => SetProperty(ref _isCurrent, value);
            }
        }

        #endregion
    }
}