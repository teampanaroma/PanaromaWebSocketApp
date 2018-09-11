using System;

namespace Panaroma.OKC.Integration.Library
{
    public class OKCStatus
    {
        public string AppVersion { get; set; }

        public int CashierId { get; set; }

        public int DocumentType { get; set; }

        public EcrModeType EcrMode { get; set; }

        public string EJNUM { get; set; }

        public string FiscalID { get; set; }

        public string ReceiptNum { get; set; }

        public DateTime TranDate { get; set; }

        public int TranStatus { get; set; }

        public string TranStatusDescription { get; set; }

        public string ZNum { get; set; }

        public bool ReceiptIsOpen { get; set; }

        public bool IsConnected { get; set; }

        public string Amount { get; set; }

        public string PaidAmount { get; set; }

        public string EjCapacityInfo { get; set; }
        public string PaperInfo { get; set; }
        public string EjRemainigCountOfLines { get; set; }
    }
}