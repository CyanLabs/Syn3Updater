using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cyanlabs.Syn3Updater.UI.Tabs
{
    /// <summary>
    ///     Interaction logic for Install.xaml
    /// </summary>
    public partial class News
    {
        private Task _updateNotices;

        public News()
        {
            InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this)) (DataContext as NewsViewModel)?.Init();
        }

        private void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!(sender is FlowDocumentScrollViewer) || e.Handled) return;
            e.Handled = true;
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            eventArg.RoutedEvent = UIElement.MouseWheelEvent;
            eventArg.Source = sender;
            var parent = ((Control)sender).Parent as UIElement;
            if (parent != null) parent.RaiseEvent(eventArg);
        }
    }
}