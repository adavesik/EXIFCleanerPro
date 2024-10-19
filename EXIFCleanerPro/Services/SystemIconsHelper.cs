using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EXIFCleanerPro.Services
{
    internal class SystemIconsHelper
    {
        // Struct to hold shell info for files/folders
        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        public const uint SHGFI_ICON = 0x100;               // Get the icon
        public const uint SHGFI_LARGEICON = 0x0;            // Large icon
        public const uint SHGFI_SMALLICON = 0x1;            // Small icon

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        // Get system icon for the specified path
        public static Icon GetSystemIcon(string path, bool isFolder)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            uint flags = SHGFI_ICON | SHGFI_SMALLICON;

            // Call SHGetFileInfo to get the icon handle
            IntPtr iconHandle = SHGetFileInfo(path, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags);

            // Check if the handle is valid
            if (iconHandle == IntPtr.Zero)
            {
                // Return a default icon or null if the handle is invalid
                return null;
            }

            // If the handle is valid, create an icon
            return Icon.FromHandle(shinfo.hIcon);
        }

    }
}
