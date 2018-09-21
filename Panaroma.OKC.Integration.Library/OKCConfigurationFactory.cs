using System;

namespace Panaroma.OKC.Integration.Library
{
    public class OKCConfigurationFactory
    {
        public static OKCConfiguration Build(IConfiguration configuration)
        {
            string name = configuration.GetType().Name;
            if(name == "COMConfiguration")
                return new OKCConfiguration(ConnectionType.COM, configuration.GetConfiguration<COMConfiguration>(),
                    null);
            if(name == "EthernetConfiguration")
                return new OKCConfiguration(ConnectionType.TCP_IP, null,
                    configuration.GetConfiguration<EthernetConfiguration>());
            throw new NotSupportedException();
        }
    }
}