using System;

namespace Panaroma.OKC.Integration.Library
{
    public static class ByteExtensions
    {
        public static string ToHexString(this byte[] value)
        {
            return BitConverter.ToString(value).Replace("-", "");
        }
    }
}