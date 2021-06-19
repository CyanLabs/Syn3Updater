using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Cyanlabs.Syn3Updater.Model;
using ModernWpf.Controls;

namespace Cyanlabs.Syn3Updater.Helper
{
    /// <summary>
    ///     Helper class containing methods to retrieve specific file system paths.
    ///     https://stackoverflow.com/a/21953690
    /// </summary>
    public static class UIHelper
    {
        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="content"></param>
        /// <param name="title"></param>
        /// <param name="secondarybutton"></param>
        /// <param name="defaultbutton"></param>
        /// <param name="cancel"></param>
        /// <param name="primarybutton"></param>
        /// <returns></returns>
        public static async Task ShowErrorDialog(string content, string cancel = null)
        {
            ContentDialog contentDialog = new()
            {
                Title = LM.GetValue("String.Error"),
                Content = content,
                CloseButtonText = string.IsNullOrEmpty(cancel) ? LM.GetValue("String.OK") : cancel,
                Background = Brushes.DarkRed
            };
            await Application.Current.Dispatcher.BeginInvoke(() => contentDialog.ShowAsync());
        }

        public static async Task<ContentDialogResult> ShowWarningDialog(string content, string title, string cancel, string primarybutton, string secondarybutton = null)
        {
            ContentDialog contentDialog = new()
            {
                Title = title,
                Content = content,
                CloseButtonText = cancel,
                Background = Brushes.DarkOrange
            };
            if (!string.IsNullOrEmpty(primarybutton)) contentDialog.PrimaryButtonText = primarybutton;
            if (!string.IsNullOrEmpty(secondarybutton)) contentDialog.SecondaryButtonText = secondarybutton;
            return await Application.Current.Dispatcher.Invoke(() => contentDialog.ShowAsync());
        }

        public static async Task<ContentDialogResult> ShowDialog(string content, string title, string cancel, string primarybutton = null, string secondarybutton = null,
            ContentDialogButton defaultbutton = ContentDialogButton.None, Brush bg = null)
        {
            ContentDialog contentDialog = new()
            {
                Title = title,
                Content = content,
                CloseButtonText = cancel
            };
            if (bg != null) contentDialog.Background = bg;
            if (!string.IsNullOrEmpty(primarybutton)) contentDialog.PrimaryButtonText = primarybutton;
            if (defaultbutton != ContentDialogButton.None) contentDialog.DefaultButton = defaultbutton;
            if (!string.IsNullOrEmpty(secondarybutton)) contentDialog.SecondaryButtonText = secondarybutton;
            return await Application.Current.Dispatcher.Invoke(() => contentDialog.ShowAsync());
        }

        #endregion
    }
}