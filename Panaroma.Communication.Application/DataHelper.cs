using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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