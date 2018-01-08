using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.Text.RegularExpressions;
using log4net;
using System.Threading;
using System.Configuration;
using System.IO;

namespace MSMQ_RFService
{
    
    public class CallDataValidation : IDisposable
    {

         public delegate void delWriteFileIntoRecovery(string calldata,string callid);

        #region Private Members

        private ILog _log = null;
        private List<string> _validationErrMsgs = null;
        private CallDataValidator _xsdValidator = null;
        private QueueProcess _queueInsert = null;
        #endregion

        #region Constructor

        public CallDataValidation()
        {
            _log = LogManager.GetLogger(typeof(CallDataValidation));
            _validationErrMsgs = new List<string>();
            _xsdValidator = new CallDataValidator();
            _queueInsert = new QueueProcess();
        }
     
        #endregion

        #region Public Methods
        /// <summary>
        /// To validate the call data received from Msmq Service
        /// </summary>
        /// <param name="callData">call data read from Msmq Service</param>
        /// <returns></returns>
        public ValidataionResponse Validate(string callData)
        {
            ValidataionResponse response = new ValidataionResponse();
            string message = string.Empty;
            string callId = string.Empty;
            string sessionId = string.Empty;
            string appId = string.Empty;
            DateTime callDtTime = System.DateTime.Now;
            try
            {
                _log.Info("Inside Method");
               
                // string regExPattern =string.Empty;
                //Regex regEx = new Regex(regExPattern, RegexOptions.Multiline);
                //MatchCollection mc = regEx.Matches(callData);

                //appId = string.IsNullOrEmpty(ConfigurationManager.AppSettings["ApplicationId"].ToString()) ? "1" : ConfigurationManager.AppSettings["ApplicationId"].ToString();

                string regExPattern = @"<APP_ID>(?<APP_ID>.*?)</APP_ID>";
                Regex regEx = new Regex(regExPattern, RegexOptions.Multiline);
                MatchCollection mc = regEx.Matches(callData);

                if (mc.Count > 0)
                {
                    appId = mc[0].Groups["APP_ID"].Value.Trim();
                }

                regExPattern = @"<CALLID>(?<CallID>.*?)</CALLID>";
                regEx = new Regex(regExPattern, RegexOptions.Multiline);
                mc = regEx.Matches(callData);

                if (mc.Count > 0)
                {
                    callId = mc[0].Groups["CallID"].Value.Trim();
                }

                regExPattern = @"<SESSION_ID>(?<SESSION_ID>.*?)</SESSION_ID>";
                regEx = new Regex(regExPattern, RegexOptions.Multiline);
                mc = regEx.Matches(callData);

                if (mc.Count > 0)
                {
                    sessionId = mc[0].Groups["SESSION_ID"].Value.Trim();
                }

                response = ValidateData(callId, sessionId, appId, callData, callDtTime);
            }
            catch (Exception ex)
            {
                message = string.Format("{0}, Call ID : {1}", ex.Message, callId);
                response.ErrorCode = (int)ValidationFailureMode.ApplicationFailed;
                response.FailureMode = ValidationFailureMode.ApplicationFailed;
                response.ErrorMessage = message;
                _log.ErrorFormat("Validate Error : Failure Code : {0}, Error Code : {1}, Error Desc : {2}", response.FailureMode, response.ErrorCode, response.ErrorMessage);

                delWriteFileIntoRecovery objwriteFile = new delWriteFileIntoRecovery(WriteIntoRecovery);
                objwriteFile.BeginInvoke(callData, callId, null, null);



            }
            return response;
        }

