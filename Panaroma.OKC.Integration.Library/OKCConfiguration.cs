namespace Panaroma.OKC.Integration.Library
{
    public sealed class OKCConfiguration
    {
        public ConnectionType ConnectionType { get; set; }
        public COMConfiguration ComConfiguration { get; set; }
        public EthernetConfiguration EthernetConfiguration { get; set; }

        public OKCConfiguration(ConnectionType connectionType, COMConfiguration comConfiguration = null,
            EthernetConfiguration ethernetConfiguration = null)
        {
            ConnectionType = connectionType;
            ComConfiguration = comConfiguration;
            EthernetConfiguration = ethernetConfiguration;
        }
    }
}