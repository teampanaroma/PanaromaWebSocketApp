using System;
using System.Runtime.InteropServices;

namespace Panaroma.Communication.Application
{
    internal class Win32MemoryAPI
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalAlloc(uint uFlags, int dwBytes);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("Kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern int GlobalSize(IntPtr hMem);
    }
}