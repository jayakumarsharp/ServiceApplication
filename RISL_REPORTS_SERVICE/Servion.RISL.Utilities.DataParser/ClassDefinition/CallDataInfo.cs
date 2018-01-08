using System;

namespace Servion.RISL.Utilities.DataImport
{
    public class IvrCallDataInfo
    {
        public string CallID { get; set; }
        public string SessionID { get; set; }
        public string ApplicationID { get; set; }
        public string CallData { get; set; }
        public DateTime CallDateTime { get; set; }
        public string Status { get; set; }

        public string QueueMsgId { get; set; }

        public IvrCallDataInfo()
        {
        }

        public IvrCallDataInfo(string callId, string sessionID, string appId, string callData, DateTime dateTime, string status, string queuemsgid)
        {
            CallID = callId;
            SessionID = sessionID;
            ApplicationID = appId;
            CallData = callData;
            CallDateTime = dateTime;
            Status = status;
            QueueMsgId = queuemsgid;
        }

       
    }

    public static class StaticParams
    {
        public static System.Messaging.MessageQueue mq = null;
    }
}
