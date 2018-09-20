using System;
using System.Runtime.InteropServices;

namespace Panaroma.Communication.Application
{
    internal class Win32MemoryAPI
    {
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, int size);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalAlloc(uint uFlags, int dwBytes);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("Kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalFree(IntPtr hMem);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern int GlobalSize(IntPtr hMem);

        public const uint GMEM_DDESHARE = 0x2000;
        public const uint GMEM_MOVEABLE = 0x2;
    }
}
