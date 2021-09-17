using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Syn3Updater.ViewModels;

namespace Syn3Updater.Pages
{
    public class DownloadPage : UserControl
    {
        public DownloadPage()
        {
            InitializeComponent();
            DataContext = new DownloadPageViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
