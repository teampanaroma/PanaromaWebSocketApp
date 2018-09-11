using System;

namespace Panaroma.OKC.Integration.Library
{
    public class InformationMessages
    {
        public int? Code { get; set; }

        public InformationMessageType InformationMessageType { get; set; }
        public object Message { get; set; }
        public Exception Exception { get; set; }
    }
}