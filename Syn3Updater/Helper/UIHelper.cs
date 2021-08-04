using System;
using System.Threading;
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
            try 
            {
                SequentialDialog contentDialog = new()
                {
                    Title = LM.GetValue("String.Error"),
                    Content = content,
                    CloseButtonText = string.IsNullOrEmpty(cancel) ? LM.GetValue("String.OK") : cancel,
                    Background = Brushes.DarkRed,
                    Foreground = Brushes.White
                };
                await DialogManager.OpenDialogAsync(contentDialog, true);
            }
            catch (InvalidOperationException)
            {

            }
        }

        public static async Task<ContentDialogResult> ShowWarningDialog(string content, string title, string cancel, string primarybutton, string secondarybutton = null)
        {
            try
            {
                SequentialDialog contentDialog = new()
                {
                    Title = title,
                    Content = content,
                    CloseButtonText = cancel,
                    Background = Brushes.DarkGoldenrod
                };
                if (!string.IsNullOrEmpty(primarybutton)) contentDialog.PrimaryButtonText = primarybutton;
                if (!string.IsNullOrEmpty(secondarybutton)) contentDialog.SecondaryButtonText = secondarybutton;
                return await DialogManager.OpenDialogAsync(contentDialog, true);
            }
            catch (InvalidOperationException)
            {
                return ContentDialogResult.None;
            }
        }

        public static async Task<ContentDialogResult> ShowDialog(string content, string title, string cancel, string primarybutton = null, string secondarybutton = null,
            ContentDialogButton defaultbutton = ContentDialogButton.None, Brush bg = null)
        {
            SequentialDialog  contentDialog = new()
            {
                Title = title,
                Content = content,
                CloseButtonText = cancel,
                
            };
            if (bg != null)
            {
                contentDialog.Background = bg;
                contentDialog.Foreground = Brushes.White;
            }
            if (!string.IsNullOrEmpty(primarybutton)) contentDialog.PrimaryButtonText = primarybutton;
            if (defaultbutton != ContentDialogButton.None) contentDialog.DefaultButton = defaultbutton;
            if (!string.IsNullOrEmpty(secondarybutton)) contentDialog.SecondaryButtonText = secondarybutton;
            return await DialogManager.OpenDialogAsync(contentDialog, true);
        }
    }

        #endregion
    }
    
    // https://github.com/microsoft/microsoft-ui-xaml/issues/1679#issuecomment-603214112
    public class SequentialDialog : ContentDialog
    {
        public bool IsAborted;
    }
    
    public static class DialogManager
    {
        public static SequentialDialog ActiveDialog;
        private static TaskCompletionSource<bool> _dialogAwaiter = new();

        public static async Task<ContentDialogResult> OpenDialogAsync(SequentialDialog dialog, bool awaitPreviousDialog)
        {
            return await OpenDialog(dialog, awaitPreviousDialog);
        }

        static async Task<ContentDialogResult> OpenDialog(SequentialDialog dialog, bool awaitPreviousDialog)
        {
            TaskCompletionSource<bool> currentAwaiter = _dialogAwaiter;
            TaskCompletionSource<bool> nextAwaiter = new();
            _dialogAwaiter = nextAwaiter;

            if (ActiveDialog != null)
            {
                if (awaitPreviousDialog)
                {
                    await currentAwaiter.Task;
                }
                else
                {
                    ActiveDialog.IsAborted = true;
                    ActiveDialog.Hide();
                }
            }

            ActiveDialog = dialog;
            ContentDialogResult result = await ActiveDialog.ShowAsync();
            nextAwaiter.SetResult(true);
            return result;
        }
    }