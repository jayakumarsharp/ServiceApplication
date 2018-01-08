using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Servion.RISL.Services.DataDownload
{
    class ConfigurationHelper
    {
        /// <summary>
        /// To validate the configuration settings (i.e DownloadServiceSettings, FtpDirectorySettings, etc...)
        /// </summary>
        /// <returns>returns true if it is valid configuration settings; otherwise false</returns>
        public static bool ValidateConfiguration()
        {
            try
            {
                Logger.Log.Info("Inside Method");
                DownloadServiceSettings downloadSettings = ConfigurationManager.GetSection("DownloadServiceSettings") as DownloadServiceSettings;
                FtpDirectorySettings ftpDirectorySettings = ConfigurationManager.GetSection("FtpDirectorySettings") as FtpDirectorySettings;
                

                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                ConfigurationSection section = config.GetSection("FtpDirectorySettings");

                if (section != null && !section.SectionInformation.IsProtected)
                {
                    section.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                    config.Save();
                }

                if (downloadSettings == null)
                {
                    Logger.Log.Error("Download service setting congfiguration is missing");
                    return false;
                }

                if (ftpDirectorySettings == null)
                {
                    Logger.Log.Error("Ftp directory setting congfiguration is missing");
                    return false;
                }

                if (downloadSettings.FtpSettings.Count == 0)
                {
                    Logger.Log.Error("There is no ftp setting congfigured");
                    return false;
                }

                if (ftpDirectorySettings.DirectorySettings.Count == 0)
                {
                    Logger.Log.Error("There is no ftp directory setting congfigured");
                    return false;
                }

                if (!HasActiveFtpSetttings(downloadSettings))
                {
                    Logger.Log.Error("There is no active ftp setting configured");
                    return false;
                }

                if (HasInvalidFtpSettings(downloadSettings, ftpDirectorySettings))
                {
                    Logger.Log.Error("Some invalid ftp settings configured");
                    return false;
                }

                if (HasInvalidFtpDirectorySettings(ftpDirectorySettings))
                {
                    Logger.Log.Error("Some invalid ftp directory settings configured");
                    return false;
                }

                if (HasDublicateFtpPath(ftpDirectorySettings))
                {
                    Logger.Log.Error("Duplicate ftp path appears in the ftp directory settings");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error Occured While Validating Configuration Settings", ex);
                return false;
            }
        }

        /// <summary>
        /// To check the ftp path/shared folder duplication
        /// </summary>
        /// <param name="ftpDirSettings"></param>
        /// <returns></returns>
        private static bool HasDublicateFtpPath(FtpDirectorySettings ftpDirSettings)
        {
            Logger.Log.Info("Inside Method");
            List<string> folders = new List<string>();

            foreach (DirectorySetting dirSetting in ftpDirSettings.DirectorySettings)
            {
                foreach (Folder folderSetting in dirSetting.Folders)
                {
                    folders.Add(string.Format("{0} - {1}",folderSetting.ServerIP,folderSetting.SourcePath));
                }
            }

            int count = folders.Count;

            string ftpPath1, ftpPath2;
            
            for (int i = 0; i < count; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    ftpPath1 = folders[i];
                    ftpPath2 = folders[j];

                    if (ftpPath1.Equals(ftpPath2, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// To validate the DirectorySettings
        /// </summary>
        /// <param name="downloadSettings">Download settings</param>
        /// <param name="ftpDirSettings">Directory settings</param>
        /// <returns></returns>
        private static bool HasInvalidFtpSettings(DownloadServiceSettings downloadSettings, FtpDirectorySettings ftpDirSettings)
        {
            Logger.Log.Info("Inside Method");
            foreach (FtpSetting ftpSetting in downloadSettings.FtpSettings)
            {

                if (string.IsNullOrEmpty(ftpSetting.FtpDirectorySetting))
                {
                    Logger.Log.Error("Ftp directory setting name missing in some download settings");
                    return true;
                }

                if (ftpDirSettings.DirectorySettings[ftpSetting.FtpDirectorySetting] == null)
                {

                    Logger.Log.ErrorFormat("Ftp directory setting missing, the direcotry setting name is {0}", ftpSetting.FtpDirectorySetting);
                    return true;
                }

                if (string.IsNullOrEmpty(ftpSetting.LoggerName))
                {
                    Logger.Log.Error("Logger name missing in some ftp configuration settings");
                    return true;
                }
            }

            int countDirectoryMapping = downloadSettings.FtpSettings.OfType<FtpSetting>().Count();
            int distinctCount = downloadSettings.FtpSettings.OfType<FtpSetting>().Select(eachsetting => eachsetting.FtpDirectorySetting).Distinct().Count();

            if (countDirectoryMapping != distinctCount)
            {
                Logger.Log.Error("The VXML server mapping to the FTP servers is repeated... ");
                return true;
            }

            return false;
        }

        /// <summary>
        /// To check the invalid ftp directory / shared folder settings
        /// </summary>
        /// <param name="ftpDirSettings">Directory settings</param>
        /// <returns></returns>
        private static bool HasInvalidFtpDirectorySettings(FtpDirectorySettings ftpDirSettings)
        {
            Logger.Log.Info("Inside Method");
            foreach (DirectorySetting setting in ftpDirSettings.DirectorySettings)
            {
                if(setting.Folders.OfType<Folder>().Where(F => string.IsNullOrEmpty(F.SourcePath)).Count() > 0)
                {
                    Logger.Log.Error("Ftp path missing in some ftp directory settings");
                    return true;
                }

                if (setting.Folders.OfType<Folder>().Where(F => string.IsNullOrEmpty(F.TargetPath)).Count() > 0)
                {
                    Logger.Log.Error("Target folder missing in some ftp directory settings");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// To the check the active download settings
        /// </summary>
        /// <param name="downloadSettings"></param>
        /// <returns></returns>
        private static bool HasActiveFtpSetttings(DownloadServiceSettings downloadSettings)
        {
            Logger.Log.Info("Inside Method");
            return (downloadSettings.FtpSettings.OfType<FtpSetting>().Where(F => F.IsActive).Count() > 0);
        }

    }
}
