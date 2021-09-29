using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Syn3Updater.ViewModels;
using Syn3Updater.Views;

namespace Syn3Updater
{
    public class App : Application
    {
        public override void Initialize()
        {
            
            AppMan.App.Initialize();
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }
            
            var theme = new Avalonia.Themes.Default.DefaultTheme();
            theme.TryGetResource("Button", out _);

            //var theme1 = new Avalonia.Themes.Fluent.FluentTheme();
            //theme1.TryGetResource("Button", out _);
            IconProvider.Register<FontAwesomeIconProvider>();
            base.OnFrameworkInitializationCompleted();
        }
    }
}
