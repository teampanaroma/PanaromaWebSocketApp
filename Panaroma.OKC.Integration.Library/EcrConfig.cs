namespace Panaroma.OKC.Integration.Library
{
    public class EcrConfig
    {
        public bool IsDisableCashControl { get; set; }

        public bool IsDisableErrorsMessages { get; set; }

        public bool IsDisableWarningMessages { get; set; }

        public bool IsDisableInformationMessages { get; set; }

        public bool IsDisableReceiptFooterMessages { get; set; }

        public bool IsDisableDepartmentLimitMessages { get; set; }

        public bool IsActiveEArchiveSecondCopy { get; set; }

        public bool IsActiveEInvoiceSecondCopy { get; set; }

        public bool IsActiveCollectionReceiptsSecondCopy { get; set; }

        public bool IsDisableCardInfoRequest { get; set; }

        public bool IsDisableExitFromSaleWithKeyX { get; set; }

        public bool IsDisableExitDeviceInSetMenu { get; set; }

        public bool IsDisableSalesScreenTimeout { get; set; }

        public bool IsEnableRed51 { get; set; }

        public bool IsEnableDiscount { get; set; }
    }
}