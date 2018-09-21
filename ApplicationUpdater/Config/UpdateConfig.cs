namespace Alfa.Windows.ApplicationUpdater
{
    /// <remarks/>
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRoot(Namespace = "", IsNullable = false)]
    public partial class UpdateConfig
    {
        private string typeField;

        private string nameField;

        private bool enabledField;

        private string descriptionField;

        private decimal versionField;

        /// <remarks/>
        public string Type
        {
            get { return typeField; }
            set { typeField = value; }
        }

        /// <remarks/>
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
    }
}