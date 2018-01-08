using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using log4net;
using Servion.RISL.Utilities.DataImport;

namespace Servion.RISL.Services.DataUpload
{
    class DataReader
    {
        
        /// <summary>
        /// To process the call data from database
        /// </summary>
        /// <param name="uploadSetting">DataReaderSettings from the configuration file</param>
        internal void StartUpload(object uploadSetting)
        {
            DataReaderSetting readerSetting = null;
            ILog log = null;
            CallDataImporation dataImporation = null;
            IvrCallDataInfo callDataInfo = null;
            Dictionary<string, string> dicImportSettings = null;
            Stopwatch sWatch = new Stopwatch();
            Stopwatch swProcess = new Stopwatch();
            try
            {
                Logger.Log.Info("inside Method");
                readerSetting = uploadSetting as DataReaderSetting;
                log = LogManager.GetLogger(readerSetting.LoggerName);

                bool writeXmlToLog = readerSetting.WriteXmlToLog;
                int maxWaitTime = readerSetting.MaxThreadSleepTime;
                int sleepIncrTime = readerSetting.SleepTimeIncrement;
                int currentWaitTime = 1;

                string connStr = ConfigurationManager.ConnectionStrings[readerSetting.ConnectionStringName].ConnectionString;
                string queueName = readerSetting.QueueName;
                string tempFolder = StaticInfo.TempXmlPacketFolder;
                bool hasTempFolder = !string.IsNullOrEmpty(tempFolder);

                dicImportSettings = new Dictionary<string, string>();
                dicImportSettings.Add("ApplicationName", StaticInfo.ApplicationName);
                dicImportSettings.Add("ApplicationServer", StaticInfo.ApplicationServer);
                dicImportSettings.Add("ApplicationThreadName", Thread.CurrentThread.Name);
                dicImportSettings.Add("ConnectionString", connStr);
                dicImportSettings.Add("CommandTimeout", StaticInfo.CommandTimeout.ToString());
                dicImportSettings.Add("QueueName", queueName);
                dataImporation = new CallDataImporation(dicImportSettings, log);


                while (StaticInfo.CanContinue)
                {
                    try
                    {
                        #region Reading Call Data from Database
                                                
                        sWatch.Start();
                        List<IvrCallDataInfo> callInfoList = dataImporation.GetCallData();
                        sWatch.Stop();
                        log.InfoFormat("Total time taken to read the data from DB : {0}, ", sWatch.Elapsed);

                        if (callInfoList.Count == 0)
                        {
                            log.InfoFormat("There is no data in queue. Putting thread to sleep for {0} seconds", currentWaitTime);
                            Thread.Sleep(currentWaitTime * 1000);
                            if (currentWaitTime < maxWaitTime) currentWaitTime++;
                            continue;
                        }
                        #endregion

                        currentWaitTime = 1;

                        foreach (IvrCallDataInfo callInfo in callInfoList)
                        {
                            try
                            {
                                swProcess.Start();
                                callDataInfo = callInfo;

                                #region Log The Call Data Into Temp Folder / Log File For Testing Purpose
                                //To write the call data into log file if it is enabled
                                if (writeXmlToLog)
                                {
                                    log.InfoFormat("Call Data : {0},Queue Data Msg Id : {1}", callDataInfo.CallData, callDataInfo.QueueMsgId);
                                    
                                }

                                //To check whether the temp folder configured in the configuration file
                                if (hasTempFolder)
                                {
                                    try
                                    {
                                        if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);

                                        int fc = Directory.GetFiles(tempFolder).Count();
                                        fc++;
                                        string f = string.Format("calldata_{0}_{1}_{2}_{3}.xml", fc, DateTime.Now.ToString("HHmmssfff"), new Random().Next(100), new Random().Next(999));
                                        string fp = string.Format("{0}\\{1}", tempFolder, f);
                                        File.AppendAllText(fp, callDataInfo.CallData);
                                    }
                                    catch (Exception e)
                                    {
                                        log.Error("Problem occured while writting the call data into temp folder", e);
                                    }
                                }
                                #endregion

                                #region Data Import Process
                                sWatch.Start();
                                ImportationResponse importResponse = dataImporation.Import(callDataInfo.CallData);
                                //ImportationResponse importResponse = dataImporation.Import(callDataInfo.CallID, callDataInfo.SessionID, callDataInfo.ApplicationID, callDataInfo.CallData, callDataInfo.CallDateTime);
                                sWatch.Stop();

                                swProcess.Stop();

                                if (importResponse.HasImported)
                                {
                                    log.Info("Call Data Imported Successfully");
                                    log.InfoFormat("Total time taken to process at data importation : {0}, Total process time : {1} milli seconds", sWatch.Elapsed, swProcess.Elapsed);   
                                }
                                else
                                {
                                    log.ErrorFormat("Call Data Failed To Import. Failure Mode : {0}, Error Code : {1}, Error Message : {2}", importResponse.FailureMode, importResponse.ErrorCode, importResponse.ErrorMessage);
                                    LoadCallDataIntoFile(log, readerSetting, callDataInfo, importResponse.FailureMode);
                                }
                                #endregion
                            }
                            catch (Exception exAtForLoop)
                            {
                                log.Error("Error occured while processing the call data", exAtForLoop);
                                log.ErrorFormat("CALL DATA : {0}", (callDataInfo != null) ? callDataInfo.CallData : "NOT AVAILABLE");
                            }
                        }
                    }
                    catch (Exception exAtInnerWhile)
                    {
                        log.Error("Error occured while processing the db data read activity", exAtInnerWhile);
                        log.ErrorFormat("CALL DATA : {0}", (callDataInfo != null) ? callDataInfo.CallData : "NOT AVAILABLE");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Data upload process failed for the readers setting : {0}" + readerSetting.Name, ex);
            }
            finally
            {
                Interlocked.Decrement(ref StaticInfo.ThreadCount);
                Logger.Log.InfoFormat("Decrementing Thread Count as Method is exiting ... Thread Count {0} ", Interlocked.Read(ref StaticInfo.ThreadCount));
                if (dataImporation != null) dataImporation.Dispose(); dataImporation = null;
                if (dicImportSettings != null) dicImportSettings.Clear(); dicImportSettings = null;
                log = null; readerSetting = null;
            }
        }

        /// <summary>
        /// To save the call data as a recovery file in case of call data import failure
        /// </summary>
        /// <param name="log">Thread specific logger</param>
        /// <param name="readerSetting">DataReaderSetting to get the recovery folder path</param>
        /// <param name="callDataInfo">Ivr call data information received from database</param>
        /// <param name="failureMode">Call data importation failure mode to identify the failure reason</param>
        private void LoadCallDataIntoFile(ILog log, DataReaderSetting readerSetting, IvrCallDataInfo callDataInfo, ImportationFailureMode failureMode)
        {
            try
            {
                log.Info("Inside Method");
                string folder = string.Empty;
                string callId = string.Empty;
                string filePrefix = string.Empty;

                if (string.IsNullOrEmpty(callDataInfo.CallID))
                    callId = "calldata";
                else
                    callId = callDataInfo.CallID;

                switch (failureMode)
                {
                    case ImportationFailureMode.BadXml:
                    case ImportationFailureMode.InvalidXml:
                    case ImportationFailureMode.InvalidData:
                        if(failureMode == ImportationFailureMode.BadXml) 
                            filePrefix = "BAD_";
                        else if(failureMode == ImportationFailureMode.InvalidData) 
                            filePrefix =  "INVALID_";
                        folder = string.Format("{0}\\{1}\\{2}", readerSetting.InvalidXmlFolder, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HH"));
                        break;
                    case ImportationFailureMode.DuplicateXml:
                        folder = string.Format("{0}\\{1}\\{2}", readerSetting.InvalidDBRequestFolder, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HH"));
                        break;
                    default:
                        folder = string.Format("{0}\\{1}\\{2}", readerSetting.RecoveryFolder, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HH"));
                        break;
                }

                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                int fileCount = Directory.GetFiles(folder).Length + 1;

                string filePath = string.Format("{0}\\{1}{2}_{3}_{4}.txt", folder, filePrefix, readerSetting.Name, fileCount, (new Random()).Next(1000, 9999));

                while (File.Exists(filePath))
                {
                    if (log.IsInfoEnabled)
                        log.InfoFormat("Recovery File already exist with this name : {0}", filePath);
                    filePath = string.Format("{0}\\{1}{2}_{3}_{4}.txt", folder, filePrefix, readerSetting.Name, fileCount.ToString("######"), (new Random()).Next(10000, 999999));
                }

                File.WriteAllText(filePath, callDataInfo.CallData);
                log.InfoFormat("Call Data Written to the file, Call ID : {0}, File Path : {1}", callId, filePath);
            }
            catch (Exception ex)
            {
                log.Error("Loading Call Data to Recovery Folder is Failed", ex);
                log.ErrorFormat("Call Data :\n {0} ", callDataInfo.CallData);
            }
        }
    }
}
