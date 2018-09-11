using System;
using System.Runtime.CompilerServices;

namespace Panaroma.OKC.Integration.Library
{
    public class ProcessInformation
    {
        public string Method { get; set; }

        public DateTime DateTime { get; private set; } = DateTime.Now;

        public InformationMessages InformationMessages { get; set; }

        public bool HasError { get; set; }

        public ProcessInformation()
        {
            InformationMessages = new InformationMessages();
        }

        public ProcessInformation(string method = "")
        {
            Method = method;
            InformationMessages = new InformationMessages();
        }

        public ProcessInformation(bool hasError, InformationMessages informationMessages,
            [CallerMemberName] string method = "")
        {
            Method = method;
            HasError = hasError;
            InformationMessages = informationMessages;
        }
    }
}