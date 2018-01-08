using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using log4net;
using System.Threading;
using System.Messaging;
using System.Xml.Linq;
using System.ServiceModel.Activation;
using System.Diagnostics;
using System.IO;
using System.Configuration;


namespace MSMQ_RFService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "MsmqService" in code, svc and config file together.
    [ServiceBehavior]
    public class MsmqService : IMsmqService
    {
        #region private variables
        private readonly ILog log = LogManager.GetLogger(typeof(MsmqService));
        DateTime startTime;
        #endregion

        public MsmqService()
        {
        }

     
        public MsmqResponse InsertCallDataToQueue(string callData)
        {

          //  string callData = objcallData.callData;

            /*-------------------This is for Thai character testing-----------------------*/
            log.Info("Inside Method");

            #region private variables
            string message = string.Empty;
            CallDataValidation dataValidation = null;
            MsmqResponse _msmqresponse = null;
            ValidataionResponse validateResponse = null;
            #endregion

            try
            {
                log.Info("Inside Method-Before Invoking PutMessageOnMSQ Method");
                #region private variables
                _msmqresponse = new MsmqResponse();
                dataValidation = new CallDataValidation();
                startTime = DateTime.Now;
                log.InfoFormat("Start Date & Time : {0} , Start Time : {1} in milliseconds \n\n", startTime, startTime.Millisecond);
                #endregion
                //validateResponse = dataValidation.Validate(callData.Replace("<?xml version=\"1.0\" encoding=\"UTF-16\"", ""));
                callData = callData.Replace("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>", "").Replace("<?xml version=\"1.0\" encoding=\"UTF-16\"?>", "");
                log.InfoFormat("calldata replaced by encoding format with empty value, calldata - {0}", callData);
                validateResponse = dataValidation.Validate(callData);
                if (validateResponse.HasQueued)
                {
                    _msmqresponse.errorCode = validateResponse.ErrorCode;
                    _msmqresponse.errorDesc = validateResponse.ErrorMessage;
                    _msmqresponse.errorMode = validateResponse.FailureMode.ToString();
                    log.InfoFormat("Call Data Queued Successfull in msmq. Failure Mode: {0}, Error Code : {1}, Error Message : {2}", validateResponse.FailureMode, validateResponse.ErrorCode, validateResponse.ErrorMessage);
                }
                else
                {
                    _msmqresponse.errorCode = validateResponse.ErrorCode;
                    _msmqresponse.errorDesc = validateResponse.ErrorMessage;
                    _msmqresponse.errorMode = validateResponse.FailureMode.ToString();
                    log.InfoFormat("Call Data Failed to Queue in msmq. Failure Mode : {0}, Error Code : {1}, Error Message : {2}", validateResponse.FailureMode, validateResponse.ErrorCode, validateResponse.ErrorMessage);
                }
                return _msmqresponse;
            }
            catch (Exception ex)
            {
                message = string.Format("{0}, Call Data : {1} \n\n", ex.Message, callData);
                _msmqresponse.errorCode = (int)ValidationFailureMode.ApplicationFailed;
                _msmqresponse.errorDesc = message;
                _msmqresponse.errorMode = ValidationFailureMode.ApplicationFailed.ToString();
                log.ErrorFormat("Error. Failure Mode : {0},Error Code : {1}, Error Message : {2}", _msmqresponse.errorMode, _msmqresponse.errorCode, _msmqresponse.errorDesc);

                // This will write the file into recovery path
                return _msmqresponse;
            }
            finally
            {
                _msmqresponse = null;
                dataValidation = null;
                validateResponse = null;
                message = string.Empty;
                log.InfoFormat("Total time taken to complete process : {0} milliseconds \n\n", DateTime.Now.Subtract(startTime).TotalMilliseconds);
            }
        }


    }
}
