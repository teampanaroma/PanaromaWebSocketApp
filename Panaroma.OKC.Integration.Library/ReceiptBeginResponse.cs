using System;

namespace Panaroma.OKC.Integration.Library
{
    public class ReceiptBeginResponse
    {
        public string UniqenNumber { get; set; }
        public string ReceiptNumber { get; set; }
        public DateTime TranDate { get; set; }
        public string ZNum { get; set; }
    }
}