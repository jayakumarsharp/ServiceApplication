using System.Configuration;

namespace Servion.RISL.Services.DataDownload
{
    /// <summary>
    /// DownloadServiceSettings - Custom Configuration Settings
    /// </summary>
    public sealed class DownloadServiceSettings : ConfigurationSection
    {
        [ConfigurationProperty("FtpSettings")]
        public FtpSettingCollection FtpSettings
        {
            get { return ((FtpSettingCollection)(base["FtpSettings"])); }
        }
    }

    [ConfigurationCollection(typeof(FtpSetting))]
    public sealed class FtpSettingCollection : ConfigurationElementCollection
    {
        #region Override Methods
        protected override ConfigurationElement CreateNewElement()
        {
            return new FtpSetting();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FtpSetting)(element)).Name;
        }
        #endregion Override Methods

        #region Indexers
        public FtpSetting this[int idx]
        {
            get
            {
                return (FtpSetting)BaseGet(idx);
            }
        }

        public new FtpSetting this[string name]
        {
            get
            {
                for (int count = 0; count < base.Count; count++)
                {
                    if ((BaseGet(count) as FtpSetting).Name == name)
                    {
                        return (FtpSetting)BaseGet(count);
                    }
                }
                return null;
            }
        }
        #endregion Indexers
    }

    public sealed class FtpSetting : ConfigurationElement
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

        [ConfigurationProperty("VxmlServer", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string VxmlServer
        {
            get
            {
                return ((string)(base["VxmlServer"]));
            }
            set
            {
                base["VxmlServer"] = value;
            }
        }

        [ConfigurationProperty("FtpDirectorySetting", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string FtpDirectorySetting
        {
            get
            {
                return ((string)(base["FtpDirectorySetting"]));
            }
            set
            {
                base["FtpDirectorySetting"] = value;
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

    public sealed class Folder : ConfigurationElement
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

        [ConfigurationProperty("ServerIP", DefaultValue = "", IsRequired = false)]
        public string ServerIP
        {
            get
            {
                return ((string)(base["ServerIP"]));
            }
            set
            {
                base["ServerIP"] = value;
            }
        }

        [ConfigurationProperty("ServerPort", DefaultValue = "", IsRequired = false)]
        public string ServerPort
        {
            get
            {
                return ((string)(base["ServerPort"]));
            }
            set
            {
                base["ServerPort"] = value;
            }
        }

        [ConfigurationProperty("SourcePath", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string SourcePath
        {
            get
            {
                return ((string)(base["SourcePath"]));
            }
            set
            {
                base["SourcePath"] = value;
            }
        }

        [ConfigurationProperty("SourceAccessMode", DefaultValue = "FtpAccess", IsKey = true, IsRequired = true)]
        public FolderAccessMode SourceAccessMode
        {
            get
            {
                return ((FolderAccessMode)(base["SourceAccessMode"]));
            }
            set
            {
                base["SourceAccessMode"] = value;
            }
        }

        [ConfigurationProperty("FtpUserId", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string FtpUserId
        {
            get
            {
                return ((string)(base["FtpUserId"]));
            }
            set
            {
                base["FtpUserId"] = value;
            }
        }

        [ConfigurationProperty("FtpPassword", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string FtpPassword
        {
            get
            {
                return ((string)(base["FtpPassword"]));
            }
            set
            {
                base["FtpPassword"] = value;
            }
        }

        [ConfigurationProperty("TargetPath", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string TargetPath
        {
            get
            {
                return ((string)(base["TargetPath"]));
            }
            set
            {
                base["TargetPath"] = value;
            }
        }
    }

    [ConfigurationCollection(typeof(Folder))]
    public sealed class FolderCollection : ConfigurationElementCollection
    {
        #region Override Methods

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Folder)element).Name;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Folder();
        }

        protected override string ElementName
        {
            get { return "Folder"; }
        }
        #endregion Override Methods

        #region Indexers
        public Folder this[int idx]
        {
            get
            {
                return (Folder)BaseGet(idx);
            }
        }

        public int IndexOf(string name)
        {
            name = name.ToLower();

            for (int idx = 0; idx < base.Count; idx++)
            {
                if (this[idx].Name.ToLower() == name)
                    return idx;
            }
            return -1;
        }

        public new Folder this[string name]
        {
            get
            {
                if (IndexOf(name) < 0) return null;

                return (Folder)BaseGet(name);
            }
        }
        #endregion Indexers
    }

    public sealed class FtpDirectorySettings : ConfigurationSection
    {
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public DirectorySettingCollection DirectorySettings
        {
            get { return ((DirectorySettingCollection)(base[""])); }
        }
    }

    public sealed class DirectorySetting : ConfigurationElement
    {
        [ConfigurationProperty("SettingName", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string SettingName
        {
            get
            {
                return ((string)(base["SettingName"]));
            }
            set
            {
                base["SettingName"] = value;
            }
        }

        [ConfigurationProperty("Folders", IsDefaultCollection = false)]
        public FolderCollection Folders
        {
            get { return ((FolderCollection)(base["Folders"])); }
        }
    }

    [ConfigurationCollection(typeof(DirectorySetting))]
    public sealed class DirectorySettingCollection : ConfigurationElementCollection
    {
        #region Override Methods
        protected override ConfigurationElement CreateNewElement()
        {
            return new DirectorySetting();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DirectorySetting)(element)).SettingName;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return "DirectorySetting"; }
        }

        #endregion Override Methods

        #region Indexers
        public DirectorySetting this[int idx]
        {
            get
            {
                return (DirectorySetting)BaseGet(idx);
            }
        }

        public new DirectorySetting this[string name]
        {
            get
            {
                for (int i = 0; i < base.Count; i++)
                {
                    if ((BaseGet(i) as DirectorySetting).SettingName.ToUpper() == name.ToUpper())
                    {
                        return (DirectorySetting)BaseGet(i);
                    }
                }
                return null;
            }
        }
        #endregion Indexers
    }

    public enum FolderAccessMode
    {
        FtpAccess,
        SftpAccess,
        SharedFolderAccess
    }
}