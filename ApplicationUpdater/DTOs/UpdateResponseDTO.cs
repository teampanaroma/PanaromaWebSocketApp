namespace Alfa.Windows.ApplicationUpdater
{
    public class UpdateResponseDTO
    {
        public string TerminalSerialNum { get; set; }
        public Updatepackage[] UpdatePackages { get; set; }
        public string ErrMessage { get; set; }
        public bool Result { get; set; }
    }

    public class Updatepackage
    {
        public string AppName { get; set; }
        public string AppType { get; set; }
        public string AppUrl { get; set; }
        public bool Enabled { get; set; }
        public decimal Version { get; set; }
        public string Description { get; set; }
    }
}