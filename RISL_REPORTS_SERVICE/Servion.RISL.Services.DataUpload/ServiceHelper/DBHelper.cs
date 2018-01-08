using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Servion.CCA.ApplicationFramework.Data.Sql;
using System;

namespace Servion.Sso.Services.DataUpload
{
    class DBHelper
    {
        private SqlDatabase _sqlDb;

        public DBHelper(string connStr)
        {
            _sqlDb = new SqlDatabase(connStr);
            _sqlDb.CommandTimeout = StaticInfo.CommandTimeout * 1000;
        }

        public List<IvrCallDataInfo> GetIvrCallData(out int errorCode, out string errorDesc)
        {
            object[] outParamList = new object[0];

            List<SqlParameter> paramList = new List<SqlParameter>();
            
            List<IvrCallDataInfo> callDataInfo = _sqlDb.ExecuteData<IvrCallDataInfo>("PROC_GET_IVR_CALLDATA", CommandType.StoredProcedure, paramList, out outParamList);

            errorCode = Convert.ToInt32(Convert.ToString(outParamList[0]));
            errorDesc = Convert.ToString(outParamList[1]);

            return callDataInfo;
        }
    }
}
