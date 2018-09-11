using Panaroma.OKC.Integration.Library;
using System;
using System.Configuration;

namespace Panaroma.Communication.Application
{
    public class OKCConfiguration
    {
        public short OKCConnectionType { get; set; }

        public EthernetConfiguration EthernetConfiguration { get; set; }

        public bool OKCLog { get; set; }

        public COMConfiguration ComConfiguration { get; set; }

        public OKCConfiguration()
        {
            if (ConfigurationManager.AppSettings["OKCConnectionType"] == null)
                throw new ArgumentNullException("Hatalı configuration dosyası. OKCConnectionType bulunamadı.");
            OKCConnectionType = OKCConnectionType = short.Parse(ConfigurationManager.AppSettings["OKCConnectionType"]);
            switch (OKCConnectionType)
            {
                case 1:
                    if (ConfigurationManager.AppSettings["OKCCOMPort"] == null)
                        throw new ArgumentNullException("Hatalı configuration dosyası. OKCCOMPort bulunamadı.");
                    ComConfiguration = new COMConfiguration(ConfigurationManager.AppSettings["OKCCOMPort"]);
                    if (ConfigurationManager.AppSettings["OKCLog"] == null)
                        break;
                    OKCLog = bool.Parse(ConfigurationManager.AppSettings["OKCLog"]);
                    break;

                case 2:
                    if (ConfigurationManager.AppSettings["OKCIpAddress"] == null)
                        throw new ArgumentNullException("Hatalı configuration dosyası. OKCIpAddress bulunamadı.");
                    EthernetConfiguration =
                        new EthernetConfiguration(ConfigurationManager.AppSettings["OKCIpAddress"], 41200);
                    if (ConfigurationManager.AppSettings["OKCLog"] == null)
                        break;
                    OKCLog = bool.Parse(ConfigurationManager.AppSettings["OKCLog"]);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        "Hatalı configuration dosyası. OKCConnectionType değeri 1 veya 2 olmalıdır.");
            }
        }

        public OKCConfiguration(EthernetConfiguration ethernetConfiguration, bool okcLog = false)
        {
            OKCConnectionType = 2;
            EthernetConfiguration = ethernetConfiguration;
            OKCLog = okcLog;
        }

        public OKCConfiguration(COMConfiguration comConfiguration, bool okcLog = false)
        {
            OKCConnectionType = 1;
            ComConfiguration = comConfiguration;
            OKCLog = okcLog;
        }
    }
}