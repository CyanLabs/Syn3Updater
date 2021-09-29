using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Syn3Updater.ViewModels;

namespace Syn3Updater.Pages
{
    public class HomePage : UserControl
    {
        public HomePage()
        {
            InitializeComponent();
            DataContext = new HomePageViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
