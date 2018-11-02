using PCPOSOKC;
using System.Collections.Generic;

namespace Panaroma.OKC.Integration.Library
{
    public interface IOKC
    {
        OKC TryPing();

        OKC TryConnectToCOM();

        OKC TryConnectToEthernet();

        OKC TryCashierLogin(string cashierId, string cashierPassword);

        OKC TryCashierLogin(Cashier cashier);

        OKC TrySetHeader(IDictionary<byte, string> headers);

        OKC TryReceiptBegin(Members requestMembers);

        OKC TryReceiptEnd(Members requestMembers);

        OKC TryDoTransaction(Members requestMembers);

        OKC TryDoBatchTransaction(Members requestMembers);

        OKC TryGetReceiptTotal();

        OKC TryDoPayment(Members requestMembers);

        OKC TryCardInfo(Members requestMembers);

        OKC TryFreePrint(Members requestMembers);

        OKC TryGetVatRates();

        OKC TryReceiptInquiry(Members requestMembers);

        OKC TryChangeEcrMode(EcrModeType ecrModeType);

        OKC TryChangeEcrMode(string ecrMode);

        OKC TrySetEcrConfig(HybridMembers hybridMembers);

        OKC TryRestartApp();

        OKC TrySetExchange(Members requestMembers);

        OKC TryGetExchange(Members requestMembers);

        OKC TrySetPaper(Members requestMembers);

        OKC TryOpenDrawer();

        OKC TryGetDrawerStatus();

        OKC TrySetGroup(Members requestMembers);

        OKC TrySetPLU(Members requestMembers);

        OKC TryGetPLUList(Members requestMembers);

        OKC TryGetBankList();

        OKC TryGetDepartmentList();

        OKC TrySetDepartmentList(Members requestMembers);

        OKC TrySetDepartmentList(params Department[] departments);

        OKC TryGMP3Echo();

        OKC TryGMP3Pair();

        OKC TryPrintZReport();

        OKC TryGetLastZReportSoftCopy();

        OKC TryPrintLastZReportCopy();

        OKC TryPrintXReport();

        OKC TryPrintXPLUSaleReport(Members requestMembers);

        OKC TryPrintZPLUSaleReport(Members requestMembers);

        OKC TryPrintXPLUProgram(Members requestMembers);

        OKC TryPrintEkuDetailReport();

        OKC TryPrintEkuZDetailReport();

        OKC TryPrintEkuReceiptDetailReportWithDatetime(Members requestMembers);

        OKC TryPrintEkuReceiptDetailReportWithZNoAndReceiptNo(Members requestMembers);

        OKC TryPrintFinancalZDetailReportWithDateTime(Members requestMembers);

        OKC TryPrintFinancalZDetailReportWithZNo(Members requestMembers);

        OKC TryPrintFinancalZReportWithDateTime(Members requestMembers);

        OKC TryPrintFinancalZReportWithZNo(Members requestMembers);

        OKC TryPrintLastSaleReceiptCopy();

        OKC TryPrintSalesReportWihtZNo(Members requestMembers);

        OKC TryPrintBankEOD();

        OKC TryPrintBankSlipCopy(Members requestMembers);

        OKC TryDisconnect();

        string TryGetAppVersion();

        string TryGetFromResultCodeToDescription(int okcresult);

        OKC TryPowerOFF();

        OKC TryInfoInquiryResponse(Members requestMembers);

        OKC TryGetOKCStatus();

        OKC TryGetUniqueId();
        void TrySetLogStat(string path = null);

        void SetDefaultProcessInformationAndMembers();
    }
}