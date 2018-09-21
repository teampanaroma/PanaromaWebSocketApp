namespace Panaroma.Communication.Application
{
    public class DataHelper
    {
        public static bool EmptyClipboard()
        {
            return Win32ClipboardAPI.EmptyClipboard();
        }
    }
}