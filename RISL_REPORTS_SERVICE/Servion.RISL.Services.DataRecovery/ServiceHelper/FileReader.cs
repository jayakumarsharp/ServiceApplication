using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using log4net;
using Servion.RISL.Utilities.DataImport;

namespace Servion.RISL.Services.DataRecovery
{
    class FileReader
    {
        private IFormatProvider _fp = new System.Globalization.CultureInfo("en-US");
        private int _fileRecoveryMaxTime = 24;

        /// <summary>
        /// To start the recovery process
        /// </summary>
        /// <param name="recoverySettings">FileReaderSettting in the configuration</param>
        internal void StartRecovery(object recoverySettings)
        {
            ILog log = null;
            FileReaderSetting recoverySetting = null;
            Queue<String> fileQ = new Queue<string>();
            CallDataImporation dataImporation = null;
            Stopwatch sWatch = new Stopwatch();
            Stopwatch swProcess = new Stopwatch();
            try
            {
                Logger.Log.Info("Inside Methosd");

                recoverySetting = recoverySettings as FileReaderSetting;
                _fileRecoveryMaxTime = StaticInfo.FileRecoveryMaxTime;

                int maxWaitTime = recoverySetting.MaxThreadSleepTime;
                int threadIncrTime = recoverySetting.SleepTimeIncrement;
                int currentWaitTime = 1;
                
                string connStr = System.Configuration.ConfigurationManager.ConnectionStrings[recoverySetting.ConnectionStringName].ConnectionString;
                string currentFile = string.Empty;
                string callData = string.Empty;

                TimeSpan importationTime;
                TimeSpan totalProcessTime;

                log = LogManager.GetLogger(recoverySetting.LoggerName);

                Dictionary<string, string> dicImportSettings = new Dictionary<string, string>();
                dicImportSettings.Add("ApplicationName", StaticInfo.ApplicationName);
                dicImportSettings.Add("ApplicationServer", StaticInfo.ApplicationServer);
                dicImportSettings.Add("ApplicationThreadName", Thread.CurrentThread.Name);
                dicImportSettings.Add("ConnectionString", connStr);
                dicImportSettings.Add("CommandTimeout", StaticInfo.CommandTimeout.ToString());

                dataImporation = new CallDataImporation(dicImportSettings, log);

                while (StaticInfo.CanContinue)
                {
                    try
                    {
                        fileQ = GetFileQueue(log, recoverySetting);
                        log.InfoFormat("Total No of Files Availble - {0}", fileQ.Count);

                        #region Delaying Recovery Process If No File Exists
                        //Putting Thread Into Sleep If No File Exists
                        if (fileQ.Count == 0)
                        {
                            if (currentWaitTime < maxWaitTime) currentWaitTime += threadIncrTime;
                            log.DebugFormat("Putting the thread [{0}] on sleep to [{1}] seconds ", Thread.CurrentThread.Name, currentWaitTime);
                            Thread.Sleep(currentWaitTime * 1000);
                            continue;
                        }
                        #endregion

                        currentWaitTime = 1;

                        while (fileQ.Count > 0 && StaticInfo.CanContinue)
                        {
                            #region Reading Call Data From Recovery File
                            swProcess.Start();
                            currentFile = fileQ.Dequeue();

                            try
                            {
                                callData = File.ReadAllText(currentFile);
                            }
                            catch (Exception ex)
                            {
                                log.ErrorFormat("Failed to read the call data file : {0}", currentFile);
                                log.Error(ex);
                                continue;
                            }

                            //To write the call data into log file if it is enabled
                            if (recoverySetting.WriteXmlToLog)
                            {
                                log.InfoFormat("Call Data : {0}", callData);
                            }

                            //Writing Empty Call Data as Bad Xml Packet
                            if (string.IsNullOrEmpty(callData))
                            {
                                MoveRecoveryFiles(log, currentFile, ImportationFailureMode.InvalidXml, recoverySetting);
                                continue;
                            }
                            #endregion

                            #region Import Process
                            sWatch.Start();
                            ImportationResponse importResponse = dataImporation.Import(callData);
                            sWatch.Stop();
                            importationTime = sWatch.Elapsed;

                            swProcess.Stop();
                            totalProcessTime = swProcess.Elapsed;
                            #endregion

                            #region Handling Import Response
                            if (importResponse.HasImported)
                            {
                                log.Info("Call Data Imported Successfully");
                                log.InfoFormat("Total time taken to process at data importation : {0}, Total process time : {1} milli seconds", importationTime.TotalMilliseconds, totalProcessTime.TotalMilliseconds);
                                DeleteProcessedFile(currentFile, log);
                            }
                            else
                            {
                                log.ErrorFormat("Call Data Failed To Import. Failure Mode : {0}, Error Code : {1}, Error Message : {2}", importResponse.FailureMode, importResponse.ErrorCode, importResponse.ErrorMessage);
                                MoveRecoveryFiles(log, currentFile, importResponse.FailureMode, recoverySetting);
                            }
                            #endregion    
                        }

                        log.DebugFormat("Putting the thread [{0}] on sleep to [{1}] seconds ", Thread.CurrentThread.Name, maxWaitTime);
                        Thread.Sleep(maxWaitTime * 1000);
                    }
                    catch (Exception exAtInnerWhile)
                    {
                        log.Error("Error occured while processing the file data read activity", exAtInnerWhile);
                        log.ErrorFormat("The file being processed at that time : {0}", currentFile);
                        log.DebugFormat("Putting the thread [{0}] on sleep to [{1}] seconds ", Thread.CurrentThread.Name, maxWaitTime);
                        Thread.Sleep(maxWaitTime * 1000);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Data Recovery Process is failed for the folder : " + recoverySetting.RecoveryFolder, ex);
            }
            finally
            {
                Interlocked.Decrement(ref StaticInfo.ThreadCount);
                Logger.Log.InfoFormat("Decrementing Thread Count as Method is exiting ... Thread Count {0} ", Interlocked.Read(ref StaticInfo.ThreadCount));
                if (fileQ != null) fileQ.Clear(); fileQ = null;
                if (dataImporation != null) dataImporation.Dispose(); dataImporation = null;
                recoverySetting = null; log = null;
            }
        }

        /// <summary>
        /// To queue the recovery files from recovery folder
        /// </summary>
        /// <param name="log">Thread specific logger</param>
        /// <param name="recoverySetting">FileReaderSetting in the configuration</param>
        /// <returns></returns>
        private Queue<string> GetFileQueue(ILog log, FileReaderSetting recoverySetting)
        {
            Queue<string> fileQ = null;
            string recoveryFolder = recoverySetting.RecoveryFolder;
            int totalIteration = 0;
            string msg = string.Empty;
            string[] dateDirectories = null;
            string[] hourDirectories = null;
            string[] hourDirFiles = null;
            bool isDebugEnabled = false;
            try
            {
                log.Info("Inside Method");
                string currentDateDir = DateTime.Now.ToString("yyyyMMdd");
                string currentHourDir = string.Empty;
                isDebugEnabled = log.IsDebugEnabled;
                fileQ = new Queue<string>();

            GET_FILE_LIST:
                //Just for ensuring to avoid unwanted looping if it is happen by mistake
                totalIteration++;

                //Importing Xml Packet File Names Into Queue Object
                dateDirectories = Directory.GetDirectories(recoveryFolder);
                log.DebugFormat("Total folders available under recovery folder [{0}] is {1}", recoveryFolder, dateDirectories.Length);

                foreach (string dateDir in dateDirectories)
                {
                    if (Path.GetFileName(dateDir).Equals(StaticInfo.SecondCycleFolder)) continue;
                    if (Path.GetFileName(dateDir).Equals(StaticInfo.UnRecoverableFolder)) continue;

                    if (!IsValidDateDirectory(log, dateDir)) continue;

                    if (Path.GetFileName(dateDir).Equals(currentDateDir))
                    {
                        currentHourDir = DateTime.Now.ToString("HH");

                        hourDirectories = Directory.GetDirectories(dateDir);
                        if(isDebugEnabled) log.DebugFormat("Total folders available under date level recovery folder [{0}] is {1}", dateDir, hourDirectories.Length);

                        foreach (string hourDir in hourDirectories)
                        {
                            if (!IsValidHourDirectory(log, hourDir)) continue;

                            if (string.Compare(Path.GetFileName(hourDir), currentHourDir) < 0)
                            {
                                hourDirFiles = Directory.GetFiles(hourDir);
                                if (isDebugEnabled) log.DebugFormat("Total files available under directory directory {0} is {1}", hourDir, hourDirFiles.Length);

                                foreach (string file in hourDirFiles) fileQ.Enqueue(file);

                                if (hourDirFiles.Length <= 0) DeleteEmptyDirectory(hourDir, log);
                            }
                        }
                    }
                    else if (string.Compare(Path.GetFileName(dateDir), currentDateDir) < 0)
                    {
                        hourDirectories = Directory.GetDirectories(dateDir);
                        if (isDebugEnabled) log.DebugFormat("Total folders available under date level recovery folder [{0}] is {1}", dateDir, hourDirectories.Length);

                        foreach (string hourDir in hourDirectories)
                        {
                            if (!IsValidHourDirectory(log, hourDir)) continue;

                            hourDirFiles = Directory.GetFiles(hourDir);
                            if (isDebugEnabled) log.DebugFormat("Total files available under directory directory {0} is {1}", hourDir, hourDirFiles.Length);

                            foreach (string file in hourDirFiles) fileQ.Enqueue(file);

                            if (hourDirFiles.Length <= 0) DeleteEmptyDirectory(hourDir, log);
                        }

                        if (hourDirectories.Length <= 0) DeleteEmptyDirectory(dateDir, log);
                    }

                }

                if (!Path.GetFileName(recoveryFolder).Equals(StaticInfo.SecondCycleFolder) && totalIteration < 2)
                {
                    recoveryFolder = string.Format("{0}\\{1}", recoveryFolder, StaticInfo.SecondCycleFolder);
                    goto GET_FILE_LIST;
                }
            }
            catch (Exception ex)
            {
                msg = string.Format("Failed to enqueue the recovery files. Recovery Folder  : {0}", recoveryFolder);
                log.Error(msg, ex);
            }
            return fileQ;
        }

        /// <summary>
        /// To delete the empty date & hour folders from recovery folder
        /// </summary>
        /// <param name="directory">Directory path which needs to be deleted</param>
        /// <param name="log">Thread specific logger</param>
        private void DeleteEmptyDirectory(string directory, ILog log)
        {
            try
            {
                log.Info("Inside Methosd");
                if (Directory.GetFiles(directory).Count() == 0 && Directory.GetDirectories(directory).Count() == 0)
                {
                    Directory.Delete(directory);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error Occured While Deleting Empty Folder. Folder Path is {0}", directory);
                log.Error(ex);
            }
        }

        /// <summary>
        /// To delete the processed recovery file
        /// </summary>
        /// <param name="file">Recovery file path</param>
        /// <param name="log">Thread specific logger</param>
        private void DeleteProcessedFile(string file, ILog log)
        {
            try
            {
                log.Info("Inside Method");
                File.Delete(file);
                log.Info("Call Data Recovery File Deleted Successfully");
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error Occured While Deleting a Processed File. File Path is {0}", file);
                log.Error(ex);
            }
        }

        /// <summary>
        /// To validate the date level folders in recovery folder
        /// </summary>
        /// <param name="log">Thread specific logger</param>
        /// <param name="dateDir">Date directory path</param>
        /// <returns></returns>
        private bool IsValidDateDirectory(ILog log, string dateDir)
        {
            DateTime date;

            if (!DateTime.TryParseExact(Path.GetFileName(dateDir), "yyyyMMdd", _fp, System.Globalization.DateTimeStyles.None, out date))
            {
                string msg = string.Format("Invalid Date folder appears under Recovery Folder should be removed : {0}", dateDir);
                log.Warn(msg);
                return false;
            }
            return true;
        }

        /// <summary>
        /// To validate the hour level folders reside in date level folder
        /// </summary>
        /// <param name="log">Thread specific logger</param>
        /// <param name="hourDir">Hour directory path</param>
        /// <returns></returns>
        private bool IsValidHourDirectory(ILog log, string hourDir)
        {
            int hour;
            string msg = string.Empty;

            if (!Int32.TryParse(Path.GetFileName(hourDir), out hour))
            {
                msg = string.Format("Invalid hour folder appears under Recovery Folder should be removed : {0}", hourDir);
                log.Warn(msg);
                return false;
            }

            if (hour < 0 || hour > 23)
            {
                msg = string.Format("Invalid hour folder appears under Recovery Folder should be removed : {0}", hourDir);
                log.Warn(msg);
                return false;
            }

            return true;
        }

        /// <summary>
        /// To move the recovery file to invalid xml, second cycle or unrecoverable folder in case of call data import failure
        /// </summary>
        /// <param name="log">Thread specific logger</param>
        /// <param name="file">Recovery file path</param>
        /// <param name="failureMode">Call data imporation failure mode</param>
        /// <param name="recoverySetting">FileReaderSetting in the configuration</param>
        private void MoveRecoveryFiles(ILog log, string file, ImportationFailureMode failureMode, FileReaderSetting recoverySetting)
        {
            try
            {
                log.Info("Inside Method");
                string folder = string.Empty;
                string filePrefix = string.Empty;

                switch (failureMode)
                {
                    case ImportationFailureMode.BadXml:
                    case ImportationFailureMode.InvalidXml:
                    case ImportationFailureMode.InvalidData:
                        if (failureMode == ImportationFailureMode.BadXml)
                            filePrefix = "BAD_";
                        else if (failureMode == ImportationFailureMode.InvalidData)
                            filePrefix = "INVALID_";
                        folder = string.Format("{0}\\{1}\\{2}", recoverySetting.InvalidXmlFolder, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HH"));
                        break;
                    case ImportationFailureMode.DuplicateXml:
                        folder = string.Format("{0}\\{1}\\{2}", recoverySetting.InvalidDBRequestFolder, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HH"));
                        break;
                    default:
                        FileInfo fileInfo = new FileInfo(file);
                        string dirName = fileInfo.Directory.Parent.Parent.Name;

                        if (DateTime.Now.Subtract(fileInfo.CreationTime).TotalHours < _fileRecoveryMaxTime) return;

                        if (dirName.Equals(StaticInfo.SecondCycleFolder, StringComparison.CurrentCultureIgnoreCase))
                        {
                            folder = string.Format("{0}\\{1}\\{2}\\{3}", recoverySetting.RecoveryFolder, StaticInfo.UnRecoverableFolder, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HH"));
                        }
                        else
                        {
                            folder = string.Format("{0}\\{1}\\{2}\\{3}", recoverySetting.RecoveryFolder, StaticInfo.SecondCycleFolder, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HH"));
                        }
                        break;
                }

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string fileNameWithExt = System.IO.Path.GetFileName(file);
                string newFilePath = string.Format("{0}\\{1}{2}", folder, filePrefix, fileNameWithExt);

                int fileCount = Directory.GetFiles(folder).Length + 1;
                while (File.Exists(newFilePath))
                {
                    newFilePath = string.Format("{0}\\{1}{2}_{3}_{4}", folder, filePrefix, fileCount.ToString("######"), (new Random()).Next(1000, 9999), fileNameWithExt);
                }

                File.Move(file, newFilePath);

                log.InfoFormat("Recovery Failed Data Moved From {0} [TO] {1}", file, newFilePath);
            }
            catch (Exception ex)
            {
                log.Error("Recovery Failed - Call Data Movement is Failed", ex);
            }
        }

    }
}
