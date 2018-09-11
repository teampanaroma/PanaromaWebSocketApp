namespace Panaroma.OKC.Integration.Library
{
    public class FreeText
    {
        public string Format { get; set; }
        public string TextPosition { get; set; }
        public string ReceiptEndMessage { get; set; }

        public FreeText(string format, string textPosition, string receiptEndMessage)
        {
            Format = format;
            TextPosition = textPosition;
            ReceiptEndMessage = receiptEndMessage;
            Helpers.ReceiptHelper.CheckReceiptEndParameters(format, textPosition, receiptEndMessage);
        }

        public string GetFreeText()
        {
            return Format + TextPosition + string.Format("{0:x2}", ReceiptEndMessage.Length) +
                   ReceiptEndMessage.BuildReceiptEndMessage();
        }
    }
}