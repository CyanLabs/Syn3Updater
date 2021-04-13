using System;
using System.Globalization;

namespace Cyanlabs.Syn3Updater.Helper
{
    /// <summary>
    ///     Helper class for various extra math methods
    /// </summary>
    public static class MathHelper
    {
        #region Methods

        public static double GetDouble(this string value, double defaultValue = 0)
        {
            //Try parsing in the current culture
            if (!double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out double result) &&
                //Then try in US english
                !double.TryParse(value, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out result) &&
                //Then in neutral language
                !double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                result = defaultValue;

            return result;
        }

        public static double Clamp(double value, double min, double max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        public static float Clamp(float value, float min, float max)
        {
            return (float)Clamp((double)value, min, max);
        }

        public static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        public static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0) return $"0{suf[0]}";

            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return Math.Sign(byteCount) * num + suf[place];
        }

        public static bool IsOdd(int value)
        {
            return value % 2 != 0;
        }

        #endregion
    }
}