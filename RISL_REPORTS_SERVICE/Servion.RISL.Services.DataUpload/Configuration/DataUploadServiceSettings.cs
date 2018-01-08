using System.Configuration;

namespace Servion.RISL.Services.DataUpload
{
    /// <summary>
    /// DataUploadServiceSettings - custom configuration settings
    /// </summary>
    public sealed class DataUploadServiceSettings : ConfigurationSection
    {
        [ConfigurationProperty("DataReaderSettings")]
        public DataReaderSettingCollection DataReaderSettings
        {
            get { return ((DataReaderSettingCollection)(base["DataReaderSettings"])); }
        }
    }

    [ConfigurationCollection(typeof(DataReaderSetting))]
    public sealed class DataReaderSettingCollection : ConfigurationElementCollection
    {
        #region Override Methods
        protected override ConfigurationElement CreateNewElement()
        {
            return new DataReaderSetting();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DataReaderSetting)(element)).Name;
        }
        #endregion Override Methods

        #region Indexers
        public DataReaderSetting this[int idx]
        {
            get
            {
                return (DataReaderSetting)BaseGet(idx);
            }
        }

        public new DataReaderSetting this[string name]
        {
            get
            {
                for (int count = 0; count < base.Count; count++)
                {
                    if ((BaseGet(count) as DataReaderSetting).Name.ToUpper() == name.ToUpper())
                    {
                        return (DataReaderSetting)BaseGet(count);
                    }
                }
                return null;
            }
        }
        #endregion Indexers
    }

    public sealed class DataReaderSetting : ConfigurationElement
    {
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

        [ConfigurationProperty("ConnectionStringName", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string ConnectionStringName
        {
            get
            {
                return ((string)(base["ConnectionStringName"]));
            }
            set
            {
                base["ConnectionStringName"] = value;
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

        [ConfigurationProperty("LoggerName", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string LoggerName
        {
            get
            {
                return ((string)(base["LoggerName"]));
            }
            set
            {
                base["LoggerName"] = value;
            }
        }

        [ConfigurationProperty("RecoveryFolder", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string RecoveryFolder
        {
            get
            {
                return ((string)(base["RecoveryFolder"]));
            }
            set
            {
                base["RecoveryFolder"] = value;
            }
        }

        [ConfigurationProperty("InvalidXmlFolder", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string InvalidXmlFolder
        {
            get
            {
                return ((string)(base["InvalidXmlFolder"]));
            }
            set
            {
                base["InvalidXmlFolder"] = value;
            }
        }

        [ConfigurationProperty("InvalidDBRequestFolder", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string InvalidDBRequestFolder
        {
            get
            {
                return ((string)(base["InvalidDBRequestFolder"]));
            }
            set
            {
                base["InvalidDBRequestFolder"] = value;
            }
        }

        [ConfigurationProperty("WriteXmlToLog", DefaultValue = false, IsKey = true, IsRequired = true)]
        public bool WriteXmlToLog
        {
            get
            {
                return ((bool)(base["WriteXmlToLog"]));
            }
            set
            {
                base["WriteXmlToLog"] = value;
            }
        }

        [ConfigurationProperty("MaxThreadSleepTime", DefaultValue = 60, IsKey = true, IsRequired = true)]
        public int MaxThreadSleepTime
        {
            get
            {
                return ((int)(base["MaxThreadSleepTime"]));
            }
            set
            {
                base["MaxThreadSleepTime"] = value;
            }
        }

        [ConfigurationProperty("SleepTimeIncrement", DefaultValue = 1, IsKey = true, IsRequired = true)]
        public int SleepTimeIncrement
        {
            get
            {
                return ((int)(base["SleepTimeIncrement"]));
            }
            set
            {
                base["SleepTimeIncrement"] = value;
            }
        }

        [ConfigurationProperty("IsActive", DefaultValue = true, IsKey = true, IsRequired = true)]
        public bool IsActive
        {
            get
            {
                return ((bool)(base["IsActive"]));
            }
            set
            {
                base["IsActive"] = value;
            }
        }

    }
}
