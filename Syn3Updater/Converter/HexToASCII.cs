using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cyanlabs.Syn3Updater.Converter
{
    public static class SyncHexToAscii
    {
        public static List<string> ConvertPackages( string packageHex)
        {
            string res = string.Empty;
            for (int a = 0; a <packageHex.Length ; a += 2)
            {
                string char2Convert = packageHex.Substring(a, 2);
                if (char2Convert == "00") 
                {
                    res += "_";
                }
                else
                {
                    int n = Convert.ToInt32(char2Convert, 16);
                    char c = (char)n;
                    res += c.ToString();
                }
            }
            return Regex.Replace(res, "_*_", "_").Split('_').ToList();
        }
    }
}