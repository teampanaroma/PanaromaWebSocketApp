using System.Text;

namespace Panaroma.OKC.Integration.Library
{
    public static class StringExtensions
    {
        public static string BuildReceiptEndMessage(this string receiptEndMessage)
        {
            return Encoding.GetEncoding("ISO-8859-9").GetBytes(receiptEndMessage).ToHexString();
        }

        public static decimal ConvertToVerifoneDecimal(this string amount)
        {
            string.IsNullOrEmpty(amount);
            return decimal.Zero;
        }
    }
}