        /// <summary>
        /// To import the call data into reporting database
        /// </summary>
        /// <param name="callId">Call ID from IVR</param>
        /// <param name="appId">Application ID from IVR</param>
        /// <param name="callData">IVR Call Data</param>
        /// <returns></returns>
        public ValidataionResponse ValidateData(string callId, string sessionId, string appId, string callData, DateTime callDateTime)
        {
            ValidataionResponse response = new ValidataionResponse();
            string message = string.Empty;
            try
            {
                _log.Info("Inside Method ");

                if (!HasBasicCallInfo(callData, appId, callId, ref response)) return response;

                if (!HasAppConfigSetings(callId, appId, ref response)) return response;

                if (!HasValidCallData(callData, callId, appId, ref response)) return response;

                if (response.FailureMode == ValidationFailureMode.None)
                {
                    if (InitailContext._dataConfigSetting.AppConfigSettings[appId].ThreadSleepRequired)
                    {
                        Thread.Sleep(InitailContext.time);
                        _log.InfoFormat("Thread sleep for {0} milliseconds",InitailContext.time);
                    }
                    _log.InfoFormat("Pushing data into queue for callid :{0}", callId);
                    _queueInsert.PutMessageOnMSQ(Convert.ToString(InitailContext._dataConfigSetting.AppConfigSettings[appId].QueueName), callId, callData, appId, ref response);
                    _log.InfoFormat("Enqueued data into queue for callid :{0}", callId);
                }
            }
            catch (Exception ex)
            {
                message = string.Format("{0}, Call ID : {1}", ex.Message, callId);
                response.FailureMode = ValidationFailureMode.ApplicationFailed;
                response.ErrorCode = (int)ValidationFailureMode.ApplicationFailed;
                response.ErrorMessage = message;
                _log.ErrorFormat("ValidateData Error : Failure Code : {0}, Error Code : {1}, Error Desc : {2}", response.FailureMode, response.ErrorCode, response.ErrorMessage);

                try
                {
                    delWriteFileIntoRecovery objwriteFile = new delWriteFileIntoRecovery(WriteIntoRecovery);
                    objwriteFile.BeginInvoke(callData, callId, null, null);

                }
                catch (Exception exp)
                {
                    _log.ErrorFormat("Error while writing File :{0}", exp);
                }


            }
            return response;
        }

   
        #endregion

        #region Private Methods
       

