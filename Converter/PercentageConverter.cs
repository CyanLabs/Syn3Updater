using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Syn3Updater.Helper;

namespace Syn3Updater.Converter
{
    public class PercentageConverter : MarkupExtension, IValueConverter
    {
        private static PercentageConverter _instance;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double res = (value?.ToString()).GetDouble() * (parameter?.ToString()).GetDouble();

                if (res < 1) res = 1;

                return res;
            }
            catch
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new PercentageConverter());
        }
    }


    public class SubtractorConverter : MarkupExtension, IValueConverter
    {
        private static SubtractorConverter _instance;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return (value?.ToString()).GetDouble() - (parameter?.ToString()).GetDouble();
            }
            catch
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new SubtractorConverter());
        }
    }



    public class MarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || (value as double?) == null)
            {
                return new Thickness(0, 0, 0, 0);
            }
            double v = (double)value;


            string param = (string)parameter;
            if (param != null && param.StartsWith("-"))
            {
                param = param.Substring(1);
                v = -v;
            }

            var parts = param?.Split('|');
            param = (parts ?? Array.Empty<string>()).First();

            if (parts != null && parts.Length > 1)
            {
                double amount = parts[1].GetDouble();

                v *= amount;
            }

            switch (param.ToLower())
            {
                case "left": return new Thickness(v, 0, 0, 0);
                case "top": return new Thickness(0, v, 0, 0);
                case "right": return new Thickness(0, 0, v, 0);
                case "bottom": return new Thickness(0, 0, 0, v);

                case "topleft": return new Thickness(v, v, 0, 0);
                case "topright": return new Thickness(0, v, v, 0);
                case "bottomleft": return new Thickness(v, 0, 0, v);
                case "bottomright": return new Thickness(0, 0, v, v);

            }

            return new Thickness(0, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
