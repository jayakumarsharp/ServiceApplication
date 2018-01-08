using System;
using System.Collections;
using System.IO;
using System.Threading;
using log4net;
using Servion.Cca.Utilities.CallDataImporter;
using Servion.Cca.Utilities.CallDataReader;
using Servion.Cca.Utilities.CommonProperties;
using Servion.Cca.Utilites.SftpCallDataReader;
using Servion.RISL.Services.DataDownload;

namespace Servion.RISL.Services.DataDownload
{
    class DataDownload
    {
        private object sftplock = new object();
        /// <summary>
        /// To process the call data download process from vxml servers
        /// </summary>
        /// <param name="downloadSettings">DownloadDirectorySettings in the configuration</param>
        internal void StartDownload(object downloadSettings)
        {
            ILog log = null;
            FtpSetting ftpSetting = null;
            DirectorySetting dirSetting = null;
            object[] settings = null;
            string message = string.Empty;
            try
            {
                Logger.Log.Info("Inside Method");
                settings = downloadSettings as object[];
                ftpSetting = settings[0] as FtpSetting;
                dirSetting = settings[1] as DirectorySetting;

                log = LogManager.GetLogger(ftpSetting.LoggerName);

                int maxWaitTime = ftpSetting.MaxThreadSleepTime;
                int threadIncrTime = ftpSetting.SleepTimeIncrement;
                int currentWaitTime = 1;
                int downloadedFileCount = 0;
                int totalDownloadedFileCount = 0;
                int defaultWaitTime = 5;

                DateTime downloadStartTime = DateTime.Now;
                TimeSpan totalDownloadTime;

                while (StaticInfo.CanContinue)
                {
                    try
                    {
                        totalDownloadedFileCount = 0;

                        foreach (Folder foldersetting in dirSetting.Folders)
                        {
                            downloadStartTime = DateTime.Now;
                            downloadedFileCount = 0;

                            int fileCount = 0;

                            if (foldersetting.SourceAccessMode == FolderAccessMode.FtpAccess)
                            {
                                fileCount = DownloadFilesFromFtp(foldersetting, log, out downloadedFileCount);
                            }
                            else if (foldersetting.SourceAccessMode == FolderAccessMode.SharedFolderAccess)
                            {
                                fileCount = DownloadFilesFromSharedFolder(foldersetting, log, out downloadedFileCount);
                            }
                            else if (foldersetting.SourceAccessMode == FolderAccessMode.SftpAccess)
                            {
                                fileCount = DownloadFilesFromSftp(foldersetting, log, out downloadedFileCount);
                            };

                            totalDownloadedFileCount = totalDownloadedFileCount + downloadedFileCount;

                            if (fileCount == 0)
                            {
                                log.DebugFormat("Putting the thread on sleep to [{0}] seconds", currentWaitTime);
                                Thread.Sleep(currentWaitTime * 1000);
                            }
                            else
                            {
                                currentWaitTime = 1;
                                totalDownloadTime = DateTime.Now.Subtract(downloadStartTime);
                                log.DebugFormat("Total Time Taken For Download {3} Files [{0} Mins]  [{1} Seconds ] [{2} Milliseconds] ", totalDownloadTime.Minutes, totalDownloadTime.Seconds, totalDownloadTime.Milliseconds, downloadedFileCount);
                                log.DebugFormat("Putting the thread to sleep for [{0}] seconds", defaultWaitTime);
                                Thread.Sleep(defaultWaitTime * 1000);
                            }

                            if (!StaticInfo.CanContinue)
                            {
                                log.Info("Download Service is requested for stopping the download process....");
                                return;
                            }
                        }

                        if (totalDownloadedFileCount == 0)
                        {
                            if (currentWaitTime < maxWaitTime) currentWaitTime = currentWaitTime + threadIncrTime;
                        }
                        else
                        {
                            log.DebugFormat("Putting the thread to sleep for [{0}] seconds after one completed iterative download", maxWaitTime);
                            Thread.Sleep(maxWaitTime * 1000);
                        }
                    }
                    catch (Exception innerWhileEx)
                    {
                        message = string.Format("Error Occured While Download Process : {0}", innerWhileEx.Message);
                        log.Error(message, innerWhileEx);
                        Thread.Sleep(maxWaitTime);
                    }
                }
            }
            catch (Exception ex)
            {
                message = string.Format("Error Occured While Starting Thread For Ftp Setting : {0}", ftpSetting.Name);
                Logger.Log.Error(message, ex);
            }
            finally
            {
                Interlocked.Decrement(ref StaticInfo.ThreadCount);
                Logger.Log.InfoFormat("Decrementing Thread Count as Method is exiting ... Thread Count {0} ", Interlocked.Read(ref StaticInfo.ThreadCount));
                ftpSetting = null;
                dirSetting = null;
                settings = null;
                log = null;
            }
        }

