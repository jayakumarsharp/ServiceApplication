using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace MSMQ_RFService
{
    /// <summary>
    /// DataImportSettings - Custom Configuration Settings
    /// </summary>
    public sealed class DataConfigSettings : ConfigurationSection
    {
        [ConfigurationProperty("AppConfigSettings")]
        public AppConfigSettingCollection AppConfigSettings
        {
            get { return ((AppConfigSettingCollection)(base["AppConfigSettings"])); }
        }
    }

    [ConfigurationCollection(typeof(AppConfigSetting))]
    public sealed class AppConfigSettingCollection : ConfigurationElementCollection
    {
        #region Override Methods
        protected override ConfigurationElement CreateNewElement()
        {
            return new AppConfigSetting();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AppConfigSetting)(element)).ID;
        }
        #endregion Override Methods

        #region Indexers
        public AppConfigSetting this[int idx]
        {
            get
            {
                return (AppConfigSetting)BaseGet(idx);
            }
        }

        public new AppConfigSetting this[string id]
        {
            get
            {
                for (int count = 0; count < base.Count; count++)
                {
                    if ((BaseGet(count) as AppConfigSetting).ID.ToUpper() == id.ToUpper())
                    {
                        return (AppConfigSetting)BaseGet(count);
                    }
                }
                return null;
            }
        }
        #endregion Indexers
    }

    public sealed class AppConfigSetting : ConfigurationElement
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
        [ConfigurationProperty("ThreadSleepRequired", DefaultValue = false, IsKey = true, IsRequired = true)]
        public bool ThreadSleepRequired
        {
            get
            {
                return ((bool)(base["ThreadSleepRequired"]));
            }
            set
            {
                base["ThreadSleepRequired"] = value;
            }
        }
        [ConfigurationProperty("QueueName", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string QueueName
        {
            get
            {
                return ((string)(base["QueueName"]));
            }
            set
            {
                base["QueueName"] = value;
            }
        }

        [ConfigurationProperty("IsTransactionEnabled", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string IsTransactionEnabled
        {
            get
            {
                return ((string)(base["IsTransactionEnabled"]));
            }
            set
            {
                base["IsTransactionEnabled"] = value;
            }
        }
    }
}