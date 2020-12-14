using System.Windows;
using System.Windows.Controls;

namespace Syn3Updater.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel Vm => ((MainWindowViewModel)DataContext);

        public static readonly SimpleLogger Logger = new SimpleLogger();

        public MainWindow()
        {

            InitializeComponent();

            this.Title = "Syn3 Updater " + System.Reflection.Assembly.GetEntryAssembly().GetName().Version;

        }

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if ((sender as Grid).DataContext is MainWindowViewModel.TabItem lvm)
            {
                if (string.IsNullOrWhiteSpace(lvm.Key))
                {
                    Vm.HamburgerExtended = !Vm.HamburgerExtended;
                }
                else
                {
                    Vm.CurrentTab = lvm.Key;
                }
            }
        }

        private void Grid_MouseDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Vm.CurrentTab = "settings";
        }
    }
}
