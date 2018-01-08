using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Servion.CCA.ApplicationFramework.Data.Sql;
using System.Configuration;

namespace Servion.RISL.Utilities.DataImport
{
    class DBHelper
    {
        private SqlDatabase _sqlDb;

        /// <summary>
        /// Constructor to initialize the database connection & command timeout
        /// </summary>
        /// <param name="connStr">Database connection string</param>
        /// <param name="commandTimeout">Database command timeout</param>
        public DBHelper(string connStr, int commandTimeout)
        {
            _sqlDb = new SqlDatabase(connStr);
            _sqlDb.CommandTimeout = commandTimeout;
        }

        /// <summary>
        /// To get the ivr call data from database (CALL DATA MASTER TABLE)
        /// </summary>
        /// <param name="errorCode">To get the error code in case of any error in database</param>
        /// <param name="errorDesc">To get the error message in case of any error in database</param>
        /// <returns></returns>
        public List<IvrCallDataInfo> GetIvrCallData(out int errorCode, out string errorDesc)
        {
            object[] outParamList = new object[0];

            List<SqlParameter> paramList = new List<SqlParameter>();

            paramList.Add(_sqlDb.CreateParameter("@o_ErrorCode", SqlDbType.Int, ParameterDirection.Output));
            paramList.Add(_sqlDb.CreateParameter("@o_ErrorDescription", SqlDbType.VarChar, 200, ParameterDirection.Output));
            List<IvrCallDataInfo> callDataInfo = _sqlDb.ExecuteData<IvrCallDataInfo>(ConfigurationManager.AppSettings["DataGetProcedureName"].ToString(), CommandType.StoredProcedure, paramList, out outParamList);
            errorCode = Convert.ToInt32(Convert.ToString(outParamList[0]));
            errorDesc = Convert.ToString(outParamList[1]);

            return callDataInfo;
        }

        /// <summary>
        /// To import the parsed call data into reporting tables
        /// </summary>
        /// <param name="procedureName">Ivr application specific call data imporation procedure name (retreived from configuration settings)</param>
        /// <param name="dicParams">Ivr call data details</param>
        /// <param name="importStatus">To get the import status. It will be IMPORTED/FAILED</param>
        /// <param name="errorCode">To get the error code in case of any error in database</param>
        /// <param name="errorDesc">To get the error message in case of any error in database</param>
        public void ImportIvrCallData(string procedureName, Dictionary<string, string> dicParams, out string importStatus, out int errorCode, out string errorDesc)
        {
            importStatus = string.Empty;
            errorCode = 0;
            errorDesc = string.Empty;
            //
            object[] outParamList = new object[0];

            List<SqlParameter> paramList = new List<SqlParameter>();
            
            paramList.Add(_sqlDb.CreateParameter("@i_CallID", SqlDbType.VarChar, 200, ParameterDirection.Input, dicParams["CALL_ID"]));
            paramList.Add(_sqlDb.CreateParameter("@i_SessionID", SqlDbType.VarChar, 200, ParameterDirection.Input, dicParams["SESSION_ID"]));
            paramList.Add(_sqlDb.CreateParameter("@i_ApplicationID", SqlDbType.VarChar, 10, ParameterDirection.Input, dicParams["APP_ID"]));
            paramList.Add(_sqlDb.CreateParameter("@i_CallData", SqlDbType.Xml, 0, ParameterDirection.Input, dicParams["CALL_DATA"]));
            paramList.Add(_sqlDb.CreateParameter("@i_ReportData", SqlDbType.Xml, 0, ParameterDirection.Input, dicParams["REPORT_DATA"]));
            paramList.Add(_sqlDb.CreateParameter("@o_ImportStatus", SqlDbType.VarChar, 50, ParameterDirection.Output));
            paramList.Add(_sqlDb.CreateParameter("@o_ErrorCode", SqlDbType.Int, ParameterDirection.Output));
            paramList.Add(_sqlDb.CreateParameter("@o_ErrorDescription", SqlDbType.VarChar, 200, ParameterDirection.Output));

            _sqlDb.ExecuteNonQuery(procedureName, CommandType.StoredProcedure, paramList, out outParamList);

            importStatus = Convert.ToString(outParamList[0]);
            errorCode = Convert.ToInt32(Convert.ToString(outParamList[1]));
            errorDesc = Convert.ToString(outParamList[2]);
        }

        /// <summary>
        /// To update the ivr call data status into database (CALL DATA MASTER BACKUP TABLE)
        /// </summary>
        /// <param name="dicParams">Ivr call data details</param>
        /// <param name="errorCode">To get the error code in case of any error in database</param>
        /// <param name="errorDesc">To get the error message in case of any error in database</param>
        public void UpdateIvrCallDataStatus(Dictionary<string, string> dicParams, out int errorCode, out string errorDesc)
        {
            errorCode = 0;
            errorDesc = string.Empty;

            object[] outParamList = new object[0];

            List<SqlParameter> paramList = new List<SqlParameter>();
            
            paramList.Add(_sqlDb.CreateParameter("@i_CallID", SqlDbType.VarChar, 200, ParameterDirection.Input, dicParams["CALL_ID"]));
            paramList.Add(_sqlDb.CreateParameter("@i_SessionID", SqlDbType.VarChar, 200, ParameterDirection.Input, dicParams["SESSION_ID"]));
            paramList.Add(_sqlDb.CreateParameter("@i_ApplicationID", SqlDbType.VarChar, 10, ParameterDirection.Input, dicParams["APP_ID"]));
            paramList.Add(_sqlDb.CreateParameter("@i_CallDateTime", SqlDbType.VarChar, 10, ParameterDirection.Input, dicParams["CALL_DATETIME"]));
            paramList.Add(_sqlDb.CreateParameter("@i_CallData", SqlDbType.Xml, 0, ParameterDirection.Input, dicParams["CALL_DATA"]));
            paramList.Add(_sqlDb.CreateParameter("@i_ReportData", SqlDbType.Xml, 0, ParameterDirection.Input, dicParams["REPORT_DATA"]));
            paramList.Add(_sqlDb.CreateParameter("@i_Status", SqlDbType.Char, 1, ParameterDirection.Input, dicParams["STATUS"]));
            paramList.Add(_sqlDb.CreateParameter("@i_ProcessStatus", SqlDbType.VarChar, 25, ParameterDirection.Input, dicParams["PROCESS_STATUS"]));
            paramList.Add(_sqlDb.CreateParameter("@i_ProcessFailureReason", SqlDbType.VarChar, 25, ParameterDirection.Input, dicParams["PROCESS_FAILUREREASON"]));
            paramList.Add(_sqlDb.CreateParameter("@o_ErrorCode", SqlDbType.Int, ParameterDirection.Output));
            paramList.Add(_sqlDb.CreateParameter("@o_ErrorDescription", SqlDbType.VarChar, 200, ParameterDirection.Output));

            _sqlDb.ExecuteNonQuery(dicParams["PROCNAME"].ToString(), CommandType.StoredProcedure, paramList, out outParamList);

            errorCode = Convert.ToInt32(Convert.ToString(outParamList[0]));
            errorDesc = Convert.ToString(outParamList[1]);
        }
    }
}
