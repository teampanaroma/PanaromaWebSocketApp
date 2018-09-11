using System;
using System.Runtime.Serialization;

namespace Panaroma.OKC.Integration.Library
{
    internal class COMPortConnectionException : Exception
    {
        public COMPortConnectionException(string message)
            : base(message)
        {
        }

        public COMPortConnectionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected COMPortConnectionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}