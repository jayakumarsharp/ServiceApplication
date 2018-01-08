
namespace Servion.RISL.Services.DataUpload
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using Servion.RISL.Utilities.DataImport;

    class ConfigurationHelper
    {
        /// <summary>
        /// To validate the configuration settings (i.e appSettings, DataReaderSetting, etc...)
        /// </summary>
        /// <returns>returns true if it is valid configuration settings; otherwise false</returns>
        public static bool ValidateConfiguration()
        {
            try
            {
                Logger.Log.Info("Inside Method");

                if (!HasValidAppSettings())
                {
                    Logger.Log.Error("Invalid appSettings in congfiguration file");
                    return false;
                }

                if (!HasValidDataReaderSettings())
                {
                    Logger.Log.Error("Invalid DataReaderSettings in congfiguration file");
                    return false;
                }

                if (!HasValidAppImportSettings())
                {
                    Logger.Log.Error("Invalid AppDataImportSettings in congfiguration file");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error occured while valiation congfiguration settings", ex);
                return false;
            }
        }

        /// <summary>
        /// To validate the appSettings section in the configuration
        /// </summary>
        /// <returns>returns true it has valid appSettings</returns>
        private static bool HasValidAppSettings()
        {
            Logger.Log.Info("Inside Method");

            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["ApplicationName"]))
            {
                Logger.Log.Error("Application name setting is missing");
                return false;
            }

            int commandTimeout=0;
            bool checkCommandTimeout = int.TryParse(ConfigurationManager.AppSettings["CommandTimeout"], out commandTimeout);

            if (!checkCommandTimeout || commandTimeout <= 0 || commandTimeout > 100000)
            {
                Logger.Log.Error("CommandTimeout (in seconds) is wrongly configured. It should be a number between 1 to 100000");
                return false;
            }

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["TempXmlPacketFolder"]))
            {
                if (!TryCreateFolderPath(ConfigurationManager.AppSettings["TempXmlPacketFolder"]))
                {
                    Logger.Log.Error("Temporary xml packet folder cannot be created / wrongly configured");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// To validate the DataReaderSettings
        /// </summary>
        /// <returns>returns true if it has valid information in DataReaderSettings</returns>
        private static bool HasValidDataReaderSettings()
        {
            Logger.Log.Info("Inside Method");

            DataUploadServiceSettings serviceSettings = ConfigurationManager.GetSection("DataUploadServiceSettings") as DataUploadServiceSettings;

            if (serviceSettings == null)
            {
                Logger.Log.Error("DataUploadServiceSettings missing in congfiguration file");
                return false;
            }

            if (serviceSettings.DataReaderSettings.Count == 0)
            {
                Logger.Log.Error("There is no DataReaderSetting configured in congfiguration file");
                return false;
            }

            if (serviceSettings.DataReaderSettings.OfType<DataReaderSetting>().Where(R => R.IsActive).Count() == 0)
            {
                Logger.Log.Error("There is no active DataReaderSetting configured in congfiguration file");
                return false;
            }

            foreach (DataReaderSetting setting in serviceSettings.DataReaderSettings)
            {
                if (string.IsNullOrEmpty(setting.RecoveryFolder)) { Logger.Log.Error("Recovery folder missing in congfiguration"); return false; }
                if (!TryCreateFolderPath(setting.RecoveryFolder)) { Logger.Log.Error("Recovery folder cannot be created / wrongly configured"); return false; }

                if (string.IsNullOrEmpty(setting.InvalidDBRequestFolder)) { Logger.Log.Error("Invalid DB Request folder missing in congfiguration"); return false; }
                if (!TryCreateFolderPath(setting.InvalidDBRequestFolder)) { Logger.Log.Error("Invalid DB Request folder cannot be created / wrongly configured"); return false; }

                if (string.IsNullOrEmpty(setting.InvalidXmlFolder)) { Logger.Log.Error("Invalid Xml folder missing in congfiguration"); return false; }
                if (!TryCreateFolderPath(setting.InvalidXmlFolder)) { Logger.Log.Error("Invalid Xml folder cannot be created / wrongly configured"); return false; }

                if (string.IsNullOrEmpty(setting.ConnectionStringName)) { Logger.Log.Error("Connectin string name missing in congfiguration"); return false; }
                if (!TryGettingConnectionStringSetting(setting.ConnectionStringName)) { Logger.Log.Error("Connectin string name is not available in connectionStrings section"); return false; }

                if (string.IsNullOrEmpty(setting.LoggerName)) { Logger.Log.Error("Logger name missing in congfiguration"); return false; }

                if (string.IsNullOrEmpty(setting.QueueName)) { Logger.Log.Error("Queue name missing in congfiguration"); return false; }
            }

            return true;
        }

        /// <summary>
        /// To validate the AppImportSettings
        /// </summary>
        /// <returns>returns true if it has valid AppImportSettings</returns>
        private static bool HasValidAppImportSettings()
        {
            Logger.Log.Info("Inside Method");

            DataImportSettings importSettings = ConfigurationManager.GetSection("DataImportSettings") as DataImportSettings;

            if (importSettings == null)
            {
                Logger.Log.Error("DataImportSettings missing in congfiguration file");
                return false;
            }

            if (importSettings.AppImportSettings.Count == 0)
            {
                Logger.Log.Error("There is no AppImportSetting configured in congfiguration file");
                return false;
            }

            foreach (AppImportSetting setting in importSettings.AppImportSettings)
            {
                if (string.IsNullOrEmpty(setting.ID)) { Logger.Log.Error("App ID missing in congfiguration"); return false; }
                if (string.IsNullOrEmpty(setting.Name)) { Logger.Log.Error("App Name missing in congfiguration"); return false; }

                if (setting.XsdValidationRequired)
                {
                    if (string.IsNullOrEmpty(setting.XsdFile)) { Logger.Log.Error("Xsd File is missing in congfiguration"); return false; }
                    string xsdFile = string.Format("{0}\\XSD\\{1}", AppDomain.CurrentDomain.BaseDirectory, setting.XsdFile);
                    if (!File.Exists(xsdFile)) { Logger.Log.Error("Xsd file congfigured is not available"); return false; } 
                }

                if (setting.DataImportRequired)
                {
                    if (string.IsNullOrEmpty(setting.DataImportProcedureName)) { Logger.Log.Error("DataImportProcedureName is missing in congfiguration"); return false; }
                }
            }

            return true;
        }

        /// <summary>
        /// To create the folders specified for recovery purpose
        /// </summary>
        /// <param name="path">recovery folder path</param>
        /// <returns>returns true if the folder has created</returns>
        private static bool TryCreateFolderPath(string path)
        {
            try
            {
                if (Directory.Exists(path)) return true;
                Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// To verify the connectionStrings section specified in the DataReaderSettings
        /// </summary>
        /// <param name="connectionString">connection string name</param>
        /// <returns>returns true if the connection string name is avaialble in connectionStrings section</returns>
        private static bool TryGettingConnectionStringSetting(string connectionString)
        {
            try
            {
                ConnectionStringSettings connSettings = ConfigurationManager.ConnectionStrings[connectionString];
                return connSettings != null;
            }
            catch (ConfigurationErrorsException ex)
            {
                Logger.Log.Error(ex);
                return false;
            }
        }
    }
}
