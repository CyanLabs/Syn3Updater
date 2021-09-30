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

            IconProvider.Register<FontAwesomeIconProvider>();
            base.OnFrameworkInitializationCompleted();
        }
    }
}
