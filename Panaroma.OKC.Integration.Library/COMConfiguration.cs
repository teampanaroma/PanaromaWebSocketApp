using System;
using System.ComponentModel;

namespace Panaroma.OKC.Integration.Library
{
    public class COMConfiguration : IConfiguration
    {
        [Description("Zorunlu alan.")]
        public string PortName { get; set; }

        public COMConfiguration(string portName)
        {
            PortName = portName;
        }

        public T GetConfiguration<T>()
        {
            return (T)Convert.ChangeType(this, typeof(T));
        }
    }
}