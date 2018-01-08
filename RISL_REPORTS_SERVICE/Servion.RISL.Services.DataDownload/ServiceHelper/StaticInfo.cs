
namespace Servion.RISL.Services.DataDownload
{
    public static class StaticInfo
    {
        private static bool _canContinue = false;

        /// <summary>
        /// To hold the thread count in the download process
        /// </summary>
        public static long ThreadCount = 0;

        /// <summary>
        /// To indicate the service to continue/stop the data download process
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
    }
}