        /// <summary>
        /// To verify the basic call information in ivr call data
        /// </summary>
        /// <param name="callData">IVR Call Data</param>
        /// <param name="appId">IVR Application ID</param>
        /// <param name="callId">IVR Call ID</param>
        /// <param name="response">To hold the call data imporation response</param>
        /// <returns></returns>
        private bool HasBasicCallInfo(string callData, string appId, string callId, ref ValidataionResponse response)
        {
            _log.Info("Inside Method");
            string message = string.Empty;
            string appIdFromCallData = string.Empty;
            string callIdFromCallData = string.Empty;

            if (string.IsNullOrEmpty(callData))
            {
                message = string.Format("Call ID is empty, App ID : {0}, Call Data : {1}", appId, callData);
                response.FailureMode = ValidationFailureMode.InvalidData;
                response.ErrorCode = (int)ValidationFailureMode.InvalidData;
                response.ErrorMessage = message;
                _log.Error(message);
                return false;
            }

            if (string.IsNullOrEmpty(appId))
            {
                message = string.Format("Application ID is empty, Call ID : {0}", callId);
                response.FailureMode = ValidationFailureMode.InvalidData;
                response.ErrorCode = (int)ValidationFailureMode.InvalidData;
                response.ErrorMessage = message;
                _log.Error(message);
                return false;
            }

            if (string.IsNullOrEmpty(callData))
            {
                message = string.Format("Call Data is empty, Call ID : {0}", callId);
                response.FailureMode = ValidationFailureMode.InvalidXml;
                response.ErrorCode = (int)ValidationFailureMode.InvalidXml;
                response.ErrorMessage = message;
                _log.Error(message);
                return false;
            }

            string regExPattern =string.Empty;
            Regex regEx = new Regex(regExPattern, RegexOptions.Multiline);
            MatchCollection mc = regEx.Matches(callData);

            //string regExPattern = @"<APP_ID>(?<AppID>.*?)</APP_ID>";
            //Regex regEx = new Regex(regExPattern, RegexOptions.Multiline);
            //MatchCollection mc = regEx.Matches(callData);


            //if (mc.Count == 0)
            //{
            //    message = "App ID missing in xml packet";
            //    response.FailureMode = ValidationFailureMode.InvalidData;
            //    response.ErrorCode = (int)ValidationFailureMode.InvalidData;
            //    response.ErrorMessage = message;
            //    _log.Error(message);
            //    return false;
            //}

            //appIdFromCallData = mc[0].Groups["AppID"].Value.Trim();

            //if (string.IsNullOrEmpty(appIdFromCallData))
            //{
            //    message = "App ID Node is empty in xml packet";
            //    response.FailureMode = ValidationFailureMode.InvalidData;
            //    response.ErrorCode = (int)ValidationFailureMode.InvalidData;
            //    response.ErrorMessage = message;
            //    _log.Error(message);
            //    return false;
            //}

            //if (!appIdFromCallData.Equals(appId))
            //{
            //    message = "App ID mismatch with xml packet";
            //    response.FailureMode = ValidationFailureMode.InvalidData;
            //    response.ErrorCode = (int)ValidationFailureMode.InvalidData;
            //    response.ErrorMessage = message;
            //    _log.Error(message);
            //    return false;
            //}

            regExPattern = @"<CALLID>(?<CallID>.*?)</CALLID>";
            regEx = new Regex(regExPattern, RegexOptions.Multiline);
            mc = regEx.Matches(callData);

            if (mc.Count == 0)
            {
                message = "Call ID missing in xml packet";
                response.FailureMode = ValidationFailureMode.InvalidData;
                response.ErrorCode = (int)ValidationFailureMode.InvalidData;
                response.ErrorMessage = message;
                _log.Error(message);
                return false;
            }

            callIdFromCallData = mc[0].Groups["CallID"].Value.Trim();

            if (string.IsNullOrEmpty(callIdFromCallData))
            {
                message = "Call ID Node is empty";
                response.FailureMode = ValidationFailureMode.InvalidData;
                response.ErrorCode = (int)ValidationFailureMode.InvalidData;
                response.ErrorMessage = message;
                _log.Error(message);
                return false;
            }

            if (!callIdFromCallData.Equals(callId))
            {
                message = "Call ID mismatch with xml packet";
                response.FailureMode = ValidationFailureMode.InvalidData;
                response.ErrorCode = (int)ValidationFailureMode.InvalidData;
                response.ErrorMessage = message;
                _log.Error(message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// To verify the appliation specific importation settings available for the call data
        /// </summary>
        /// <param name="callId">IVR Call ID</param>
        /// <param name="appId">IVR Application ID</param>
        /// <param name="response">To hold the call data imporation response</param>
        /// <returns></returns>
        private bool HasAppConfigSetings(string callId, string appId, ref ValidataionResponse response)
        {
            string message = string.Empty;

            if (InitailContext._dataConfigSetting.AppConfigSettings[appId] == null)
            {
                message = string.Format("App config setting missing in configuration. App ID : {0}, Call ID : {1}", appId, callId);
                response.FailureMode = ValidationFailureMode.InvalidConfig;
                response.ErrorCode = (int)ValidationFailureMode.InvalidConfig;
                response.ErrorMessage = message;
                _log.Error(message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// To validate the call data against the xsd schema
        /// </summary>
        /// <param name="callData">IVR Call Data</param>
        /// <param name="callId">IVR Call ID</param>
        /// <param name="appId">IVR Application ID</param>
        /// <param name="response">To hold the call data imporation response</param>
        /// <returns></returns>
        private bool HasValidCallData(string callData, string callId, string appId, ref ValidataionResponse response)
        {
            Stopwatch sWatch = new Stopwatch();
            bool isValid = false;
            string message = string.Empty;

            if (InitailContext._dataConfigSetting.AppConfigSettings[appId].XsdValidationRequired)
            {
                _validationErrMsgs.Clear();
                sWatch.Start();
                isValid = _xsdValidator.Validate(callData, InitailContext._appXsd[appId], ref _validationErrMsgs);
                sWatch.Stop();
                _log.InfoFormat("Total time taken to validate the call data : {0}", sWatch.Elapsed);

                if (isValid)
                {
                    _log.InfoFormat("Call data schema validation successfull for call Id : {0}", callId);
                }
                else
                {
                    message = _validationErrMsgs.Count == 0 ? string.Empty : string.Format("Invalid Call Data Against XSD. Call ID : {0}, \n Validation Error : {1}", callId, string.Join("\n", _validationErrMsgs.ToArray()));
                    response.FailureMode = ValidationFailureMode.InvalidXml;
                    response.ErrorCode = (int)ValidationFailureMode.InvalidXml;
                    response.ErrorMessage = message;
                    _log.InfoFormat("Validation Error", message);
                    return false;
                }
            }
            else
            {
                _log.InfoFormat("Call data schema validation is not enabled. Call ID : {0}", callId);
            }
            return true;
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
                    _log.InfoFormat("Call Data with Callid : {0} , has Written to the file File Path : {1}", callId, filePath);

                }

            }
            catch (Exception exp)
            {
                _log.ErrorFormat("Error while writing File :{0}", exp);
            }
        }
      
        #endregion

        #region IDisposable Members
        /// <summary>
        /// To destroy the objects by implementing IDisposable interface method
        /// </summary>
        public void Dispose()
        {
            if (_validationErrMsgs != null) _validationErrMsgs.Clear();
            if (_validationErrMsgs != null) InitailContext._appXsd.Clear();
            _validationErrMsgs = null;
            InitailContext._dataConfigSetting = null;
            _log = null;
            InitailContext._appXsd = null;
            _xsdValidator = null;
            _queueInsert = null;
        }

        #endregion
    }
}