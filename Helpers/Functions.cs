using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Syn3Updater.Helpers
{
    class Functions
    {
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

        public void HideCheckbox(ListView lvw, ListViewItem item)
        {
            var lviItem = new LVITEM();
            lviItem.iItem = item.Index;
            lviItem.mask = LVIF_STATE;
            lviItem.stateMask = LVIS_STATEIMAGEMASK;
            lviItem.state = 0;
            SendMessage(lvw.Handle, LVM_SETITEM, IntPtr.Zero, ref lviItem);
        }

        public const int LVIF_STATE = 0x8;
        public const int LVIS_STATEIMAGEMASK = 0xF000;
        public const int LVM_FIRST = 0x1000;
        public const int LVM_SETITEM = LVM_FIRST + 76;

        // suppress warnings for interop
#pragma warning disable 0649
        public struct LVITEM
        {
            public int mask;
            public int iItem;
            public int iSubItem;
            public int state;
            public int stateMask;
            [MarshalAs(UnmanagedType.LPTStr)]
            public String lpszText;
            public int cchTextMax;
            public int iImage;
            public IntPtr iParam;
        }
#pragma warning restore 0649

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref LVITEM lParam);
    }
}
