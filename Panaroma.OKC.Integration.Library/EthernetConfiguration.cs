using System;
using System.ComponentModel;

namespace Panaroma.OKC.Integration.Library
{
    public class EthernetConfiguration : IConfiguration
    {
        [Description("Zorunlu alan.")]
        public string IpAddress { get; set; }

        [Description("Zorunlu alan.")]
        public int Port { get; set; }

        public EthernetConfiguration(string ipAddress, int port)
        {
            IpAddress = ipAddress;
            Port = port.Equals(0) ? 41200 : port;
        }

        public T GetConfiguration<T>()
        {
            return (T)Convert.ChangeType(this, typeof(EthernetConfiguration));
        }
    }
}