
namespace Servion.RISL.Services.DataUpload
{
    public static class StaticInfo
    {
        private static int _commandTimeout = 60;
        private static bool _canContinue = false;
        private static string _applicationServer = string.Empty;
        private static string _applicationName = string.Empty;
        private static string _tempXmlPacketFolder = string.Empty;

        /// <summary>
        /// To hold the thread count in the upload process
        /// </summary>
        public static long ThreadCount = 0;

        /// <summary>
        /// To indicate the service to continue/stop the data upload process
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
        /// Temparory folder for storing call data xml packet
        /// </summary>
        public static string TempXmlPacketFolder
        {
            get
            {
                return _tempXmlPacketFolder;
            }
            set
            {
                _tempXmlPacketFolder = value;
            }
        }
    }
}
