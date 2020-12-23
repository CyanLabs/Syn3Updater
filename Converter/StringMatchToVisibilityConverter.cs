using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Syn3Updater.Converter
{
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class StringMatchToVisibilityConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value?.ToString() == parameter?.ToString()) return Visibility.Visible;

            return Visibility.Collapsed;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as Visibility? == Visibility.Visible;
        }

        #endregion
    }

    [ValueConversion(typeof(string), typeof(Visibility))]
    public class InvertedStringMatchToVisibilityConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value?.ToString() != parameter?.ToString()) return Visibility.Visible;

            return Visibility.Collapsed;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as Visibility? == Visibility.Visible;
        }

        #endregion
    }
}