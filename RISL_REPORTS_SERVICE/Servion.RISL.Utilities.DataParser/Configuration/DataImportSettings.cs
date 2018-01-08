using System.Configuration;

namespace Servion.RISL.Utilities.DataImport
{
    /// <summary>
    /// DataImportSettings - Custom Configuration Settings
    /// </summary>
    public sealed class DataImportSettings : ConfigurationSection
    {
        [ConfigurationProperty("AppImportSettings")]
        public AppImportSettingCollection AppImportSettings
        {
            get { return ((AppImportSettingCollection)(base["AppImportSettings"])); }
        }
    }

    [ConfigurationCollection(typeof(AppImportSetting))]
    public sealed class AppImportSettingCollection : ConfigurationElementCollection
    {
        #region Override Methods
        protected override ConfigurationElement CreateNewElement()
        {
            return new AppImportSetting();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AppImportSetting)(element)).ID;
        }
        #endregion Override Methods

        #region Indexers
        public AppImportSetting this[int idx]
        {
            get
            {
                return (AppImportSetting)BaseGet(idx);
            }
        }

        public new AppImportSetting this[string id]
        {
            get
            {
                for (int count = 0; count < base.Count; count++)
                {
                    if ((BaseGet(count) as AppImportSetting).ID.ToUpper() == id.ToUpper())
                    {
                        return (AppImportSetting)BaseGet(count);
                    }
                }
                return null;
            }
        }
        #endregion Indexers
    }

    public sealed class AppImportSetting : ConfigurationElement
    {
        [ConfigurationProperty("ID", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string ID
        {
            get
            {
                return ((string)(base["ID"]));
            }
            set
            {
                base["ID"] = value;
            }
        }

        [ConfigurationProperty("Name", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Name
        {
            get
            {
                return ((string)(base["Name"]));
            }
            set
            {
                base["Name"] = value;
            }
        }

        [ConfigurationProperty("XsdFile", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string XsdFile
        {
            get
            {
                return ((string)(base["XsdFile"]));
            }
            set
            {
                base["XsdFile"] = value;
            }
        }

        [ConfigurationProperty("XsdValidationRequired", DefaultValue = false, IsKey = true, IsRequired = true)]
        public bool XsdValidationRequired
        {
            get
            {
                return ((bool)(base["XsdValidationRequired"]));
            }
            set
            {
                base["XsdValidationRequired"] = value;
            }
        }

        [ConfigurationProperty("DataImportProcedureName", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string DataImportProcedureName
        {
            get
            {
                return ((string)(base["DataImportProcedureName"]));
            }
            set
            {
                base["DataImportProcedureName"] = value;
            }
        }

      

        [ConfigurationProperty("DataImportStatusProcedureName", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string DataImportStatusProcedureName
        {
            get
            {
                return ((string)(base["DataImportStatusProcedureName"]));
            }
            set
            {
                base["DataImportStatusProcedureName"] = value;
            }
        }

        [ConfigurationProperty("DataImportRequired", DefaultValue = false, IsKey = true, IsRequired = true)]
        public bool DataImportRequired
        {
            get
            {
                return ((bool)(base["DataImportRequired"]));
            }
            set
            {
                base["DataImportRequired"] = value;
            }
        }
    }
}
