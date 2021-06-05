using System;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Cyanlabs.Syn3Updater.Model;
using Microsoft.Win32;
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
        ///
        /// </summary>
        /// <param name="content"></param>
        /// <param name="title"></param>
        /// <param name="secondarybutton"></param>
        /// <param name="defaultbutton"></param>
        /// <param name="cancel"></param>
        /// <param name="primarybutton"></param>
        /// <returns></returns>
        public static ContentDialog ShowErrorDialog(string content, string cancel = null)
        {
            ContentDialog contentDialog = new();
            contentDialog.Title = LM.GetValue("String.Error");
            contentDialog.Content = content;
            contentDialog.CloseButtonText = string.IsNullOrEmpty(cancel) ? LM.GetValue("String.OK") : cancel;
            contentDialog.Background = Brushes.DarkRed;
            return contentDialog;
        }

        public static ContentDialog ShowWarningDialog(string content, string title, string cancel, string primarybutton, string secondarybutton = null)
        {
            ContentDialog contentDialog = new();
            contentDialog.Title = title;
            contentDialog.Content = content;
            contentDialog.CloseButtonText = cancel;
            contentDialog.Background = Brushes.DarkOrange;
            if(!string.IsNullOrEmpty(primarybutton)) contentDialog.PrimaryButtonText = primarybutton;
            if(!string.IsNullOrEmpty(secondarybutton)) contentDialog.SecondaryButtonText = secondarybutton;
            return contentDialog;
        }

        public static ContentDialog ShowDialog(string content, string title, string cancel, string primarybutton = null, string secondarybutton = null, ContentDialogButton defaultbutton = ContentDialogButton.None, Brush bg = null)
        {
            ContentDialog contentDialog = new();
            contentDialog.Title = title;
            contentDialog.Content = content;
            contentDialog.CloseButtonText = cancel;
            if(bg != null) contentDialog.Background = bg;
            if(!string.IsNullOrEmpty(primarybutton)) contentDialog.PrimaryButtonText = primarybutton;
            if(defaultbutton != ContentDialogButton.None) contentDialog.DefaultButton = defaultbutton;
            if(!string.IsNullOrEmpty(secondarybutton)) contentDialog.SecondaryButtonText = secondarybutton;
            return contentDialog;
        }
        #endregion
    }
}