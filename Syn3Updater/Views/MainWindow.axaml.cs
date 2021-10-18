using System.ComponentModel;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Syn3Updater.ViewModels;

// ReSharper disable UnusedParameter.Local

namespace Syn3Updater.Views
{
    public class MainWindow : Window
    {
        private ToggleButton darkThemeToggleButton;
        private ToggleButton defaultStyleToggleButton;

        private bool isDefaultStyle = false;
        private bool isDarkTheme = false;
        public MainWindow()
        {
            this.InitializeComponent();

            #if DEBUG
            this.AttachDevTools();
            #endif


            // Do not use a custom title bar on Linux, because there are too many possible options.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) == true)
            {
                UseNativeTitleBar();
            }

            this.DataContext = new MainWindowViewModel(this);

            this.Height = 400;
            this.Width = 600;
            this.Padding = new Thickness(
                            this.OffScreenMargin.Left,
                            this.OffScreenMargin.Top,
                            this.OffScreenMargin.Right,
                            this.OffScreenMargin.Bottom);

            darkThemeToggleButton = this.FindControl<ToggleButton>("DarkThemeToggleButton");
            darkThemeToggleButton.Checked += SetDarkTheme;
            darkThemeToggleButton.Unchecked += SetLightTheme;

            defaultStyleToggleButton = this.FindControl<ToggleButton>("DefaultStyleToggleButton");
            defaultStyleToggleButton.Checked += SetDefaultTheme;
            defaultStyleToggleButton.Unchecked += SetFluentTheme;

            Application.Current.Styles[1] = App.FluentLight;
        }

        private void SetLightTheme(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Cursor = new Cursor(StandardCursorType.Wait);
            Application.Current.Styles[1] = isDefaultStyle ? App.DefaultLight : App.FluentLight;
            Application.Current.Resources["MacOsTitleBarBackground"] = new SolidColorBrush { Color = new Color(255, 222, 225, 230) };
            Application.Current.Resources["MacOsWindowTitleColor"] = new SolidColorBrush { Color = new Color(255, 77, 77, 77) };
            Cursor = new Cursor(StandardCursorType.Arrow);
            isDarkTheme = false;
        }

        private void SetDarkTheme(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Cursor = new Cursor(StandardCursorType.Wait);
            Application.Current.Styles[1] = isDefaultStyle ? App.DefaultDark : App.FluentDark;
            Application.Current.Resources["MacOsTitleBarBackground"] = new SolidColorBrush { Color = new Color(255, 62, 62, 64) };
            Application.Current.Resources["MacOsWindowTitleColor"] = new SolidColorBrush { Color = new Color(255, 153, 158, 161) };
            Cursor = new Cursor(StandardCursorType.Arrow);
            isDarkTheme = true;
        }

        private void SetDefaultTheme(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Cursor = new Cursor(StandardCursorType.Wait);
            Application.Current.Styles[1] = isDarkTheme ? App.DefaultDark : App.DefaultLight;
            Cursor = new Cursor(StandardCursorType.Arrow);
            isDefaultStyle = true;
        }

        private void SetFluentTheme(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Cursor = new Cursor(StandardCursorType.Wait);
            Application.Current.Styles[1] = isDarkTheme ? App.FluentDark : App.FluentLight;
            Cursor = new Cursor(StandardCursorType.Arrow);
            isDefaultStyle = false;
        }

        private void UseNativeTitleBar()
        {
            ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.SystemChrome;
            ExtendClientAreaTitleBarHeightHint = -1;
            ExtendClientAreaToDecorationsHint = false;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Window_OnClosing(object? sender, CancelEventArgs cancelEventArgs)
        {
            AppMan.Exit();
        }
    }
}