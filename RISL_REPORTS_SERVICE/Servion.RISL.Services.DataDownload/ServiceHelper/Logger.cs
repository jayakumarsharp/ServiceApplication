using log4net;

namespace Servion.RISL.Services.DataDownload
{
    class Logger
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// For logging the information
        /// </summary>      
        public static ILog Log
        {
            get { return log; }
        }
    }
}
