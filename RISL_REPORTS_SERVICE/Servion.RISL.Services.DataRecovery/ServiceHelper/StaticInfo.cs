
namespace Servion.RISL.Services.DataRecovery
{
    public static class StaticInfo
    {
        private static bool _canContinue = false;
        private static string _applicationServer = string.Empty;
        private static string _applicationName = string.Empty;
        private static int _commandTimeout = 60;
        private static int _fileRecoveryMaxTime = 24;

        /// <summary>
        /// To hold the thread count in the upload process
        /// </summary>
        public static long ThreadCount = 0;

        /// <summary>
        /// To represent the second cycle folder name inside recovery folder
        /// </summary>
        public static string SecondCycleFolder = "SecondCycle";
        
        /// <summary>
        /// To represent the unrecoverable folder name inside recovery folder
        /// </summary>
        public static string UnRecoverableFolder = "UnRecoverable";

        /// <summary>
        /// To indicate the service to continue/stop the data recovery process
        /// </summary>
        public static bool CanContinue
        {
            get 
            { 
                return _canContinue; 
            }
            set 
            { 
                _canContinue = value; 
            }
        }

        /// <summary>
        /// Application Server IP/Name
        /// </summary>
        public static string ApplicationServer
        {
            get
            {
                return _applicationServer;
            }
            set
            {
                _applicationServer = value;
            }
        }

        /// <summary>
        /// Application Name
        /// </summary>
        public static string ApplicationName
        {
            get
            {
                return _applicationName;
            }
            set
            {
                _applicationName = value;
            }
        }

        /// <summary>
        /// Database Command Timeout in Seconds
        /// </summary>
        public static int CommandTimeout
        {
            get
            {
                return _commandTimeout;
            }
            set
            {
                _commandTimeout = value;
            }
        }

        /// <summary>
        /// Maximum allowed file recovery time in hours
        /// </summary>
        public static int FileRecoveryMaxTime
        {
            get
            {
                return _fileRecoveryMaxTime;
            }
            set
            {
                _fileRecoveryMaxTime = value;
            }
        }
    }
}
