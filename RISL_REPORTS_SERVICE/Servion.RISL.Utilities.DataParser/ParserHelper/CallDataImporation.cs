using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Servion.RISL.Utilities.DataImport
{
    public class CallDataImporation : IDisposable
    {
        #region Private Members
        private string _applicationName;
        private string _applicationThreadName;
        private string _applicationServer;

        private log4net.ILog _log = null;
        private bool _isDebugLogEnabled = false;

        private List<string> _validationErrMsgs = null;
        private CallDataValidator _xsdValidator = null;
        private DataImportSettings _dataImportSetting = null;
        private Dictionary<string, string> _appXsd = null;
        private DBHelper _dbHelper = null;
        private MsmqHelper _msmqHelper = null;
        #endregion

        #region Constructor
        public CallDataImporation(Dictionary<string, string> importSettings, log4net.ILog log)
        {
            _validationErrMsgs = new List<string>();
            _appXsd = new Dictionary<string, string>();
            _xsdValidator = new CallDataValidator();

            _log = log;
            _isDebugLogEnabled = _log.IsDebugEnabled;
            _dataImportSetting = System.Configuration.ConfigurationManager.GetSection("DataImportSettings") as DataImportSettings;
            _applicationName = importSettings["ApplicationName"];
            _applicationServer = importSettings["ApplicationServer"];
            _applicationThreadName = importSettings["ApplicationThreadName"];
            _dbHelper = new DBHelper(importSettings["ConnectionString"], Convert.ToInt32(importSettings["CommandTimeout"]));
            _msmqHelper = new MsmqHelper(importSettings["QueueName"]);

            this.LoadApplicationSchema();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// To import the call data received from recovery service
        /// </summary>
        /// <param name="callData">call data read from recovery files</param>
        /// <returns></returns>
        public ImportationResponse Import(string callData)
        {
            ImportationResponse response = new ImportationResponse();
            string message = string.Empty;
            string callId = string.Empty;
            string sessionId = string.Empty;
            string appId = string.Empty;
            DateTime callDtTime = System.DateTime.Now;
            try
            {
                _log.Info("Inside Method - Recovery File Import");


                string regExPattern = @"<APP_ID>(?<AppID>.*?)</APP_ID>";
                Regex regEx = new Regex(regExPattern, RegexOptions.Multiline);
                MatchCollection mc = regEx.Matches(callData);

                if (mc.Count > 0)
                {
                    appId = mc[0].Groups["AppID"].Value.Trim();
                }

                regExPattern = @"<CALLID>(?<CallID>.*?)</CALLID>";
                regEx = new Regex(regExPattern, RegexOptions.Multiline);
                mc = regEx.Matches(callData);

                if (mc.Count > 0)
                {
                    callId = mc[0].Groups["CallID"].Value.Trim();
                }

                regExPattern = @"<SESSION_ID>(?<SessionID>.*?)</SESSION_ID>";
                regEx = new Regex(regExPattern, RegexOptions.Multiline);
                mc = regEx.Matches(callData);

                if (mc.Count > 0)
                {
                    sessionId = mc[0].Groups["SessionID"].Value.Trim();
                }

                response = Import(callId, sessionId, appId, callData, callDtTime);
            }
            catch (Exception ex)
            {
                message = string.Format("{0}, Call ID : {1}", ex.Message, callId);
                response.FailureMode = ImportationFailureMode.ApplicationFailed;
                response.ErrorMessage = message;
                _log.Error(ex);
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
        public ImportationResponse Import(string callId, string sessionId, string appId, string callData, DateTime callDateTime)
        {
            ImportationResponse response = new ImportationResponse();
            string message = string.Empty;
            try
            {
                _log.Info("Inside Method ");

                if (!HasBasicCallInfo(callData, appId, callId, ref response)) return response;

                if (!HasAppImportSetings(callId, appId, ref response)) return response;

                if (!HasValidCallData(callData, callId, appId, ref response)) return response;

                IvrReportData ivrData = ParseCallData(callData, appId, callId, ref response);

                if (response.FailureMode == ImportationFailureMode.None)
                {
                    ImportCallData(callId, appId, sessionId, callData, callDateTime, ivrData, ref response);
                }
            }
            catch (Exception ex)
            {
                message = string.Format("{0}, Call ID : {1}", ex.Message, callId);
                response.FailureMode = ImportationFailureMode.ApplicationFailed;
                response.ErrorMessage = message;
                _log.Error(ex);
            }
            finally
            {
                UpdateIvrCallDataStatus(callData, string.Empty, appId, callId, sessionId, callDateTime, response.FailureMode);
            }
            return response;
        }


        /// <summary>
        /// To get the call data from MsMq
        /// </summary>
        /// <returns></returns>
        public List<IvrCallDataInfo> GetCallData()
        {
            string errorCode = string.Empty;
            string errorDesc = string.Empty;
            List<IvrCallDataInfo> ivrcallData = _msmqHelper.GetCallData(out errorCode, out errorDesc);
            if (_isDebugLogEnabled) _log.DebugFormat("Error Code : {0}, Error Description : {1}, ", errorCode, errorDesc);
            return ivrcallData;
        }

        /// <summary>
        /// To get the call data from database
        /// </summary>
        /// <returns></returns>
        public List<IvrCallDataInfo> GetIvrCallData()
        {
            int errorCode = 0;
            string errorDesc = string.Empty;
            List<IvrCallDataInfo> ivrCallDataList = _dbHelper.GetIvrCallData(out errorCode, out errorDesc);
            if (_isDebugLogEnabled) _log.DebugFormat("Error Code : {0}, Error Description : {1}", errorCode, errorDesc);
            return ivrCallDataList;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// To import the parsed call data into reporting database
        /// </summary>
        /// <param name="callId">Call ID from IVR</param>
        /// <param name="appId">Application ID from IVR</param>
        /// <param name="callData">IVR Call Data</param>
        /// <param name="ivrReportData">Parsed Ivr Report Data (i.e. Call Data)</param>
        /// <param name="response">To hold the call data imporation response</param>
        private void ImportCallData(string callId, string appId, string sessionId, string callData, DateTime callDateTime, IvrReportData ivrReportData, ref ImportationResponse response)
        {
            int errorCode = 0;
            string errorDesc = string.Empty;
            string importStatus = string.Empty;
            string message = string.Empty;
            string procName = string.Empty;
            System.Xml.Serialization.XmlSerializer xmlSerialize = null;
            Dictionary<string, string> dicParams = null;
            Stopwatch sWatch = new Stopwatch();
            System.Text.StringBuilder sbSummary = null;
            try
            {
                if (!_dataImportSetting.AppImportSettings[appId].DataImportRequired)
                {
                    _log.Info("Call DataImportRequired flag set to false. So the call data will not be imported into db");
                    return;
                }

                sbSummary = new System.Text.StringBuilder();
                xmlSerialize = new System.Xml.Serialization.XmlSerializer(typeof(IvrReportData));
                xmlSerialize.Serialize(new System.IO.StringWriter(sbSummary), ivrReportData);

                procName = _dataImportSetting.AppImportSettings[appId].DataImportProcedureName;
                dicParams = new Dictionary<string, string>();
                dicParams.Add("CALL_ID", callId);
                dicParams.Add("APP_ID", appId);
                dicParams.Add("SESSION_ID", sessionId);
                dicParams.Add("CALL_DATA", callData);
                dicParams.Add("CALL_DATETIME", callDateTime.ToString("MM/dd/yyyy hh:mm:ss"));
                dicParams.Add("REPORT_DATA", sbSummary.ToString());

                sWatch.Start();
                _dbHelper.ImportIvrCallData(procName, dicParams, out importStatus, out errorCode, out errorDesc);
                sWatch.Stop();
                _log.InfoFormat("Total time taken for insertion of data into database :{0}, Status :{1}", sWatch.Elapsed, importStatus);

                if (importStatus.Equals("IMPORTED", StringComparison.InvariantCultureIgnoreCase))
                {
                    response.HasImported = true;
                }
                else
                {
                    response.HasImported = false;
                    response.ErrorMessage = errorDesc;
                    response.ErrorCode = errorCode;
                    response.FailureMode = ImportationFailureMode.ImportFailed;
                }
            }
            catch (Exception ex)
            {
                message = string.Format("{0}, Call ID : {1}", ex.Message, callId);
                response.FailureMode = ImportationFailureMode.ImportFailed;
                response.ErrorMessage = message;
                _log.Error(ex);
            }
            finally
            {
                UpdateIvrCallDataStatus(callData, sbSummary.ToString(), appId, callId, sessionId, callDateTime, response.FailureMode);
                if (dicParams != null) dicParams.Clear();
                sbSummary.Remove(0, sbSummary.Length);
                dicParams = null;
                sWatch = null;
            }
        }

        /// <summary>
        /// To parse the call data
        /// </summary>
        /// <param name="callData">IVR Call Data</param>
        /// <param name="appId">IVR Application ID</param>
        /// <param name="callId">IVR Call ID</param>
        /// <param name="response">To hold the call data imporation response</param>
        /// <returns></returns>
        private IvrReportData ParseCallData(string callData, string appId, string callId, ref ImportationResponse response)
        {
            IvrData ivrData = null;
            IvrReportData reportData = null;
            System.Xml.Serialization.XmlSerializer xmlSerialize = null;
            Stopwatch sWatch = new Stopwatch();
            try
            {
                sWatch.Start();
                xmlSerialize = new System.Xml.Serialization.XmlSerializer(typeof(IvrData));
                ivrData = (IvrData)xmlSerialize.Deserialize(new System.IO.StringReader(callData));
                reportData = new IvrReportData(ivrData);
                reportData.AdditionalCallInformation.ApplicationName = _applicationName;
                reportData.AdditionalCallInformation.ApplicationThreadName = _applicationThreadName;
                reportData.AdditionalCallInformation.ApplicationServer = _applicationServer;
                sWatch.Stop();
                if (_isDebugLogEnabled) _log.DebugFormat("Total time taken to parse the call data {0}", sWatch.Elapsed);
            }
            catch (Exception ex)
            {
                response.HasImported = false;
                response.FailureMode = ImportationFailureMode.ParserFailed;
                response.ErrorMessage = ex.Message;
                _log.Error("Error occured while parsing the process", ex);
            }
            finally
            {
                xmlSerialize = null;
                sWatch = null;
            }
            return reportData;
        }

        /// <summary>
        /// To load the various xsd schema files into dictionary obect
        /// </summary>
        /// <returns></returns>
        private bool LoadApplicationSchema()
        {
            _log.Info("Inside Method");

            _appXsd.Clear();

            foreach (AppImportSetting setting in _dataImportSetting.AppImportSettings)
            {
                string xsdFile = string.Format("{0}\\XSD\\{1}", AppDomain.CurrentDomain.BaseDirectory, setting.XsdFile);
                _appXsd.Add(setting.ID, System.IO.File.ReadAllText(xsdFile));
            }

            return true;
        }

        /// <summary>
        /// To verify the basic call information in ivr call data
        /// </summary>
        /// <param name="callData">IVR Call Data</param>
        /// <param name="appId">IVR Application ID</param>
        /// <param name="callId">IVR Call ID</param>
        /// <param name="response">To hold the call data imporation response</param>
        /// <returns></returns>
        private bool HasBasicCallInfo(string callData, string appId, string callId, ref ImportationResponse response)
        {
            _log.Debug("Inside Method");
            string message = string.Empty;
            string appIdFromCallData = string.Empty;
            string callIdFromCallData = string.Empty;

            if (string.IsNullOrEmpty(callData))
            {
                message = string.Format("Call ID is empty, App ID : {0}, Call Data : {1}", appId, callData);
                response.FailureMode = ImportationFailureMode.InvalidData;
                response.ErrorMessage = message;
                _log.Error(message);
                return false;
            }

            if (string.IsNullOrEmpty(appId))
            {
                message = string.Format("Application ID is empty, Call ID : {0}", callId);
                response.FailureMode = ImportationFailureMode.InvalidData;
                response.ErrorMessage = message;
                _log.Error(message);
                return false;
            }

            if (string.IsNullOrEmpty(callData))
            {
                message = string.Format("Call Data is empty, Call ID : {0}", callId);
                response.FailureMode = ImportationFailureMode.InvalidXml;
                response.ErrorMessage = message;
                _log.Error(message);
                return false;
            }

            string regExPattern = @"<APP_ID>(?<AppID>.*?)</APP_ID>";
            Regex regEx = new Regex(regExPattern, RegexOptions.Multiline);
            MatchCollection mc = regEx.Matches(callData);

            if (mc.Count == 0)
            {
                message = "App ID missing in xml packet";
                response.FailureMode = ImportationFailureMode.InvalidData;
                response.ErrorMessage = message;
                _log.Error(message);
                return false;
            }

            appIdFromCallData = mc[0].Groups["AppID"].Value.Trim();

            if (string.IsNullOrEmpty(appIdFromCallData))
            {
                message = "App ID Node is empty in xml packet";
                response.FailureMode = ImportationFailureMode.InvalidData;
                response.ErrorMessage = message;
                _log.Error(message);
                return false;
            }

            if (!appIdFromCallData.Equals(appId))
            {
                message = "App ID mismatch with xml packet";
                response.FailureMode = ImportationFailureMode.InvalidData;
                response.ErrorMessage = message;
                _log.Error(message);
                return false;
            }

            regExPattern = @"<CALLID>(?<CallID>.*?)</CALLID>";
            regEx = new Regex(regExPattern, RegexOptions.Multiline);
            mc = regEx.Matches(callData);

            if (mc.Count == 0)
            {
                message = "Call ID missing in xml packet";
                response.FailureMode = ImportationFailureMode.InvalidData;
                response.ErrorMessage = message;
                _log.Error(message);
                return false;
            }

            callIdFromCallData = mc[0].Groups["CallID"].Value.Trim();

            if (string.IsNullOrEmpty(callIdFromCallData))
            {
                message = "Call ID Node is empty";
                response.FailureMode = ImportationFailureMode.InvalidData;
                response.ErrorMessage = message;
                _log.Error(message);
                return false;
            }

            if (!callIdFromCallData.Equals(callId))
            {
                message = "Call ID mismatch with xml packet";
                response.FailureMode = ImportationFailureMode.InvalidData;
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
        private bool HasAppImportSetings(string callId, string appId, ref ImportationResponse response)
        {
            string message = string.Empty;

            if (_dataImportSetting.AppImportSettings[appId] == null)
            {
                message = string.Format("App import setting missing in configuration. App ID : {0}, Call ID : {1}", appId, callId);
                response.FailureMode = ImportationFailureMode.InvalidConfig;
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
        private bool HasValidCallData(string callData, string callId, string appId, ref ImportationResponse response)
        {
            Stopwatch sWatch = new Stopwatch();
            bool isValid = false;
            string message = string.Empty;

            if (_dataImportSetting.AppImportSettings[appId].XsdValidationRequired)
            {
                _validationErrMsgs.Clear();
                sWatch.Start();
                isValid = _xsdValidator.Validate(callData, _appXsd[appId], ref _validationErrMsgs);
                sWatch.Stop();
                if (_isDebugLogEnabled) _log.DebugFormat("Total time taken to validate the call data : {0}", sWatch.Elapsed);

                if (isValid)
                {
                    _log.InfoFormat("Call data schema validation successfull for call Id : {0}", callId);
                }
                else
                {
                    message = _validationErrMsgs.Count == 0 ? string.Empty : string.Format("Invalid Call Data Against XSD. Call ID : {0}, \n Validation Error : {1}", callId, string.Join("\n", _validationErrMsgs.ToArray()));
                    response.FailureMode = ImportationFailureMode.InvalidXml;
                    response.ErrorMessage = message;
                    _log.Error(message);
                    return false;
                }
            }
            else
            {
                _log.InfoFormat("Call data schema validation is not enabled. Call ID : {0}", callId);
            }
            return true;
        }

        /// <summary>
        /// To update the ivr call data status in database
        /// </summary>
        /// <param name="callData">IVR Call Data</param>
        /// <param name="appId">IVR Application ID</param>
        /// <param name="callId">IVR Call ID</param>
        /// <param name="failureMode">Call Data Imporation Response</param>
        private void UpdateIvrCallDataStatus(string callData, string reportdata, string appId, string callId, string sessionId, DateTime callDateTime, ImportationFailureMode failureMode)
        {
            int errorCode = 0;
            string errorDesc = string.Empty;
            Dictionary<string, string> dicParams = null;
            try
            {
                _log.Info("Inside Method");
                if (failureMode == ImportationFailureMode.ImportFailed || failureMode == ImportationFailureMode.None) return;

                dicParams = new Dictionary<string, string>();
                dicParams.Add("CALL_ID", callId);
                dicParams.Add("SESSION_ID", sessionId);
                dicParams.Add("APP_ID", appId);
                dicParams.Add("CALL_DATA", callData);
                dicParams.Add("REPORT_DATA", reportdata);
                dicParams.Add("CALL_DATETIME", callDateTime.ToString("MM/dd/yyyy hh:mm:ss"));
                dicParams.Add("STATUS", failureMode == ImportationFailureMode.InvalidXml ? "I" : "F");
                dicParams.Add("PROCESS_STATUS", failureMode.ToString());
                dicParams.Add("PROCESS_FAILUREREASON", failureMode.ToString());
                dicParams.Add("PROCNAME", _dataImportSetting.AppImportSettings[appId].DataImportStatusProcedureName.ToString());
                _dbHelper.UpdateIvrCallDataStatus(dicParams, out errorCode, out errorDesc);
                if (_isDebugLogEnabled) _log.DebugFormat("Error Code : {0}, Error Description : {1}", errorCode, errorDesc);
            }
            catch (Exception ex)
            {
                _log.Error("Error occured while updating the status of Ivr Call Data", ex);
            }
            finally
            {
                if (dicParams != null) dicParams.Clear(); dicParams = null;
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
            if (_validationErrMsgs != null) _appXsd.Clear();
            _validationErrMsgs = null;
            _dataImportSetting = null;
            _dbHelper = null;
            _log = null;
            _appXsd = null;
            _xsdValidator = null;
        }

        #endregion
    }
}
