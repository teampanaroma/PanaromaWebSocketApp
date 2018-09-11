using System;
using System.Runtime.Serialization;

namespace Panaroma.OKC.Integration.Library
{
    public class PCPOSOKCException : Exception
    {
        public PCPOSOKCException(string message)
            : base(message)
        {
        }

        public PCPOSOKCException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public PCPOSOKCException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}