using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;
using System.Xml.Linq;

namespace MSMQ_RFService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IMsmqService" in both code and config file together.
    [ServiceContract]
    public interface IMsmqService
    {
        [OperationContract]
        [WebInvoke(Method = "POST")]
        MsmqResponse InsertCallDataToQueue(string callData);
        
    }
}
