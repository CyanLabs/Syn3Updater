using System.ComponentModel;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Syn3Updater.UI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Methods
        public MainWindow()
        {
            InitializeComponent();
            Title = $"Syn3 Updater {Assembly.GetEntryAssembly()?.GetName().Version}";
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ApplicationManager.Logger.Debug("MainWindow Initialized");
            if (CryptoConfig.AllowOnlyFipsAlgorithms)
            {
                MessageBox.MessageBox.Show("Syn3 Updater has detected that 'Use FIPS Compliant algorithms for encryption, hashing, and signing.' is enforced via Group Policy, Syn3 Updater will be unable to validate any files using MD5 with this policy enforced\n\nThe application will now close!", "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Error);
                ApplicationManager.Instance.Exit();
            }
        }

        private MainWindowViewModel Vm => (MainWindowViewModel) DataContext;

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as Grid)?.DataContext is MainWindowViewModel.TabItem lvm)
            {
                if (string.IsNullOrWhiteSpace(lvm.Key))
                    Vm.HamburgerExtended = !Vm.HamburgerExtended;
                else
                    Vm.CurrentTab = lvm.Key;
            }
        }

        private void Grid_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            Vm.CurrentTab = "settings";
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ApplicationManager.Instance.Exit();
        }

        #endregion
    }
}