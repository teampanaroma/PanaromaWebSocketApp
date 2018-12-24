using AsyncWindowsClipboard;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace Panaroma.Communication.Application
{
    public class WindowClipboardMonitor : IDisposable
    {
        private readonly IAsyncClipboardService _asyncClipboardService = new WindowsClipboardService(timeout: TimeSpan.FromMilliseconds(100));
        public event EventHandler<string> ClipboardTextChanged;
        private bool disposed = false;
        HwndSource Win32InteropSource;
        IntPtr WindowInteropHandle;

        public WindowClipboardMonitor(Window clipboardWindow)
        {
            InitializeInteropSource(clipboardWindow);
            InitializeWindowInteropHandle(clipboardWindow);

            StartHandlingWin32Messages();
            AddListenerForClipboardWin32Messages();
        }

        private void InitializeInteropSource(Window clipboardWindow)
        {
            var presentationSource = PresentationSource.FromVisual(clipboardWindow);
            Win32InteropSource = presentationSource as HwndSource;

            if(Win32InteropSource == null)
            {
                throw new ArgumentException(
                    $"Window must be initialized before using the {nameof(WindowClipboardMonitor)}. Use the window's OnSourceInitialized() handler if possible, or a later point in the window lifecycle."
                    , nameof(clipboardWindow));
            }
        }

        private void InitializeWindowInteropHandle(Window clipboardWindow)
        {
            WindowInteropHandle = new WindowInteropHelper(clipboardWindow).Handle;
            if(WindowInteropHandle == null)
            {
                throw new ArgumentException(
                    $"{nameof(clipboardWindow)} must be initialized before using the {nameof(WindowClipboardMonitor)}. Use the Window's OnSourceInitialized() handler if possible, or a later point in the window lifecycle."
                    , nameof(clipboardWindow));
            }
        }

        private void StartHandlingWin32Messages()
        {
            Win32InteropSource.AddHook(Win32InteropMessageHandler);
        }

        private void StopHandlingWin32Messages()
        {
            Win32InteropSource.RemoveHook(Win32InteropMessageHandler);
        }

        private void AddListenerForClipboardWin32Messages()
        {
            NativeMethods.AddClipboardFormatListener(WindowInteropHandle);
        }

        private void RemoveListenerForClipboardWin32Messages()
        {
            NativeMethods.RemoveClipboardFormatListener(WindowInteropHandle);
        }

        private IntPtr Win32InteropMessageHandler(IntPtr windowHandle, int messageCode, IntPtr wParam, IntPtr lParam, ref bool messageHandled)
        {
            if(messageCode == NativeMethods.ClipboardUpdateWindowMessageCode)
            {
                OnClipboardChanged();

                messageHandled = true;
                return NativeMethods.HandledClipboardUpdateReturnCode;
            }

            return NativeMethods.NoMessageHandledReturnCode;
        }

        private void OnClipboardChanged()
        {
            ProcessClipboardTextWithRetry();
        }

        private void ProcessClipboardTextWithRetry()
        {
            const int maxAttempts = 10;
            int currentAttemptNumber = 1;

            while(currentAttemptNumber <= maxAttempts)
            {
                try
                {
                    ProcessClipboardText();
                    return;
                }
                catch(COMException ex) when(ex.ErrorCode == NativeMethods.UnableToOpenClipboardComErrorCode)
                {
                    SleepUntilNextRetry(currentAttemptNumber);
                }
                currentAttemptNumber++;
            }
        }

        private void SleepUntilNextRetry(int currentAttemptNumber)
        {
            const int sleepDurationMilliseconds = 50;
            var timeUntilNextRetry = TimeSpan.FromMilliseconds(sleepDurationMilliseconds);
            Thread.Sleep(timeUntilNextRetry);
        }

        private async void ProcessClipboardText()
        {
            if(Clipboard.ContainsText())
            {
                ClipboardTextChanged?.Invoke(this, await _asyncClipboardService.GetTextAsync() );
            }
        }

        public void Dispose()
        {
            ReleaseResources();
            GC.SuppressFinalize(this);
        }

        protected virtual void ReleaseResources()
        {
            if(disposed)
            {
                return;
            }
            else
            {
                disposed = true;
            }

            RemoveListenerForClipboardWin32Messages();
            StopHandlingWin32Messages();

            Win32InteropSource = null;
            WindowInteropHandle = IntPtr.Zero;
        }

        ~WindowClipboardMonitor()
        {
            ReleaseResources();
        }
    }

    internal static class NativeMethods
    {
        public const int WM_CLIPBOARDUPDATE = 0x031D;
        public static IntPtr HWND_MESSAGE = new IntPtr(-3);

        public const int ClipboardUpdateWindowMessageCode = 0x031D;
        public static readonly IntPtr HandledClipboardUpdateReturnCode = IntPtr.Zero;
        public static readonly IntPtr NoMessageHandledReturnCode = IntPtr.Zero;
        private static uint CLIPBRD_E_CANT_OPEN = 0x800401D0;
        public static int UnableToOpenClipboardComErrorCode = (int)CLIPBRD_E_CANT_OPEN;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AddClipboardFormatListener(IntPtr windowHandle);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RemoveClipboardFormatListener(IntPtr windowHandle);

    }
}