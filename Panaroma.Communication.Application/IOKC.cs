namespace Panaroma.Communication.Application
{
    public interface IOKC
    {
        void DoWork();

        void TryReceiptBegin(OKCParameters okcParameters);

        void TryDoTransaction(OKCParameters okcParameters);

        void TryDoBatchTransaction(OKCParameters okcParameters);

        void TryDoPayment(OKCParameters okcParameters);

        void TryReceiptEnd();

        void TryFreePrint(OKCParameters okcParameters);

        void TryFreePrintList(OKCParameters okcParameters);

        void TryPrintZReport();

        void TryPrintLastZReportCopy();

        void TryPrintXReport();

        void TryGetOKCStatus();

        void TryGMP3Pair();

        void TryPing();

        void TryPrintXPLUSaleReport(OKCParameters okcParameters);

        void TryPrintXPLUProgram(OKCParameters okcParameters);

        void TryPrintEkuDetailReport();

        void TryPrintEkuZDetailReport();

        void TryPrintEkuReceiptDetailReportWithDatetime(OKCParameters okcParameters);

        void TryPrintLastSaleReceiptCopy();

        void TryPrintSalesReportWihtZNo(OKCParameters okcParameters);

        void TryPrintBankEOD();

        void TryPrintBankSlipCopy(OKCParameters requestMembers);

        void TryOpenDrawer();

        void TryRestartApp();

        void TryPowerOFF();

        void TrySetEcrConfig(OKCParameters okcParameters);

        void TrySetGroup(OKCParameters okcParameters);
    }
}