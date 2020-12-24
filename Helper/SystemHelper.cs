﻿using System;
using System.Management;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Syn3Updater.Helper
{
    /// <summary>
    ///     Class containing methods to retrieve specific file system paths.
    ///     https://stackoverflow.com/a/21953690
    /// </summary>
    public static class SystemHelper
    {
        #region Properties & Fields

        [DllImport("Shell32.dll")]
        private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);

        [Flags]
        private enum KnownFolderFlags : uint
        {
            DontVerify = 0x00004000,
        }

        private static readonly string[] KnownFolderGuids =
        {
            "{56784854-C6CB-462B-8169-88E350ACB882}", // Contacts
            "{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}", // Desktop
            "{FDD39AD0-238F-46AF-ADB4-6C85480369C7}", // Documents
            "{374DE290-123F-4565-9164-39C4925E467B}", // Downloads
            "{1777F761-68AD-4D8A-87BD-30B759FA33DD}", // Favorites
            "{BFB9D5E0-C6A9-404C-B2B2-AE6DB6AF4968}", // Links
            "{4BD8D571-6D19-48D3-BE97-422220080E43}", // Music
            "{33E28130-4E1E-4676-835A-98395C3BC3BB}", // Pictures
            "{4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4}", // SavedGames
            "{7D1D3A04-DEBB-4115-95CF-2F29DA2920DA}", // SavedSearches
            "{18989B1D-99B5-455B-841C-AB7C74E4DDFC}" // Videos
        };

        public enum KnownFolder
        {
            Desktop,
            Downloads,
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets the current path to the specified known folder as currently configured. This does
        ///     not require the folder to be existent.
        /// </summary>
        /// <param name="knownFolder">The known folder which current path will be returned.</param>
        /// <returns>The default path of the known folder.</returns>
        /// <exception cref="System.Runtime.InteropServices.ExternalException">
        ///     Thrown if the path
        ///     could not be retrieved.
        /// </exception>
        public static string GetPath(KnownFolder knownFolder)
        {
            return GetPath(knownFolder, false);
        }

        /// <summary>
        ///     Gets the current path to the specified known folder as currently configured. This does
        ///     not require the folder to be existent.
        /// </summary>
        /// <param name="knownFolder">The known folder which current path will be returned.</param>
        /// <param name="defaultUser">
        ///     Specifies if the paths of the default user (user profile
        ///     template) will be used. This requires administrative rights.
        /// </param>
        /// <returns>The default path of the known folder.</returns>
        /// <exception cref="System.Runtime.InteropServices.ExternalException">
        ///     Thrown if the path
        ///     could not be retrieved.
        /// </exception>
        public static string GetPath(KnownFolder knownFolder, bool defaultUser)
        {
            return GetPath(knownFolder, KnownFolderFlags.DontVerify, defaultUser);
        }

        private static string GetPath(KnownFolder knownFolder, KnownFolderFlags flags, bool defaultUser)
        {
            int result = SHGetKnownFolderPath(new Guid(KnownFolderGuids[(int) knownFolder]), (uint) flags, new IntPtr(defaultUser ? -1 : 0), out IntPtr outPath);
            if (result >= 0)
            {
                string path = Marshal.PtrToStringUni(outPath);
                Marshal.FreeCoTaskMem(outPath);
                return path;
            }

            throw new ExternalException("Unable to retrieve the known folder path. It may not " + "be available on this system.", result);
        }

        public static string GetOsFriendlyName()
        {
            string result = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
            foreach (ManagementBaseObject o in searcher.Get())
            {
                ManagementObject os = (ManagementObject) o;
                result = os["Caption"].ToString();
                break;
            }

            return $"{result} ({Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId", "")})";
        }

        #endregion
    }
}