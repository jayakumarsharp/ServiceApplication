using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MSMQ_RFService
{
    public enum ValidationFailureMode
    {
        None=1,               //There is no failure
        InvalidData=2,        //If the xml data is mismatched with call id/app id 
        InvalidXml=3,         //Invalid xml against XSD
        InvalidConfig=4,      //Invalid or missing configuration settings
        QueueInsertFailed=5,  //Queue insertion failure
        ApplicationFailed=6,   //Other exceptions
        BadXml = 7,         //If the xml data is empty or not well formed
        XsdFailed = 8      //Xsd validation failure
        //DuplicateXml,       //Call ID alreday exist in database
        
    }

    public class ValidataionResponse
    {
        public ValidataionResponse()
        {
            HasQueued = false;
            FailureMode = ValidationFailureMode.None;
            ErrorCode = 0;
            ErrorMessage = string.Empty;
        }

        public bool HasQueued { get; set; }

        public ValidationFailureMode FailureMode { get; set; }

        public int ErrorCode { get; set; }

        public string ErrorMessage { get; set; }
    }
}