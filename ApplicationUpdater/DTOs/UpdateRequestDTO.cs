namespace Alfa.Windows.ApplicationUpdater
{
    public class UpdateRequestDTO
    {
        public string TerminalSerialNum { get; set; }
        public Installedsoftware[] InstalledSoftwares { get; set; }

    }

    public class Installedsoftware
    {
        public string AppType { get; set; }
        public string AppHash { get; set; }
        public string AppSignature { get; set; }
        public string AppName { get; set; }
        public decimal AppVersion { get; set; }
        public bool Enabled { get; set; }
        public string Description { get; set; }
        public string AllowedIpAddresses { get; set; }
    }
}