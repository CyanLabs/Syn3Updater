using System.Windows;
using System.Windows.Controls;

namespace Syn3Updater.UI
{
    public class TextBoxAttachedProperties
    {
        #region Methods

        // Using a DependencyProperty as the backing store for AutoScrollToEnd.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoScrollToEndProperty = DependencyProperty.RegisterAttached("AutoScrollToEnd", typeof(bool), typeof(TextBoxAttachedProperties),
            new PropertyMetadata(false, AutoScrollToEndPropertyChanged));

        public static bool GetAutoScrollToEnd(DependencyObject obj)
        {
            return (bool) obj.GetValue(AutoScrollToEndProperty);
        }

        public static void SetAutoScrollToEnd(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollToEndProperty, value);
        }

        private static void AutoScrollToEndPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textbox && e.NewValue is bool mustAutoScroll && mustAutoScroll)
                textbox.TextChanged += (s, ee) => AutoScrollToEnd(textbox);
        }

        private static void AutoScrollToEnd(TextBox textbox)
        {
            textbox.ScrollToEnd();
        }

        #endregion
    }
}