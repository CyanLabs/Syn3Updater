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
        public News()
        {
            InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this)) (DataContext as NewsViewModel)?.Init();
        }

        private void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!(sender is FlowDocumentScrollViewer) || e.Handled) return;
            e.Handled = true;
            MouseWheelEventArgs eventArg = new(e.MouseDevice, e.Timestamp, e.Delta) {RoutedEvent = MouseWheelEvent, Source = sender};
            if (((Control) sender).Parent is UIElement parent) parent.RaiseEvent(eventArg);
        }
    }
}