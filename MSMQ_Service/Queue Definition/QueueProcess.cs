using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Messaging;
using log4net;
using System.IO;
using System.Configuration;


namespace MSMQ_RFService
{
    public class QueueProcess : IDisposable
    {
        private readonly ILog log = LogManager.GetLogger(typeof(QueueProcess));

        public void PutMessageOnMSQ(string queueName, string callId, string callData,string appid, ref ValidataionResponse response)
        {
            #region private variables
            string message = string.Empty;
            DateTime startTime = DateTime.Now;
            System.Messaging.Message mm = null;
            #endregion

            log.InfoFormat("Started Enqueue Process -Start Time in:{0}  \n\n", startTime);
            try
            {
                if (InitailContext.mq == null)
                {
                    if (!MessageQueue.Exists(queueName))
                    {
                        log.DebugFormat("{0} - Queue does not exist", queueName);
                        MessageQueue.Create(queueName, true);
                     
                    }
                    InitailContext.mq = new System.Messaging.MessageQueue(queueName);
                    InitailContext.mq.SetPermissions("Users",MessageQueueAccessRights.FullControl,AccessControlEntryType.Allow);
                    InitailContext.mq.Authenticate = false;
                }
                mm = new System.Messaging.Message();
                mm.Body = callData;
                mm.Label = "QueueData";
                log.InfoFormat("Enqueued Details:{0}", mm.Body.ToString());
                //mm.TimeToReachQueue = new TimeSpan(0, 0,InitailContext.queueTimeout);
                //log.InfoFormat("Timeout {0} seconds", InitailContext.queueTimeout);
                if (InitailContext._dataConfigSetting.AppConfigSettings[appid].IsTransactionEnabled.ToString().ToUpper() == "Y")
                {
                    log.Debug("Transaction Enabled");
                    InitailContext.mq.Send(mm, "QueueData", MessageQueueTransactionType.Single);
                }
                else
                {
                    log.Debug("Transaction not Enabled");
                    InitailContext.mq.Send(mm);
                }
                response.HasQueued = true;
                response.ErrorCode = 0;
                response.ErrorMessage = "Success";
                response.FailureMode = ValidationFailureMode.None;
                InitailContext.mq.Close();
                log.InfoFormat("Completed Enquee Process,Total Time taken:{0} milliseconds \n\n", DateTime.Now.Subtract(startTime).Milliseconds);
            }
            catch (Exception ex)
            {
                message = string.Format("{0}, Call ID : {1}", ex.Message, callId);
                response.HasQueued = false;
                response.ErrorCode = (int)ValidationFailureMode.QueueInsertFailed;
                response.FailureMode = ValidationFailureMode.QueueInsertFailed;
                response.ErrorMessage = message;
                log.ErrorFormat("Error : Failure Code : {0}, Error Code : {1}, Error Desc : {2} \n\n", response.FailureMode, response.ErrorCode, response.ErrorMessage);

                try
                {
                    //log.Info("Start writing file into Recovery folder");
                    //string folder = string.Format("{0}\\{1}\\{2}", ConfigurationManager.AppSettings["RecoveryFolder"].ToString(), DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HH"));
                    //log.Debug("finished getting folder path");
                    //if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    ////int fileCount = Directory.GetFiles(folder).Length + 1;
                    //log.Debug("finished getting file count");
                    
                    //string filePath = string.Format("{0}\\{1}_{2}_{3}.txt", folder, "MSMQ",DateTime.Now.Ticks.ToString(), (new Random()).Next(1000, 99999));
                    //log.Debug("finished writing file path");
                    //if (!File.Exists(filePath))
                    //{
                    //    log.Info("File is Exists");
                    //    File.WriteAllText(filePath, callData);
                    //    log.InfoFormat("Call Data with Callid : {0} , has Written to the file File Path : {1}", callId, filePath);
                    //}
                    //else
                    //{
                    //    log.DebugFormat("Filepath is not Exists {0}", filePath);
                    //}

                    //log.Debug("finished writing teh file into recovery folder");

                    MSMQ_RFService.CallDataValidation.delWriteFileIntoRecovery objwriteFile = new MSMQ_RFService.CallDataValidation.delWriteFileIntoRecovery(WriteIntoRecovery);
                    objwriteFile.BeginInvoke(callData, callId, null, null);

                }
                catch (Exception exp)
                {
                    log.ErrorFormat("Error while writing File :{0}", exp);
                }

            }
            finally
            {
                mm.Dispose();
                mm = null;
                message = string.Empty;
               
            }
        }
        public void WriteIntoRecovery(string calldata, string callId)
        {
            try
            {
                string folder = string.Format("{0}\\{1}\\{2}", ConfigurationManager.AppSettings["RecoveryFolder"].ToString(), DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HH"));

                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                //int fileCount = Directory.GetFiles(folder).Length + 1;

                string filePath = string.Format("{0}\\{1}_{2}_{3}.txt", folder, "MSMQ", DateTime.Now.Ticks.ToString(), (new Random()).Next(1000, 99999));

                if (!File.Exists(filePath))
                {
                    File.WriteAllText(filePath, calldata);
                    log.InfoFormat("Call Data with Callid : {0} , has Written to the file File Path : {1}", callId, filePath);

                }

            }
            catch (Exception exp)
            {
                log.ErrorFormat("Error while writing File :{0}", exp);
            }
        }

        #region IDisposable Members
        /// <summary>
        /// To destroy the objects by implementing IDisposable interface method
        /// </summary>
        public void Dispose()
        {

        }

        #endregion

    }
}




