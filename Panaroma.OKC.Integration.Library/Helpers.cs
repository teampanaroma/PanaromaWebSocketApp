using PCPOSOKC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Panaroma.OKC.Integration.Library
{
    public class Helpers
    {
        public static class Conditional
        {
            public static void SetExceptionInformation(ref ProcessInformation processInformation, Exception exception)
            {
                processInformation.HasError = true;
                processInformation.InformationMessages.InformationMessageType = InformationMessageType.FATAL;
                processInformation.InformationMessages.Exception = new PCPOSOKCException(exception.Message, exception);
                processInformation.InformationMessages.Message = exception.Message;
                processInformation.InformationMessages.Code = new int?(-200);
            }

            public static void SetCustomWarningInformation(ref ProcessInformation processInformation, string message)
            {
                processInformation.HasError = true;
                processInformation.InformationMessages.InformationMessageType = InformationMessageType.WARNING;
                processInformation.InformationMessages.Message = message;
                processInformation.InformationMessages.Code = new int?(-100);
            }

            public static void SetOKCWarningInformation(ref ProcessInformation processInformation, int code)
            {
                processInformation.HasError = true;
                processInformation.InformationMessages.InformationMessageType = InformationMessageType.WARNING;
                processInformation.InformationMessages.Code = new int?(code);
                processInformation.InformationMessages.Message =
                    string.Format("{0} Hata kodu: {1}", EcrInterface.GetErrorExplain(code), code);
            }

            public static void SetSuccessInformation(ref ProcessInformation processInformation)
            {
                processInformation.InformationMessages.Code = new int?(0);
                processInformation.InformationMessages.Message = "Başarılı";
            }

            public static void SetSuccessInformation(ref ProcessInformation processInformation, object message)
            {
                processInformation.InformationMessages.Code = new int?(0);
                processInformation.InformationMessages.Message = message;
            }
        }

        public static class ReceiptHelper
        {
            public static void CheckReceiptEndParameters(string format, string textPosition, string receiptEndMessage)
            {
                if(string.IsNullOrEmpty(receiptEndMessage))
                {
                    throw new ArgumentNullException("receiptEndMessage",
                        string.Format("{0} boş geçilemez.", "receiptEndMessage"));
                }

                if(string.IsNullOrEmpty(format))
                {
                    throw new ArgumentNullException("format", string.Format("{0} boş geçilemez.", "format"));
                }

                if(format.Length > 2)
                {
                    throw new ArgumentException(string.Format("{0} uzunluğu 2 karakterden uzun olamaz.", "format"));
                }

                if(string.IsNullOrEmpty(textPosition))
                {
                    throw new ArgumentNullException("textPosition",
                        string.Format("{0} boş geçilemez.", "textPosition"));
                }

                if(textPosition.Length > 2)
                {
                    throw new ArgumentException(
                        string.Format("{0} uzunluğu 2 karakterden uzun olamaz.", "textPosition"));
                }
            }

            public static DateTime MergeDateAndTime(string tranDate, string tranTime)
            {
                if(string.IsNullOrEmpty(tranDate))
                    return DateTime.MinValue;
                int year = int.Parse(tranDate.Substring(0, 2));
                int month = int.Parse(tranDate.Substring(2, 2));
                int day = int.Parse(tranDate.Substring(4, 2));
                if(string.IsNullOrEmpty(tranTime))
                    return new DateTime(year, month, day);
                int hour = int.Parse(tranTime.Substring(0, 2));
                int minute = int.Parse(tranTime.Substring(2, 2));
                int second = int.Parse(tranTime.Substring(4, 2));
                return new DateTime(year, month, day, hour, minute, second);
            }

            public static string GetStatusDescription(string tranStatus)
            {
                if(tranStatus == "1")
                    return "Fiş Kapalı";
                if(tranStatus == "3")
                    return "Fiş Açık";
                if(tranStatus == "4")
                    return "Ödeme Bekleniyor";
                if(tranStatus == "5")
                    return "Fiş Bulunamadı";
                return string.Empty;
            }
        }

        private static class ConstValueHelper
        {
            private const int BarcodeLength = 13;
            private const int AmountLength = 12;
            private const int ROrPluLength = 6;
            private const int ZLength = 4;
            private const int ByteLength = 2;
        }

        public static class MembersHelper
        {
            public static void SetDefaultPadLeft(ref Members members)
            {
                if(!string.IsNullOrEmpty(members.Rate) && members.Rate.Length > 0 && members.Rate.Length < 4)
                    members.Rate = members.Rate.PadLeft(4, '0');
                if(!string.IsNullOrEmpty(members.Amount) && members.Amount.Length > 0 && members.Amount.Length < 12)
                    members.Amount = members.Amount.PadLeft(12, '0');
                if(!string.IsNullOrEmpty(members.OriginalAmount) && members.OriginalAmount.Length > 0 &&
                    members.OriginalAmount.Length < 12)
                    members.OriginalAmount = members.OriginalAmount.PadLeft(12, '0');
                if(members.BatchTranItems != null && ((IEnumerable<BatchTranItem>)members.BatchTranItems).Any())
                {
                    for(int index = 0; index < members.BatchTranItems.Length; ++index)
                    {
                        if(members.BatchTranItems[index].Amount != null &&
                            !string.IsNullOrEmpty(members.BatchTranItems[index].Amount))
                            members.BatchTranItems[index].Amount =
                                members.BatchTranItems[index].Amount.PadLeft(12, '0');
                        if(members.BatchTranItems[index].Quantity != null &&
                            !string.IsNullOrEmpty(members.BatchTranItems[index].Quantity))
                            members.BatchTranItems[index].Quantity =
                                members.BatchTranItems[index].Quantity.PadLeft(8, '0');
                        if(members.BatchTranItems[index].Barcode != null &&
                            !string.IsNullOrEmpty(members.BatchTranItems[index].Barcode))
                            members.BatchTranItems[index].Barcode =
                                members.BatchTranItems[index].Barcode.PadLeft(13, '0');
                    }
                }

                if(!string.IsNullOrEmpty(members.UnitPrice) && members.UnitPrice.Length > 0 &&
                    members.UnitPrice.Length < 12)
                    members.UnitPrice = members.UnitPrice.PadLeft(12, '0');
                if(!string.IsNullOrEmpty(members.TranId) && members.TranId.Length > 0 && members.TranId.Length < 2)
                    members.TranId = members.TranId.PadLeft(2, '0');
                if(!string.IsNullOrEmpty(members.Barcode) && members.Barcode.Length > 0 && members.Barcode.Length < 13)
                    members.Barcode = members.Barcode.PadLeft(13, '0');
                if(!string.IsNullOrEmpty(members.ExcRate) && members.ExcRate.Length > 0 && members.ExcRate.Length < 12)
                    members.ExcRate = members.ExcRate.PadLeft(12, '0');
                if(!string.IsNullOrEmpty(members.BatchNum) && members.BatchNum.Length > 0 &&
                    members.BatchNum.Length < 6)
                    members.BatchNum = members.BatchNum.PadLeft(6, '0');
                if(!string.IsNullOrEmpty(members.StanNum) && members.StanNum.Length > 0 && members.StanNum.Length < 6)
                    members.StanNum = members.StanNum.PadLeft(6, '0');
                if(!string.IsNullOrEmpty(members.AcquirerId) && members.AcquirerId.Length > 0 &&
                    members.AcquirerId.Length < 4)
                    members.AcquirerId = members.AcquirerId.PadLeft(4, '0');
                if(!string.IsNullOrEmpty(members.InstallmentCnt) && members.InstallmentCnt.Length > 0 &&
                    members.InstallmentCnt.Length < 2)
                    members.InstallmentCnt = members.InstallmentCnt.PadLeft(2, '0');
                if(!string.IsNullOrEmpty(members.ZNum) && members.ZNum.Length > 0 && members.ZNum.Length < 4)
                    members.ZNum = members.ZNum.PadLeft(4, '0');
                if(!string.IsNullOrEmpty(members.ReceiptNum) && members.ReceiptNum.Length > 0 &&
                    members.ReceiptNum.Length < 6)
                    members.ReceiptNum = members.ReceiptNum.PadLeft(6, '0');
                if(!string.IsNullOrEmpty(members.PLUNo) && members.PLUNo.Length > 0 && members.PLUNo.Length < 6)
                    members.PLUNo = members.PLUNo.PadLeft(6, '0');
                if(!string.IsNullOrEmpty(members.GroupNo) && members.GroupNo.Length > 0 && members.GroupNo.Length < 6)
                    members.GroupNo = members.GroupNo.PadLeft(6, '0');
                if(!string.IsNullOrEmpty(members.StockPiece) && members.StockPiece.Length > 0 &&
                    members.StockPiece.Length < 12)
                    members.StockPiece = members.StockPiece.PadLeft(12, '0');
                if(!string.IsNullOrEmpty(members.StartPLUNo) && members.StartPLUNo.Length > 0 &&
                    members.StartPLUNo.Length < 6)
                    members.StartPLUNo = members.StartPLUNo.PadLeft(6, '0');
                if(!string.IsNullOrEmpty(members.EndPLUNo) && members.EndPLUNo.Length > 0 &&
                    members.EndPLUNo.Length < 6)
                    members.EndPLUNo = members.EndPLUNo.PadLeft(6, '0');
                if(!string.IsNullOrEmpty(members.DepLimitAmount) && members.DepLimitAmount.Length > 0 &&
                    members.DepLimitAmount.Length < 12)
                    members.DepLimitAmount = members.DepLimitAmount.PadLeft(12, '0');
                if(!string.IsNullOrEmpty(members.StartZNo) && members.StartZNo.Length > 0 &&
                    members.StartZNo.Length < 4)
                    members.StartZNo = members.StartZNo.PadLeft(4, '0');
                if(!string.IsNullOrEmpty(members.EndZNo) && members.EndZNo.Length > 0 && members.EndZNo.Length < 4)
                    members.EndZNo = members.EndZNo.PadLeft(4, '0');
                if(!string.IsNullOrEmpty(members.StartReceiptNo) && members.StartReceiptNo.Length > 0 &&
                    members.StartReceiptNo.Length < 6)
                    members.StartReceiptNo = members.StartReceiptNo.PadLeft(6, '0');
                if(!string.IsNullOrEmpty(members.EndReceiptNo) && members.EndReceiptNo.Length > 0 &&
                    members.EndReceiptNo.Length < 6)
                    members.EndReceiptNo = members.EndReceiptNo.PadLeft(6, '0');
                if(string.IsNullOrEmpty(members.GroupNo) || members.GroupNo.Length <= 0 || members.GroupNo.Length >= 6)
                    return;
                members.GroupNo = members.GroupNo.PadLeft(6, '0');
            }
        }
    }
}