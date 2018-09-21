using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Interop;

namespace Panaroma.Communication.Application
{
    public class Win32ClipboardAPI
    {
        [DllImport("User32.dll", SetLastError = true)]
        private static extern IntPtr GetOpenClipboardWindow();
        [DllImport("User32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsClipboardFormatAvailable(uint format);

        [DllImport("User32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        public static extern bool EmptyClipboard();

        [DllImport("User32.dll", SetLastError = true)]
        private static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("user32.dll")]
        public static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("User32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseClipboard();

        private const uint CF_TEXT = 1U;
        private const uint CF_BITMAP = 2U;
        private const uint CF_UNICODETEXT = 13U;

        private const uint GHND = 0x0042;
        public static string GetText()
        {
      
            if(!IsClipboardFormatAvailable(CF_UNICODETEXT))
                return null;

            try
            {
                if(!OpenClipboard(IntPtr.Zero))
                    return null;

                IntPtr handle = GetClipboardData(CF_UNICODETEXT);
                if(handle == IntPtr.Zero)
                    return null;

                IntPtr pointer = IntPtr.Zero;

                try
                {
                    pointer = Win32MemoryAPI.GlobalLock(handle);
                    if(pointer == IntPtr.Zero)
                        return null;

                    int size = Win32MemoryAPI.GlobalSize(handle);
                    byte[] buff = new byte[size];

                    Marshal.Copy(pointer, buff, 0, size);

                    return Encoding.Unicode.GetString(buff).TrimEnd('\0');
                }
                finally
                {
                    if(pointer != IntPtr.Zero)
                        Win32MemoryAPI.GlobalUnlock(handle);
                }
            }
            finally
            {
                CloseClipboard();
            }
        }
        public static bool IsFree()
        {
            return (GetOpenClipboardWindow() == IntPtr.Zero);
        }

        public static bool SetText(string text)
        {
            if(!IsFree())
                return false;

            var data = Encoding.Unicode.GetBytes(text + char.MinValue); // Add null at the tail.

            return SetData(CF_UNICODETEXT, IntPtr.Zero, data);
        }
        private static bool SetData(uint format, IntPtr windowHandle, byte[] data)
        {
            try
            {
                if(!OpenClipboard(windowHandle))
                    return false;

                if(!EmptyClipboard())
                    throw new Win32Exception(Marshal.GetLastWin32Error(), nameof(EmptyClipboard));

                var handle = Win32MemoryAPI.GlobalAlloc(GHND, data.Length);
                if(handle == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error(), nameof(Win32MemoryAPI.GlobalAlloc));

                try
                {
                    var pointer = Win32MemoryAPI.GlobalLock(handle);
                    if(pointer == IntPtr.Zero)
                        throw new Win32Exception(Marshal.GetLastWin32Error(), nameof(Win32MemoryAPI.GlobalLock));

                    Marshal.Copy(data, 0, pointer, data.Length);

                    if(SetClipboardData(format, handle) == IntPtr.Zero)
                        throw new Win32Exception(Marshal.GetLastWin32Error(), nameof(SetClipboardData));

                    return true;
                }
                finally
                {
                    Win32MemoryAPI.GlobalUnlock(handle);
                }
            }
            finally
            {
                CloseClipboard();
            }
        }
    }

}
