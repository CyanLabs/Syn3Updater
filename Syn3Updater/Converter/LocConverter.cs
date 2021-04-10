using System;
using System.Globalization;
using System.Windows.Data;
using Cyanlabs.Syn3Updater.Model;

namespace Cyanlabs.Syn3Updater.Converter
{
    [ValueConversion(typeof(string), typeof(string))]
    public class LocConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace((string)value))
                    return LM.GetValue(parameter?.ToString(), value.ToString());
            }
            catch
            {
                // ignored
            }

            return LM.GetValue(parameter?.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    //[ValueConversion(typeof(string), typeof(string))]
    //public class ValueLocConverter : IValueConverter
    //{
    //    #region Methods

    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return LanguageManager.GetValue(value?.ToString().Replace(" ", ""));
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    #endregion
    //}
}