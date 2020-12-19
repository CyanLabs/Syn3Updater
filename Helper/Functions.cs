using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Syn3Updater.Helpers
{
    public class Functions
    {
        public static string BytesToString(long byteCount)
        {
            string[] suf = {"B", "KB", "MB", "GB", "TB", "PB", "EB"}; //Longs run out around EB
            if (byteCount == 0) return "0" + suf[0];

            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return Math.Sign(byteCount) * num + suf[place];
        }
    }
}
