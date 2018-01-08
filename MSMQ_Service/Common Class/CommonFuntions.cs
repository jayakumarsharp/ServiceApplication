using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace MSMQ_RFService
{
    public class MsmqResponse
    {
        [DataMember]
        public int errorCode { get; set; }
        [DataMember]
        public string errorDesc { get; set; }
        [DataMember]
        public string errorMode { get; set; }
       
    }
    public static class InitailContext
    {
         public static Dictionary<string, string> _appXsd = null;
         public static DataConfigSettings _dataConfigSetting = null;
         public static System.Messaging.MessageQueue mq = null;
         public static int time =0;
         public static int queueTimeout = 0;
         
    }

}