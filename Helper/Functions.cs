using System;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Syn3Updater.Helpers
{
    class Functions
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
    }
}
