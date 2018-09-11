using System;
using System.Runtime.Serialization;

namespace Panaroma.OKC.Integration.Library
{
    public class EthernetConnectionException : Exception
    {
        public EthernetConnectionException(string message)
          : base(message)
        {
        }

        public EthernetConnectionException(string message, Exception innerException)
          : base(message, innerException)
        {
        }

        protected EthernetConnectionException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }
    }
}