        /// <summary>
        /// Download files from Ftp location
        /// </summary>
        /// <param name="folderSetting">FolderSetting from configuration to get the source ftp path & target folder</param>
        /// <param name="log">Thread specific logger</param>
        /// <param name="downloadedFileCount">To get the count of downloaded files from ftp</param>
        /// <returns>returns - Total files available in the ftp folder</returns>
        private int DownloadFilesFromFtp(Folder folderSetting, ILog log, out int downloadedFileCount)
        {
            string localFilePath = string.Empty;
            string localFileFolder = string.Empty;
            string message = string.Empty;
            int totalFileFound = 0;
            downloadedFileCount = 0;
            try
            {
                log.Info("Inside Method");

                FtpCallDataReader ftpReader = new FtpCallDataReader(folderSetting.SourcePath, folderSetting.FtpUserId, folderSetting.FtpPassword);
                ftpReader.FolderHierarchy = DirectoryHierarchy.None;

                FileCallDataImporter fileImporter = new FileCallDataImporter(folderSetting.TargetPath);
                fileImporter.FolderHierarchy = DirectoryHierarchy.DateHour;

                ftpReader.ReadCallData();

                totalFileFound = ftpReader.CallDataList.Count;
                log.InfoFormat("{0} Files Found At {1}", totalFileFound, folderSetting.SourcePath);

                if (totalFileFound == 0) return 0;

                foreach (IvrCallData callData in ftpReader.CallDataList)
                {
                    FtpCallData ftpCallData = callData as FtpCallData;

                    Hashtable htParams = new Hashtable();
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(ftpCallData.FtpFilePath);
                    string fileExtn = System.IO.Path.GetExtension(ftpCallData.FtpFilePath).Replace(".", "");
                    htParams["FILE_NAME"] = fileName;
                    htParams["FILE_EXTENSION"] = fileExtn;
                    htParams["CALL_DATA"] = ftpCallData.CallData;

                    fileImporter.ImportCallData(htParams);
                }

                log.InfoFormat("{0} Files Downloaded From {1}", downloadedFileCount, folderSetting.SourcePath);

                if (downloadedFileCount < totalFileFound)
                {
                    message = string.Format("{0} Files Unable to Download From {1} ", totalFileFound - downloadedFileCount, folderSetting.SourcePath);
                    log.InfoFormat(message);
                }
            }
            catch (Exception ex)
            {
                message = string.Format("Error Occured while downloading the files from ftp : {0}", folderSetting.SourcePath);
                log.Error(message, ex);
            }
            return totalFileFound;
        }

