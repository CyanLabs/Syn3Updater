using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using Cyanlabs.Syn3Updater.Model;
using FontAwesome5;
using ModernWpf;
using ElementTheme = SourceChord.FluentWPF.ElementTheme;
using ResourceDictionaryEx = SourceChord.FluentWPF.ResourceDictionaryEx;

namespace Cyanlabs.Syn3Updater.UI
{
    public class MainWindowViewModel : LanguageAwareBaseViewModel
    {
        #region Constructors

        public MainWindowViewModel()
        {
            AppTitle = $"Syn3 Updater {Assembly.GetEntryAssembly()?.GetName().Version}";
            switch (AppMan.App.Settings.Theme)
            {
                case "Dark":
                    ResourceDictionaryEx.GlobalTheme = ElementTheme.Dark;
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                    ThemeIcon = EFontAwesomeIcon.Solid_Sun;
                    break;
                case "Light":
                    ResourceDictionaryEx.GlobalTheme = ElementTheme.Light;
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                    ThemeIcon = EFontAwesomeIcon.Solid_Sun;
                    break;
                default:
                    ResourceDictionaryEx.GlobalTheme = ElementTheme.Dark;
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                    ThemeIcon = EFontAwesomeIcon.Solid_Sun;
                    break;
            }

            _args = Environment.GetCommandLineArgs();
            AppMan.App.LanguageChangedEvent += delegate
            {
                ObservableCollection<TabItem> ti = new ObservableCollection<TabItem>
                {
                    new TabItem(EFontAwesomeIcon.Solid_InfoCircle, "About", "about"),
                    new TabItem(EFontAwesomeIcon.Solid_Home, "Home", "home", true),
                    new TabItem(EFontAwesomeIcon.Solid_Tools, "Utility", "utility"),
                    new TabItem(EFontAwesomeIcon.Solid_Download, "Downloads", "downloads"),
                    //new TabItem(EFontAwesomeIcon.Solid_Bug, "Crash", "crashme"),
                    //TODO Implement Profiles and News in the future
                    //new TabItem("0xF163","Profiles","profiles"),
                    new TabItem(EFontAwesomeIcon.Solid_Newspaper,"News","news"),
                };

                foreach (TabItem tabItem in ti.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Key)))
                    tabItem.Name = LM.GetValue($"Main.{tabItem.Key}", Language);
                TabItems = ti;
            };

            AppMan.App.ShowDownloadsTab += delegate { CurrentTab = "downloads"; };
            AppMan.App.ShowSettingsTab += delegate { CurrentTab = "settings"; };
            AppMan.App.ShowHomeTab += delegate { CurrentTab = "home"; };
            AppMan.App.ShowUtilityTab += delegate { CurrentTab = "utility"; };
            AppMan.App.ShowNewsTab += delegate { CurrentTab = "news"; };
        }

        #endregion

        #region Properties & Fields

        private readonly string[] _args;
        private int _newsvisited = 0;
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
                if (AppMan.App.AppUpdated != 2 && _args.Contains("/updated"))
                {
                    value = "news";
                    AppMan.App.AppUpdated++;
                }
                else if (value != "about" && !AppMan.App.Settings.DisclaimerAccepted)
                {
                    ModernWpf.MessageBox.Show(LM.GetValue("MessageBox.DisclaimerNotAccepted"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
                    value = "about";
                }
                else if (value == "home" && (AppMan.App.Settings.CurrentRegion?.Length == 0 || AppMan.App.Settings.CurrentVersion == 0 || AppMan.App.Settings.CurrentVersion.ToString().Length != 7))
                {
                    ModernWpf.MessageBox.Show(LM.GetValue("MessageBox.NoVersionOrRegionSelected"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
                    value = "settings";
                }
                else if (value != "downloads" && AppMan.App.IsDownloading)
                {
                    ModernWpf.MessageBox.Show(LM.GetValue("MessageBox.DownloadInProgress"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                foreach (TabItem tabItem in TabItems)
                    tabItem.IsCurrent = string.Equals(tabItem.Key, value, System.StringComparison.OrdinalIgnoreCase);
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

            public TabItem(EFontAwesomeIcon icon, string name, string key, bool current = false)
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

        private string _appTitle;

        public string AppTitle
        {
            get => _appTitle;
            set => SetProperty(ref _appTitle, value);
        }

        private EFontAwesomeIcon _themeIcon;
        public EFontAwesomeIcon ThemeIcon
        {
            get => _themeIcon;
            set => SetProperty(ref _themeIcon, value);
        }

        #endregion
    }
}