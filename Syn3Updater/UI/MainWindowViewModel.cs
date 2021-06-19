using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Cyanlabs.Syn3Updater.Helper;
using Cyanlabs.Syn3Updater.Model;
using MahApps.Metro.IconPacks;
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
            try
            {
                string regioninfo = AppMan.Client.GetStringAsync(new Uri("https://api.cyanlabs.net/app/syn3updater/githublatest")).Result;
                if (Version.Parse(regioninfo) > Version.Parse(Assembly.GetEntryAssembly()?.GetName().Version.ToString()!))
                    AppMan.App.Outdated = regioninfo;
            }
            catch (Exception)
            {
                // ignored
            }
            
            switch (AppMan.App.MainSettings.Theme)
            {
                case "Dark":
                    ResourceDictionaryEx.GlobalTheme = ElementTheme.Dark;
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                    ThemeIcon = PackIconVaadinIconsKind.SunOutline;
                    break;
                case "Light":
                    ResourceDictionaryEx.GlobalTheme = ElementTheme.Light;
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                    ThemeIcon = PackIconVaadinIconsKind.SunOutline;
                    break;
                default:
                    ResourceDictionaryEx.GlobalTheme = ElementTheme.Dark;
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                    ThemeIcon = PackIconVaadinIconsKind.SunOutline;
                    break;
            }

            _args = Environment.GetCommandLineArgs();
            AppMan.App.LanguageChangedEvent += delegate
            {
                ObservableCollection<TabItem> ti = new()
                {
                    new TabItem(PackIconVaadinIconsKind.InfoCircle, "About", "about"),
                    new TabItem(PackIconVaadinIconsKind.Home, "Home", "home", true),
                    new TabItem(PackIconVaadinIconsKind.Tools, "Utility", "utility"),
                    new TabItem(PackIconVaadinIconsKind.Download, "Downloads", "downloads"),
                    //new TabItem(EFontAwesomeIcon.Solid_Bug, "Crash", "crashme"),
                    new TabItem(PackIconVaadinIconsKind.Car, "Profiles", "profiles"),
                    new TabItem(PackIconVaadinIconsKind.FileText, "Logs", "logs"),
                    new TabItem(PackIconVaadinIconsKind.Newspaper, "News", "news")
                };

                foreach (TabItem tabItem in ti.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Key)))
                    tabItem.Name = LM.GetValue($"Main.{tabItem.Key}", Language);
                TabItems = ti;

                AppTitle =
                    $"Syn3 Updater {Assembly.GetEntryAssembly()?.GetName().Version} ({AppMan.App.LauncherPrefs.ReleaseTypeInstalled}) - {LM.GetValue("Profiles.CurrentProfile")}: {AppMan.App.MainSettings.Profile}";
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
        private string _currentTab = "home";
        private bool _hamburgerExtended;
        private ObservableCollection<TabItem> _tabItems = new();

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
                if (AppMan.App.Outdated != null)
                {
                    value = "news";
                }
                if (AppMan.App.AppUpdated != 2 && _args.Contains("/updated"))
                {
                    value = "news";
                    AppMan.App.AppUpdated++;
                }
                else if (value != "about" && !AppMan.App.MainSettings.DisclaimerAccepted)
                {
                    UIHelper.ShowDialog(LM.GetValue("MessageBox.DisclaimerNotAccepted"), LM.GetValue("String.Notice"), LM.GetValue("String.OK"));
                    value = "about";
                }
                else if (value == "home" && (AppMan.App.Settings.CurrentRegion?.Length == 0 || AppMan.App.Settings.CurrentVersion == 0 ||
                                             AppMan.App.Settings.CurrentVersion.ToString().Length != 7))
                {
                    UIHelper.ShowDialog(LM.GetValue("MessageBox.NoVersionOrRegionSelected"), LM.GetValue("String.Notice"), LM.GetValue("String.OK"));
                    value = "settings";
                }
                else if (value != "downloads" && AppMan.App.IsDownloading)
                {
                    UIHelper.ShowDialog(LM.GetValue("MessageBox.DownloadInProgress"), LM.GetValue("String.Notice"), LM.GetValue("String.OK"));
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
                    tabItem.IsCurrent = string.Equals(tabItem.Key, value, StringComparison.OrdinalIgnoreCase);

                AppTitle =
                    $"Syn3 Updater {Assembly.GetEntryAssembly()?.GetName().Version} ({AppMan.App.LauncherPrefs.ReleaseTypeInstalled}) - {LM.GetValue("Profiles.CurrentProfile")}: {AppMan.App.MainSettings.Profile}";
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

            public TabItem(PackIconVaadinIconsKind icon, string name, string key, bool current = false)
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

        private PackIconVaadinIconsKind _themeIcon;

        public PackIconVaadinIconsKind ThemeIcon
        {
            get => _themeIcon;
            set => SetProperty(ref _themeIcon, value);
        }

        private string _appTitle;

        public string AppTitle
        {
            get => _appTitle;
            set => SetProperty(ref _appTitle, value);
        }

        #endregion
    }
}