        /// <summary>
        /// Download files from Ftp location
        /// </summary>
        /// <param name="folderSetting">FolderSetting from configuration to get the source ftp path & target folder</param>
        /// <param name="log">Thread specific logger</param>
        /// <param name="downloadedFileCount">To get the count of downloaded files from ftp</param>
        /// <returns>returns - Total files available in the ftp folder</returns>
        private int DownloadFilesFromSftp(Folder folderSetting, ILog log, out int downloadedFileCount)
        {
            string localFilePath = string.Empty;
            string localFileFolder = string.Empty;
            string message = string.Empty;
            int totalFileFound = 0;
            downloadedFileCount = 0;
            lock (sftplock)
            {
                try
                {
                    log.Info("Inside Method");

                    SftpCallDataReader sftpReader = new SftpCallDataReader(folderSetting.ServerIP, folderSetting.ServerPort,
                        folderSetting.SourcePath, folderSetting.FtpUserId, folderSetting.FtpPassword);
                    sftpReader.FolderHierarchy = DirectoryHierarchy.None;

                    FileCallDataImporter fileImporter = new FileCallDataImporter(folderSetting.TargetPath);
                    fileImporter.FolderHierarchy = DirectoryHierarchy.DateHour;

                    sftpReader.ReadCallData();

                    totalFileFound = sftpReader.CallDataList.Count;
                    log.InfoFormat("{0} Files Found At {1}", totalFileFound, folderSetting.SourcePath);

                    if (totalFileFound == 0) return 0;

                    foreach (IvrCallData callData in sftpReader.CallDataList)
                    {
                        FtpCallData ftpCallData = callData as FtpCallData;

                        Hashtable htParams = new Hashtable();
                        string fileName = System.IO.Path.GetFileNameWithoutExtension(ftpCallData.FtpFilePath);
                        string fileExtn = System.IO.Path.GetExtension(ftpCallData.FtpFilePath).Replace(".", "");
                        htParams["FILE_NAME"] = fileName;
                        htParams["FILE_EXTENSION"] = fileExtn;
                        htParams["CALL_DATA"] = ftpCallData.CallData;
                        fileImporter.ImportCallData(htParams);
                    }
                    log.InfoFormat("{0} Files Downloaded From {1}", downloadedFileCount, folderSetting.SourcePath);

                    if (downloadedFileCount < totalFileFound)
                    {
                        message = string.Format("{0} Files Unable to Download From {1} ", totalFileFound - downloadedFileCount, folderSetting.SourcePath);
                        log.InfoFormat(message);
                    }
                }
                catch (Exception ex)
                {
                    message = string.Format("Error Occured while downloading the files from ftp : {0}", folderSetting.SourcePath);
                    log.Error(message, ex);
                }
                return totalFileFound;
            }
        }

        /// <summary>
        /// Download files from Shared Folder
        /// </summary>
        /// <param name="folderSetting">FolderSetting from configuration to get the source folder & target folder</param>
        /// <param name="log">Thread specific logger</param>
        /// <param name="downloadedFileCount">To get the count of retrieved files from shared path</param>
        /// <returns>returns - Total files available in the shared folder</returns>
        private int DownloadFilesFromSharedFolder(Folder folderSetting, ILog log, out int downloadedFileCount)
        {
            string localFilePath = string.Empty;
            string localFileFolder = string.Empty;
            string message = string.Empty;
            int totalFileFound = 0;
            downloadedFileCount = 0;
            try
            {
                log.Info("Inside Method");
                FileCallDataReader fileCallDataReader = new FileCallDataReader(folderSetting.SourcePath);
                fileCallDataReader.FolderHierarchy = DirectoryHierarchy.None;

                FileCallDataImporter fileCallDataImporter = new FileCallDataImporter(folderSetting.TargetPath);
                fileCallDataImporter.FolderHierarchy = DirectoryHierarchy.DateHour;

                fileCallDataReader.ReadCallData();

                totalFileFound = fileCallDataReader.CallDataList.Count;
                log.InfoFormat("{0} Files Found At {1}", totalFileFound, folderSetting.SourcePath);

                if (totalFileFound == 0) return 0;

                foreach (IvrCallData ivrCallData in fileCallDataReader.CallDataList)
                {
                    FileCallData fileCallData = ivrCallData as FileCallData;

                    Hashtable htParams = new Hashtable();
                    htParams["FILE_NAME"] = System.IO.Path.GetFileNameWithoutExtension(fileCallData.FilePath);
                    htParams["FILE_EXTENSION"] = System.IO.Path.GetExtension(fileCallData.FilePath).Replace(".", "");
                    htParams["CALL_DATA"] = fileCallData.CallData;

                    fileCallDataImporter.ImportCallData(htParams);
                    File.Delete(fileCallData.FilePath);
                    downloadedFileCount++;
                }

                log.InfoFormat("{0} Files Downloaded From {1}", downloadedFileCount, folderSetting.SourcePath);

                if (downloadedFileCount < totalFileFound)
                {
                    message = string.Format("{0} Files Unable to Download From {1} ", totalFileFound - downloadedFileCount, folderSetting.SourcePath);
                    log.InfoFormat(message);
                }
            }
            catch (Exception ex)
            {
                message = string.Format("Error Occured while downloading the files from shared path : {0}", folderSetting.SourcePath);
                log.Error(message, ex);
            }
            return totalFileFound;
        }
    }
}
