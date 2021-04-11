using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

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

        private void News_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && !(bool)e.OldValue && (bool)e.NewValue == true) (DataContext as NewsViewModel)?.Reload();
        }
    }
}