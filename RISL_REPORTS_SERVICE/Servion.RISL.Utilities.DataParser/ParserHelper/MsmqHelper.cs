using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Messaging;
using log4net;
using System.IO;
using System.Data;

namespace Servion.RISL.Utilities.DataImport
{
    class MsmqHelper
    {
        string _queueName = string.Empty;
        /// <summary>
        /// Constructor to initialize the queue name
        /// </summary>
        /// <param name="queueName">Msmq Queue name</param>
        public MsmqHelper(string queueName)
        {
            _queueName = queueName;
        }

        public List<IvrCallDataInfo> GetCallData(out string errorCode, out string errorDesc)
        {
            System.Messaging.Message mm = null;
            List<IvrCallDataInfo> ivrcalldata = new List<IvrCallDataInfo>();
            string messageId = string.Empty;
            string errorcode = string.Empty;
            string errordesc = string.Empty;
            DateTime startTime = DateTime.Now;
            try
            {

                //MessageQueue rmQ = new MessageQueue("FormatName:DIRECT=TCP:172.16.6.47\\Private$\\CallDataMsmq");
                
                //rmQ.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });

                //System.Messaging.Message msg = rmQ.Receive(MessageQueueTransactionType.Single);

                //string msg1=msg.Body.ToString();

                if (!MessageQueue.Exists(_queueName))
                {
                    errorcode = "None";
                    errordesc = string.Format("Specified Queue:{0} name is Not Available for Dequeue", _queueName);
                }
                else
                {
                    //if (StaticParams.mq == null)
                    //{
                    //}

                    StaticParams.mq = new System.Messaging.MessageQueue(_queueName);
                    StaticParams.mq.SetPermissions("Users", MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow);
                    // StaticParams.mq.SetPermissions("Everyone", MessageQueueAccessRights.ReceiveMessage, AccessControlEntryType.Allow);
                    StaticParams.mq.Authenticate = false;

                    //int a = StaticParams.mq.GetAllMessages().Length;
                    if (StaticParams.mq.GetAllMessages().Length > 0)
                    {
                        mm = StaticParams.mq.Receive(MessageQueueTransactionType.Single);
                        mm.Formatter = new XmlMessageFormatter(new String[] { "System.String,mscorlib" });
                        messageId = mm.Id.ToString();
                        IvrCallDataInfo data = new IvrCallDataInfo();
                        data.CallData = mm.Body.ToString();
                        data.QueueMsgId = mm.Id.ToString();
                        data.Status = "Y";
                        data.CallDateTime = startTime;

                        ivrcalldata.Add(data);
                        errorcode = "0";
                        errordesc = "Queue data successfully Dequeued";
                    }
                    else
                    {
                        errorcode = "None";
                        errordesc = string.Format("No data in Queue, Count - {0}", StaticParams.mq.GetAllMessages().Length);
                    }
                    StaticParams.mq.Close();
                }

            }
            catch (Exception ex)
            {
                errorcode = "1";
                errordesc = string.Format("Error in Dequeue process:{0} ", ex);
            }
            finally
            {
                StaticParams.mq = null;
                mm = null;
            }

            //DirectoryInfo info = new DirectoryInfo(@"e:\Servion\file\");
            //FileInfo[] file = info.GetFiles();
            //IvrCallDataInfo calldatainfo = new IvrCallDataInfo();

            //foreach (FileInfo files in file)
            //{
            //    calldatainfo.CallData = File.ReadAllText(files.FullName);
            //}
      
            errorCode = errorcode;
            errorDesc = errordesc;
            return ivrcalldata;

        }




    }
}
