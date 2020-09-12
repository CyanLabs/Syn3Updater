using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace Sync3Updater.Helpers
{
    class Functions
    {
        public  static string CalculateMd5(string filename)
        {
            using (var stream = new BufferedStream(File.OpenRead(filename), 100000))
            {
                return BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(stream)).Replace("-", string.Empty);
            }
        }

        public static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0) return "0" + suf[0];

            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return Math.Sign(byteCount) * num + suf[place];
        }

        #region CopyFileEx
       
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CopyFileEx(string lpExistingFileName, string lpNewFileName,
            CopyProgressRoutine lpProgressRoutine, IntPtr lpData, ref int pbCancel,
            CopyFileFlags dwCopyFlags);

        public delegate CopyProgressResult CopyProgressRoutine(long totalFileSize, long totalBytesTransferred,
            long streamSize, long streamBytesTransferred, uint dwStreamNumber, IntPtr hSourceFile,
            IntPtr hDestinationFile, IntPtr lpData);

        [Flags]
        public enum CopyFileFlags : uint
        {
            CopyFileRestartable = 0x00000002
        }

        public enum CopyProgressResult : uint
        {
            ProgressContinue = 0,
            ProgressCancel = 1
        }
        #endregion

        #region Borderless Window Move / Close/Minimize
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        public static extern void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        public static extern void SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);
        #endregion
    }
}
