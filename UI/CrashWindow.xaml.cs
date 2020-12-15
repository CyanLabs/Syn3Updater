using System.Collections.Generic;
using System.Windows;

namespace Syn3Updater.UI
{
    /// <summary>
    /// Interaction logic for CrashWindow.xaml
    /// </summary>
    public partial class CrashWindow : Window
    {
        public CrashWindow()
        {
            InitializeComponent();
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            ApplicationManager.Instance.Exit();
        }

        public class CrashContainer
        {
            public string ErrorName { get; set; }
            public string ErrorLocation { get; set; }
            public List<SimpleLogger.LogEntry> Logs { get; set; } = new List<SimpleLogger.LogEntry>();
        }
    }
}
