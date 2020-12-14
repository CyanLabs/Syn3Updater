using System;
using System.Globalization;
using System.Windows.Data;
using Syn3Updater.Model;

namespace Syn3Updater.Converter
{
    [ValueConversion(typeof(string), typeof(string))]
    public class LocConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace((string)value))
                {
                    return LanguageManager.GetValue(parameter?.ToString(), value.ToString());
                }
            }
            catch
            {
                // ignored
            }

            return LanguageManager.GetValue(parameter?.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(string), typeof(string))]
    public class ValueLocConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LanguageManager.GetValue(value?.ToString().Replace(" ", ""));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}