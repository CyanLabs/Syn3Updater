using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
// ReSharper disable UnusedParameter.Local

namespace Syn3Updater.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
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