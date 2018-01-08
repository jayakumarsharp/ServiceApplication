
namespace Servion.RISL.Services.DataRecovery
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using Servion.RISL.Utilities.DataImport;

    class ConfigurationHelper
    {
        /// <summary>
        /// To validate the application configuration
        /// </summary>
        /// <returns></returns>
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

                DataRecoveryServiceSettings serviceSettings = ConfigurationManager.GetSection("DataRecoveryServiceSettings") as DataRecoveryServiceSettings;

                if (serviceSettings == null)
                {
                    Logger.Log.Error("DataRecoveryServiceSettings missing in congfiguration file");
                    return false;
                }

                if (serviceSettings.FileReaderSettings.Count == 0)
                {
                    Logger.Log.Error("There is no FileReaderSettings configured in congfiguration file");
                    return false;
                }

                if (!HasActiveFileReaderSettings(serviceSettings))
                {
                    Logger.Log.Error("There is no active FileReaderSettings in congfiguration file");
                    return false;
                }

                if (!HasValidFileReaderSettings(serviceSettings))
                {
                    Logger.Log.Error("Invalid FileReaderSettings in congfiguration file");
                    return false;
                }

                if (!IsAllRecoveryFolderExist(serviceSettings))
                {
                    Logger.Log.Error("Recovery folder is not available");
                    return false;
                }

                if (IsRecoveryDirectoryOverlap(serviceSettings))
                {
                    Logger.Log.Error("Recovery folders are overlapped with one another");
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
        /// To validate the appSettings in the configuration file
        /// </summary>
        /// <returns></returns>
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

            int fileRecoveryMaxTime = 0;
            bool checkFileRecoveryMaxTime = int.TryParse(ConfigurationManager.AppSettings["FileRecoveryMaxTime"], out fileRecoveryMaxTime);

            if (!checkFileRecoveryMaxTime || fileRecoveryMaxTime <= 0)
            {
                Logger.Log.Error("FileRecoveryMaxTime (in hours) is wrongly configured. It should be a natural number");
                return false;
            }

            return true;
        }

        /// <summary>
        /// To validate the FileReaderSettings
        /// </summary>
        /// <param name="serviceSettings">DataRecoveryServiceSettings in the configuration file</param>
        /// <returns></returns>
        private static bool HasValidFileReaderSettings(DataRecoveryServiceSettings serviceSettings)
        {
            Logger.Log.Info("Inside Method");

            foreach (FileReaderSetting setting in serviceSettings.FileReaderSettings)
            {
                if (!setting.IsActive) continue;

                if (string.IsNullOrEmpty(setting.RecoveryFolder)) { Logger.Log.Error("Recovery folder missing in congfiguration"); return false; }
                
                if (string.IsNullOrEmpty(setting.InvalidDBRequestFolder)) { Logger.Log.Error("Invalid DB Request folder missing in congfiguration"); return false; }
                
                if (string.IsNullOrEmpty(setting.InvalidXmlFolder)) { Logger.Log.Error("Invalid Xml folder missing in congfiguration"); return false; }
                
                if (string.IsNullOrEmpty(setting.ConnectionStringName)) { Logger.Log.Error("Connectin string name missing in congfiguration"); return false; }
                if (!TryGettingConnectionStringSetting(setting.ConnectionStringName)) { Logger.Log.Error("Connectin string name is not available in connectionStrings section"); return false; }

                if (string.IsNullOrEmpty(setting.LoggerName)) { Logger.Log.Error("Logger name missing in congfiguration"); return false; }
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
        /// To check the active file reader settings
        /// </summary>
        /// <param name="serviceSettings">DataRecoveryServiceSettings in the configuration file</param>
        /// <returns></returns>
        private static bool HasActiveFileReaderSettings(DataRecoveryServiceSettings serviceSettings)
        {
            Logger.Log.Info("Inside Method");
            return serviceSettings.FileReaderSettings.OfType<FileReaderSetting>().Where(eachSetting => eachSetting.IsActive).Count() > 0;
        }

        /// <summary>
        /// To check the existance of recovery folder, invalid xml folder, etc... 
        /// </summary>
        /// <param name="recoverySettings">DataRecoveryServiceSettings in the configuration file</param>
        /// <returns></returns>
        private static bool IsAllRecoveryFolderExist(DataRecoveryServiceSettings recoverySettings)
        {
            Logger.Log.Info("Inside Methosd");
            foreach (FileReaderSetting setting in recoverySettings.FileReaderSettings)
            {
                if (!setting.IsActive) continue;

                if (Directory.Exists(setting.RecoveryFolder))
                {
                    string subDir = string.Format("{0}\\{1}", setting.RecoveryFolder, StaticInfo.SecondCycleFolder);
                    if (!Directory.Exists(subDir)) Directory.CreateDirectory(subDir);
                    subDir = string.Format("{0}\\{1}", setting.RecoveryFolder, StaticInfo.UnRecoverableFolder);
                    if (!Directory.Exists(subDir)) Directory.CreateDirectory(subDir);
                }
                else
                {
                    Logger.Log.ErrorFormat("Recovery folder does not exist. Folder Path is {0}", setting.RecoveryFolder);
                    return false;
                }

                if (!Directory.Exists(setting.InvalidXmlFolder))
                {
                    Logger.Log.ErrorFormat("Invalid Xml folder does not exist. Folder Path is {0}", setting.InvalidXmlFolder);
                    return false;
                }

                if (!Directory.Exists(setting.InvalidDBRequestFolder))
                {
                    Logger.Log.ErrorFormat("Invalid DB Request folder does not exist. Folder Path is {0}", setting.InvalidDBRequestFolder);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// To check whether the recovery directories reside within one another
        /// </summary>
        /// <param name="recoverySettings">RecoverySettings in the configuration</param>
        /// <returns></returns>
        private static bool IsRecoveryDirectoryOverlap(DataRecoveryServiceSettings recoverySettings)
        {
            Logger.Log.Info("Inside Methosd");
            List<string> folders = null;
            try
            {
                folders = new List<string>();

                foreach (FileReaderSetting setting in recoverySettings.FileReaderSettings)
                {
                    folders.Add(setting.RecoveryFolder.ToLower());
                }

                int count = folders.Count;

                for (int i = 0; i < count; i++)
                {
                    for (int j = i + 1; j < count; j++)
                    {
                        if (folders[i].StartsWith(folders[j]))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            finally
            {
                if (folders != null) folders.Clear(); folders = null;
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
