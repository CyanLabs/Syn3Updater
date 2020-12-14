using System;
using System.Globalization;

namespace Syn3Updater.Helper
{
    public static class MathHelper
    {
        #region Methods
        public static double GetDouble(this string value, double defaultValue = 0)
        {
            double result;

            //Try parsing in the current culture
            if (!double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.CurrentCulture, out result) &&
                //Then try in US english
                !double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out result) &&
                //Then in neutral language
                !double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            {
                result = defaultValue;
            }

            return result;
        }
        public static double Clamp(double value, double min, double max) => Math.Max(min, Math.Min(max, value));
        public static float Clamp(float value, float min, float max) => (float)Clamp((double)value, min, max);
        public static int Clamp(int value, int min, int max) => Math.Max(min, Math.Min(max, value));

        #endregion
    }
}
