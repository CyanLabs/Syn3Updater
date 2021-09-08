using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Cyanlabs.Syn3Updater.Converter
{
    public class SyncHexToAscii
    {
        public static List<string> ConvertPackages( string packageHex)
        {
            List<string> output = new();
            foreach (string package in Regex.Replace(packageHex, "0*0", " ").Split(' '))
            {
                string res = string.Empty ;
                for (int a = 0; a <package.Length ; a += 2)
                {
                    string char2Convert = package.Substring(a, 2);
                    int n = Convert.ToInt32(char2Convert, 16);
                    char c = (char)n;
                    res += c.ToString();
                }
                if (res != string.Empty) output.Add(res);
            }
            return output;
        }

    }
}