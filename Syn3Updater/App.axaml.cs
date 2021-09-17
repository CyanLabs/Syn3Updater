using System.Net.Http.Headers;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
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
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
            {
                singleViewLifetime.MainView = new MainView
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
