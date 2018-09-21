using PCPOSOKC;
using System.Collections.Generic;

namespace Panaroma.OKC.Integration.Library
{
    public class ReceiptInquiryResponse
    {
        public string Amount { get; set; }
        public string TransStatus { get; set; }
        public string CashPaymentsTotal { get; set; }
        public string OtherPaymentsTotal { get; set; }
        public int CreditCardPaymentCount { get; set; }

        public List<Dictionary<int, CreditPaymentResultTable>> CreditCardPayments { get; set; } =
            new List<Dictionary<int, CreditPaymentResultTable>>();

        public ReceiptInquiryResponse SetResponse(Members members)
        {
            ReceiptInquiryResponse receiptInquiryResponse = new ReceiptInquiryResponse()
            {
                Amount = members.Amount,
                TransStatus = members.TranStatus,
                CashPaymentsTotal = members.CashPaymentsTotal,
                CreditCardPaymentCount = members.CreditCardPaymentCnt,
                OtherPaymentsTotal = members.OtherPaymentsTotal
            };
            if(receiptInquiryResponse.CreditCardPaymentCount == 0)
                return receiptInquiryResponse;
            int num = 1;
            foreach(CreditPaymentResultTable paymentResultTable in members.CreditPaymentResult)
            {
                if(paymentResultTable.AcqId != null)
                {
                    receiptInquiryResponse.CreditCardPayments.Add(new Dictionary<int, CreditPaymentResultTable>()
                    {
                        {
                            num,
                            paymentResultTable
                        }
                    });
                    ++num;
                }
                else
                    break;
            }

            return receiptInquiryResponse;
        }
    }
}