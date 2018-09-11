namespace Alfa.Windows.ApplicationUpdater
{
    /// <remarks/>
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRoot(Namespace = "", IsNullable = false)]
    public partial class AppConfig
    {
        private string nameField;

        private bool enabledField;

        private string descriptionField;

        private decimal versionField;
        private string mainAppNameField;
        private AppConfigFwRule[] fwRulesField;

        private AppConfigTrustedApp[] trustedAppsField;

        public string Name
        {
            get { return nameField; }
            set { nameField = value; }
        }

        /// <remarks/>
        public bool Enabled
        {
            get { return enabledField; }
            set { enabledField = value; }
        }

        /// <remarks/>
        public string Description
        {
            get { return descriptionField; }
            set { descriptionField = value; }
        }

        /// <remarks/>
        public decimal Version
        {
            get { return versionField; }
            set { versionField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("FwRule", IsNullable = false)]
        public AppConfigFwRule[] FwRules
        {
            get { return fwRulesField; }
            set { fwRulesField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("TrustedApp", IsNullable = false)]
        public AppConfigTrustedApp[] TrustedApps
        {
            get { return trustedAppsField; }
            set { trustedAppsField = value; }
        }

        public string MainAppName
        {
            get { return mainAppNameField; }

            set { mainAppNameField = value; }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    public partial class AppConfigFwRule
    {

        private string ipField;

        private string portField;

        private string typeField;

        private string directionField;

        /// <remarks/>
        public string Ip
        {
            get { return ipField; }
            set { ipField = value; }
        }

        /// <remarks/>
        public string Port
        {
            get { return portField; }
            set { portField = value; }
        }

        /// <remarks/>
        public string Type
        {
            get { return typeField; }
            set { typeField = value; }
        }

        /// <remarks/>
        public string Direction
        {
            get { return directionField; }
            set { directionField = value; }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    public partial class AppConfigTrustedApp
    {

        private string nameField;

        private string fileNameField;

        private decimal versionField;

        private string hashField;

        /// <remarks/>
        public string Name
        {
            get { return nameField; }
            set { nameField = value; }
        }

        /// <remarks/>
        public string FileName
        {
            get { return fileNameField; }
            set { fileNameField = value; }
        }

        /// <remarks/>
        public decimal Version
        {
            get { return versionField; }
            set { versionField = value; }
        }

        /// <remarks/>
        public string Hash
        {
            get { return hashField; }
            set { hashField = value; }
        }
    }
}